using System;
using Raylib_cs;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Server> serverList = new List<Server>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Server>));
            if (File.Exists("Servers.xml"))
            {
                serverList = LoadInstances(serverList, serializer);
            }
            Raylib.InitWindow(1200,800,"Janne's Chatt");
            serverList.Add(new Server());
            serverList[0].ip = "localhost";
            serverList[0].port = 9999;

            serverList[0].Join();
            InteractUi ui = new InteractUi();
            ui.SetActiveServer(serverList[0]);
            Thread timeTick = new Thread(()=>BackgroundTick(serverList, serializer));
            timeTick.Start();
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DARKGRAY);
                ui.ChatBox();
                serverList[0].PrintMessages();
                new Notification().NotificationPopup("");
                Raylib.EndDrawing();
            }
        }

        //for anything that needs to run at a delay in the background
        private static void BackgroundTick(List<Server> serverList, XmlSerializer serializer)
        {
            while (true)
            {
                Thread.Sleep(4000);
                SaveInstances(serverList, serializer);
            }
        }

        private static void SaveInstances(List<Server> serverList, XmlSerializer serializer)
        {
          //filestream closes safely with using statement. Open or creates file and serializes the list inputed in parameter.
          using (FileStream serverFile = File.Open("Servers.xml", FileMode.OpenOrCreate))
          {
              serializer.Serialize(serverFile, serverList);
          }
        }

        private static List<Server> LoadInstances(List<Server> serverList, XmlSerializer serializer)
        {
            //filestream closes with using statement. Opens file, deserialize it to List with tamagochis and returns it.
            using FileStream serverStream = File.OpenRead("Servers.xml");
            return (List<Server>)serializer.Deserialize(serverStream);
        }
    }
}
