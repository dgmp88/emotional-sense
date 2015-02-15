using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace emotion_viewer.cs
{
    class EmotionalResponse
    {
        private string startTime = null, endTime = null;

        public float intensity { get; set; }
        public int evidence { get; set; }
        public string emotion { get; set; }
        public string sentiment {get; set;}

      //private string[] EmotionLabels = {"ANGER","CONTEMPT","DISGUST","FEAR","JOY","SADNESS","SURPRISE"};
      //private string[] SentimentLabels = {"NEGATIVE","POSITIVE","NEUTRAL"};
        
        public EmotionalResponse()
        {
            startTime = DateTime.Now.ToString("ddMMyyyyHHmmssfff");
        }

        public void endEmotion()
        {
            endTime = DateTime.Now.ToString("ddMMyyyyHHmmssfff");
        }

        public bool Equals(EmotionalResponse e)
        {
            // If parameter is null return false:
            if (e == null)
            {
                return false;
            }

            if (e.endTime != null || endTime != null)
            {
                return false;
            }

            return e.emotion.Equals(emotion) && e.sentiment.Equals(sentiment);
        }

        public override string ToString()
        {
            return emotion + " " + sentiment + " " + intensity.ToString() + " " + evidence.ToString();
        }

        internal string getJson()
        {

            return "{\"emotion\":{" +
               "\"mood\":\"" + emotion + "\"," +
               "\"type\":\"" + sentiment + "\"}," + 
               "\"timestampBegin\":\"" + startTime + "\"," +
               "\"timestampEnd\":\"" + endTime + "\"}";
        }
    }
}
