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
        public string name;
        public string messageText = "";
        public string image = "";
        private Texture2D rayImage;
        public string color;

        public Texture2D GetImage()
        {
            return rayImage;
        }
        public void SetImage(Texture2D rayImage)
        {
            this.rayImage = rayImage;
        }
    }
}
