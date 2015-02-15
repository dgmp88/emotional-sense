using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace emotion_viewer.cs
{
    
    class EmotionDetection
    {

        private MainForm form;
        private bool disconnected = false;
        private FPSTimer timer;
        private EmotionalResponse lastResponse;

        private EmotionalResponseArray currentArray = new EmotionalResponseArray();
        private MyHttpPost currentPost;

        private int postEveryNSecs = 3;
        private Stopwatch stopwatch;

        public EmotionDetection(MainForm form)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            this.form = form;
        }

        private bool DisplayDeviceConnection(bool state)
        { 
            if (state)
            {
                if (!disconnected) form.UpdateStatus("Device Disconnected");
                disconnected = true;
            }
            else
            {
                if (disconnected) form.UpdateStatus("Device Reconnected");
                disconnected = false;
            }
            return disconnected;
        }

        private void DisplayPicture(PXCMImage image)
        {
            PXCMImage.ImageData data;
            pxcmStatus sts = image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out data);
            if ( sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                form.DisplayBitmap(data.ToBitmap(0, image.info.width, image.info.height));
                timer.Tick("");
                image.ReleaseAccess(data);
            }
        }

        private void DisplayLocation(PXCMEmotion ft)
        {

            int onlyUseFirstFace = 0;
            PXCMEmotion.EmotionData[] arrData = new PXCMEmotion.EmotionData[form.NUM_EMOTIONS];
            if (ft.QueryAllEmotionData(onlyUseFirstFace, out arrData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                UpdateResponse(arrData);
                PostIfRequired();
                form.DrawLocation(arrData);
            }
        }

        private void UpdateResponse(PXCMEmotion.EmotionData[] data)
        {
            EmotionalResponse response = GetFirstEmotion(data);

            // OK, if we have a null response, check whether it should a not-null response.
            if (response == null)
            {
                if (lastResponse == null)
                {
                    return;
                } else 
                {
                    currentArray.addResponse(lastResponse);
                    lastResponse = null;
                }
            }
            else
            {
                if (lastResponse == null)
                {
                    lastResponse = response;
                }
                else if (!response.Equals(lastResponse))
                {
                    currentArray.addResponse(response);
                    lastResponse = response;
                }
            }
        }

        private void PostIfRequired()
        {

            if (stopwatch.Elapsed.Seconds > postEveryNSecs)
            {
                if (currentPost != null)
                {
                    if (currentPost.done)
                    {
                        PostData();
                    }
                }
                else
                {
                    PostData();
                }
            }
        }

        private void PostData()
        {
            if (currentArray.Count() > 0)
            {
                currentPost = new MyHttpPost(currentArray.ToJson());
                currentArray = new EmotionalResponseArray();
                stopwatch.Restart();
            }
        }



        private EmotionalResponse GetFirstEmotion(PXCMEmotion.EmotionData[] data)
        {
            bool emotionPresent = false;
            int epidx = -1; int maxscoreE = -3; float maxscoreI = 0;
            string emotionLabel = null, sentimentLabel = null;

            if (data[0] == null)
            {
                return null;
            }

            try
            {
                for (int i = 0; i < form.NUM_PRIMARY_EMOTIONS; i++)
                {
                    if (data[i].evidence < maxscoreE) continue;
                    if (data[i].intensity < maxscoreI) continue;
                    maxscoreE = data[i].evidence;
                    maxscoreI = data[i].intensity;
                    epidx = i;
                }
                if ((epidx != -1) && (maxscoreI > 0.4))
                {
                    emotionLabel = form.EmotionLabels[epidx];
                    emotionPresent = true;
                }

                int spidx = -1;
                if (emotionPresent)
                {
                    maxscoreE = -3; maxscoreI = 0;
                    for (int i = 0; i < (form.NUM_EMOTIONS - form.NUM_PRIMARY_EMOTIONS); i++)
                    {
                        if (data[form.NUM_PRIMARY_EMOTIONS + i].evidence < maxscoreE) continue;
                        if (data[form.NUM_PRIMARY_EMOTIONS + i].intensity < maxscoreI) continue;
                        maxscoreE = data[form.NUM_PRIMARY_EMOTIONS + i].evidence;
                        maxscoreI = data[form.NUM_PRIMARY_EMOTIONS + i].intensity;
                        spidx = i;
                    }
                    if ((spidx != -1))
                    {
                        sentimentLabel = form.SentimentLabels[spidx];
                    }

                    EmotionalResponse response = new EmotionalResponse();
                    response.emotion = emotionLabel;
                    response.sentiment = sentimentLabel;
                    response.intensity = maxscoreI;
                    response.evidence = maxscoreE;
                    return response;
                }

            }
            catch (NullReferenceException e)
            {
            }
            return null;
        }

        // Handler functions
        private int profileIndex;
        public pxcmStatus OnModuleQueryProfile(Int32 mid, PXCMBase obj, Int32 pidx)
        {
            return pidx == profileIndex ? pxcmStatus.PXCM_STATUS_NO_ERROR : pxcmStatus.PXCM_STATUS_PARAM_UNSUPPORTED;
        }

        public void SimplePipeline()
        {
            bool sts = true;
            PXCMSenseManager pp = form.session.CreateSenseManager();
            if (pp == null) throw new Exception("Failed to create sense manager");
            disconnected = false;

            /* Set Source & Profile Index */
            PXCMCapture.DeviceInfo info = null;
            if (this.form.GetRecordState())
            {
                pp.captureManager.SetFileName(this.form.GetFileName(), true);
                form.PopulateDeviceMenu();
                if (this.form.Devices.TryGetValue(this.form.GetCheckedDevice(), out info))
                {
                    pp.captureManager.FilterByDeviceInfo(info);
                }
            }
            else if (this.form.GetPlaybackState())
            {
                pp.captureManager.SetFileName(this.form.GetFileName(), false);
            }
            else
            {
                if (this.form.Devices.TryGetValue(this.form.GetCheckedDevice(), out info))
                {
                    pp.captureManager.FilterByDeviceInfo(info);
                }
            }

            /* Set Module */
            pp.EnableEmotion(form.GetCheckedModule());

            /* Initialization */
            form.UpdateStatus("Init Started");

            PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler()
            {
               //GZ onModuleQueryProfile = OnModuleQueryProfile
            };

            if (pp.Init(handler) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                form.UpdateStatus("Streaming");
                this.timer = new FPSTimer(form);
                PXCMCaptureManager captureManager = pp.QueryCaptureManager();
                if (captureManager == null) throw new Exception("Failed to query capture manager");
                PXCMCapture.Device device = captureManager.QueryDevice();

                if (device != null && !this.form.GetPlaybackState())
                    device.SetDepthConfidenceThreshold(7);
                    //GZ device.SetProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_CONFIDENCE_THRESHOLD, 7);                

                while (!form.stop)
                {
                    if (pp.AcquireFrame(true) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    if (!DisplayDeviceConnection(!pp.IsConnected()))
                    {
                        /* Display Results */
                        PXCMEmotion ft = pp.QueryEmotion();
                        if (ft == null)
                        {
                            pp.ReleaseFrame();
                            continue;
                        }

                        //GZ DisplayPicture(pp.QueryImageByType(PXCMImage.ImageType.IMAGE_TYPE_COLOR));
                        PXCMCapture.Sample sample = pp.QueryEmotionSample();
                        if (sample == null)
                        {
                            pp.ReleaseFrame();
                            continue;
                        }

                        DisplayPicture(sample.color);

                        DisplayLocation(ft);

                        form.UpdatePanel();
                    }
                    pp.ReleaseFrame();
                }
            }
            else
            {
                form.UpdateStatus("Init Failed");
                sts = false;
            }

            pp.Close();
            pp.Dispose();
            if (sts) form.UpdateStatus("Stopped");
        }
    }
}
