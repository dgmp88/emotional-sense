using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace emotion_viewer.cs
{
    class EmotionalResponseArray
    {
        List<EmotionalResponse> responses;

        public EmotionalResponseArray()
        {
            responses = new List<EmotionalResponse>();
        }

        public void addResponse(EmotionalResponse response)
        {
            response.endEmotion();
            responses.Add(response);
        }

        internal string ToJson()
        {
            string json;
            if (responses.Count > 0)
            {
                json = "[";
                for (int i = 0; i < responses.Count - 1; i++)
                {
                    json += responses[i].getJson() + ",";
                }
                json += responses[responses.Count - 1].getJson() + "]";
            }
            else
            {
                json = "{}";
            }
            return json;
        }

        public int Count()
        {
            return responses.Count;
        }
    }
}
