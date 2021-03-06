using System;
using System.IO;
using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Client
{
    public class InteractUi
    {
        private const int  maxInput = 250;
        private char[] name = new char[maxInput];
        private int letterCount = 0;
        private Rectangle inputBox = new Rectangle(100,500,650,650);
        private Rectangle openImage = new Rectangle(670,730,80,80);
        private Rectangle imageBorder = new Rectangle(220,100,400,450);
        private Image imgIcon = Raylib.LoadImage(Path.GetFullPath("uploadPicture.png"));
        private Texture2D iconTexture;
        private Image img;
        private int key;
        private float dt;
        private Font font = Raylib.GetFontDefault();
        private int[] written = new int[maxInput];

        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawText([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int posX, int posY, int fontSize, Color color);
        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawTextRec(Font font, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, Rectangle rec, float fontSize, float spacing, bool wordWrap, Color tint);
        private Server activeServer;

        public InteractUi()
        {
            iconTexture = Raylib.LoadTextureFromImage(imgIcon);
        }
        public Server ActiveServer
        {
            get{return activeServer;}
            set{activeServer = value;}
        }
        public void ChatBox()
        {
            dt += Raylib.GetFrameTime();
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(),inputBox))
            {
                key = Raylib.GetKeyPressed();
                if (key >= 32 && key <= 255 && letterCount < maxInput && key != 0)
                {
                    written[letterCount] = key;
                    name[letterCount] = (char)key;
                    letterCount++;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_BACKSPACE) && dt > 0.1)
                {
                    letterCount--;
                    if (letterCount < 0)
                    {
                        letterCount = 0;
                    }
                    name[letterCount] = '\0';
                    dt = 0;
                }
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_ENTER))
                {
                    //FIX: 
                    if (name[0] == '!')
                    {
                        activeServer.SendMessage("COMMAND",new string(name).Replace("\0",string.Empty));
                    }
                    else
                    {
                        activeServer.SendMessage("MESSAGE",new string(name).Replace("\0",string.Empty));
                    }
                    for (int i = 0; i < name.Length; i++)
                    {
                        name[i] = '\0';
                        letterCount = 0;
                    }
                }
            }
            //Upload Picture function
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(),openImage) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                CommonOpenFileDialog fileExplorer = new CommonOpenFileDialog();
                fileExplorer.InitialDirectory = @"C:\Users\" +Environment.UserName + @"\Pictures";
                fileExplorer.Filters.Add(new CommonFileDialogFilter("", "*.PNG;*.JPG"));
                if (fileExplorer.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    activeServer.SendMessage("MESSAGE",imagePath: fileExplorer.FileName);
                }
            }
            Raylib.DrawRectangle((int)inputBox.x,(int)inputBox.y,(int)inputBox.height,(int)inputBox.width,Color.BLUE);
            iconTexture.height = 80;
            iconTexture.width = 80;
            Raylib.DrawTexture(iconTexture, 670, 730, Color.WHITE);
            DrawTextRec(font, new string(name),inputBox,16,1,true,Color.WHITE);
        }
        public void ServerListUI(List<Server> serverList)
        {
            int y = 10;
            foreach (Server server in serverList)
            {
                Raylib.DrawRectangle(10, y, 50, 50, Color.GRAY);
                Raylib.DrawTexture(server.ServerImageTexture, 10, y, Color.WHITE);
                if (ActiveServer != null)
                {
                    DrawText("Current Server: " + ActiveServer.serverName,950, 10, 16, Color.WHITE);   
                }
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(10, y, 50, 50)) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    activeServer = server;
                }
                y += 60;
            }
        }
    }
}