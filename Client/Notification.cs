using System;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Client
{
    public class Notification
    {
        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawTextRec(Font font, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, Rectangle rec, float fontSize, float spacing, bool wordWrap, Color tint);
        Font font = Raylib.LoadFont(@"");
        public void NotificationPopup(string notificationMessage)
        {
            Rectangle notifBubble = new Rectangle(850,700,500,100);
            Raylib.DrawRectangle(850,700, 500, 100, Color.BLACK);
            DrawTextRec(font,notificationMessage,notifBubble,16,1,true,Color.WHITE);
        }
    }
}
