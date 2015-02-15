using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace emotion_viewer.cs
{
    class MyHttpPost
    {
        private long startTime = -1, endTime = -1;

        public string url = "http://real-emo.herokuapp.com/emotions"; //
        //public string url = "http://www.posttestserver.com/post.php/";
        public string data {get; set;}
        public bool done { get; set; }
        public string result { get; set; }

        WebClient wc;

        public MyHttpPost(string data)
        {
            this.data = data;
            startTime = Stopwatch.GetTimestamp();
            System.Diagnostics.Debug.WriteLine("Posting: " + data);
            Post(data);
        }


        private void Post(string data)
        {
            wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers["Content-Type"] = "application/json";
           // wc.DownloadStringCompleted += wc_UploadStringCompleted;
           // wc.DownloadStringAsync(new Uri(url), data);

            wc.UploadStringCompleted += wc_UploadStringCompleted;
            wc.UploadStringAsync(new Uri(url), "POST", data);

            //wc.UploadStringAsync(new Uri(url), data);
          //  wc.UploadDataAsync(new Uri(url), "POST", System.Text.Encoding.UTF8.GetBytes(data));
        }

        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            result = e.Result.ToString();
            System.Diagnostics.Debug.WriteLine("Response: " + result);
            done = true;
            endTime = Stopwatch.GetTimestamp();
        }
    }
}
