using System;
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
        const int  maxInput = 250;
        char[] name = new char[maxInput];
        int letterCount = 0;
        Rectangle inputBox = new Rectangle(100,500,650,650);
        Rectangle openImage = new Rectangle(670,730,80,80);
        Rectangle imageBorder = new Rectangle(220,100,400,450);
        Image imgIcon = Raylib.LoadImage("uploadPicture.png");
        Texture2D iconTexture;
        Image img;
        int key;
        float dt;
        Font font = Raylib.GetFontDefault();
        int[] written = new int[maxInput];

        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawText([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int posX, int posY, int fontSize, Color color);
        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawTextRec(Font font, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, Rectangle rec, float fontSize, float spacing, bool wordWrap, Color tint);
        public Server activeServer;

        public InteractUi()
        {
            iconTexture = Raylib.LoadTextureFromImage(imgIcon);
        }

        public void SetActiveServer(Server activeServer)
        {
            this.activeServer = activeServer;
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
                    activeServer.SendMessage("MESSAGE",new string(name).Replace("\0",string.Empty));
                    for (int i = 0; i < name.Length; i++)
                    {
                        name[i] = '\0';
                        letterCount = 0;
                    }
                }
            }
            //Upload Picture function
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(),openImage))
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    CommonOpenFileDialog fileExplorer = new CommonOpenFileDialog();
                    fileExplorer.InitialDirectory = @"C:\Users\" +Environment.UserName + @"\Pictures";
                    fileExplorer.Filters.Add(new CommonFileDialogFilter("", "*.PNG;*.JPG"));
                    if (fileExplorer.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        activeServer.SendMessage("MESSAGE",imagePath: fileExplorer.FileName);
                    }
                }
            }
            Raylib.DrawRectangle((int)inputBox.x,(int)inputBox.y,(int)inputBox.height,(int)inputBox.width,Color.BLUE);
            iconTexture.height = 80;
            iconTexture.width = 80;
            Raylib.DrawTexture(iconTexture, 670, 730, Color.WHITE);
            DrawTextRec(font, new string(name),inputBox,16,1,true,Color.WHITE);
        }
    }
}