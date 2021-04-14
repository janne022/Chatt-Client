using System;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Client
{
    public class Message
    {
        //uuid should be assigned by server and set for every message
        string uuid;
        public string messageText = "";
        
        public string image = "";
        string color;
    }
}
