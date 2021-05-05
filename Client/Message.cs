using System;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Client
{
    public class Message
    {
        //uuid should be assigned by server and set for every message
        public string header;
        public int length;
        public string uuid;
        public string messageText = "";
        public string image = "";
        private Image rayImage;
        public string color;

        public Image GetImage()
        {
            return rayImage;
        }
        public void SetImage(Image rayImage)
        {
            this.rayImage = rayImage;
        }
    }
}
