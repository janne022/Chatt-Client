using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Raylib_cs;
using System.Runtime.Serialization.Json;

namespace Client
{
    public class Server
    {
        public InteractUi ui;
        public string ip;
        public int port;
        private Raylib_cs.Image img = new Raylib_cs.Image();
        private Raylib_cs.Image previousImg = new Raylib_cs.Image();
        private Texture2D imageTexture = new Texture2D();
        public string username;
        public byte[] password;

        //These may need to change to update password, I read that these shouldn't be the same? But it keeps complaining without them being the same.
        private readonly byte[] decryptedKey = Encoding.UTF8.GetBytes("r4u7x!A%D*G-KaPd");
        private readonly byte[] decryptedIV = Encoding.UTF8.GetBytes("r4u7x!A%D*G-KaPd");
        private string serverImagePath;
        Raylib_cs.Texture2D serverImageTexture;
        public Texture2D ServerImageTexture
        {
            get {return serverImageTexture;}
            set {serverImageTexture = value;}
        }
        public string ServerImagePath
        {
            get {return serverImagePath;}
            set {
                    serverImagePath = value;
                    Raylib_cs.Image serverImage = Raylib.LoadImage(value);
                    serverImageTexture = Raylib.LoadTextureFromImage(serverImage);
                    //POSSIBLE TODO: scale height + width according to algorithm, instead of constant
                    serverImageTexture.height = 50;
                    serverImageTexture.width = 50;
                }
        }
        string nameColor;
        TcpClient client;
        List<Message> messages = new List<Message>();

        private bool liveChat = true;
        NetworkStream stream;

        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawText([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int posX, int posY, int fontSize, Raylib_cs.Color color);
        public void SetPortIp(string ip, int port)
        {
            //This method takes both ip and port, since the two don't have any special rules and because they are interconnected they can be set together
            //Port and ip validity should be handled by the Join class.
            this.port = port;
            this.ip = ip;
        }
        public void SetUsername(string username)
        {
            //Sets username in class. This does not take any action but to trim the string. Server should handle username violations.
            this.username = username.Trim();
        }
        public void SetPassword(string newPassword)
        {

            using (AesManaged myAes = new AesManaged())
            {
                password = Encrypt(newPassword, myAes.Key, myAes.IV);
                System.Console.WriteLine(myAes.Key + " encrypt");
            }
        }
        public string GetPassword()
        {
            using (AesManaged myAes = new AesManaged())
            {
                return Decrypt(password, myAes.Key, myAes.IV);
            }

        }
        public byte[] Encrypt(string decryptedString, byte[] key, byte[] IV)
        {
            byte[] decryptedStringBytes = Encoding.UTF8.GetBytes(decryptedString);
            byte[] encrypted;
            System.Console.WriteLine(IV.Length);
            //aes handles keys, ivs, encryptors etc.
            using (Aes aes = new AesManaged())
            {
                aes.IV = IV;
                aes.Key = key;
                ICryptoTransform encryptor = aes.CreateEncryptor();

                //reads and write to memory, used as buffer
                using (MemoryStream streamEncrypt = new MemoryStream())
                {
                    //uses previous stream together with the aes encryptor to create a cryptopstream, where we can encrypt string
                    using(CryptoStream cryptoStream = new CryptoStream(streamEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        //finally, streamwriter takes the cryptostream and writes the string password into memory (cryptostream)
                        using(StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(decryptedString);
                        }
                        //here it takes the data from the buffer, in this case the password generate using the cryptostream and streamwriter
                        //and stores that in a byte array
                        encrypted = streamEncrypt.ToArray();
                    }
                }

            }
            return encrypted;
        }

        private string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {

            string plaintext;

            //aes handles keys, ivs, encryptors etc.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                //reads and write to memory, used as buffer
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    //uses previous stream together with the aes encryptor to create a cryptopstream, where we can decrypt a byte array
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

         public bool Join()
        {
            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
                SendMessage("LOGIN", "Janne,programmering");
                /*this while loop uses stream.Read to get data from the server, the stream we are using is the server and by reading it
                    * we can get the bytes coming from the stream and then we can use the GetString to convert bytes to a string, which we then
                    * print to the user
                */
                byte[] data2 = new byte[256];
                string responeseData = string.Empty;
                int bytes = stream.Read(data2, 0, data2.Length);
                responeseData = System.Text.Encoding.UTF8.GetString(data2, 0, bytes);
                Message newMessage = JsonConvert.DeserializeObject<Message>(responeseData);
                if (newMessage.messageText == "yes")
                {
                    Thread liveChat = new Thread(() => LiveChat());
                    liveChat.Start();
                    return true;
                }
                else if (newMessage.messageText == "no")
                {
                    Thread liveChat = new Thread(() => LiveChat());
                    liveChat.Start();
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect, doublecheck the ip/port and try again");
            }
            return false;
        }

        public void Leave()
        {
            liveChat = false;
        }

        private void LiveChat()
        {
            try
            {
                /*this while loop uses stream.Read to get data from the server, the stream we are using is the server and by reading it
                    * we can get the bytes coming from the stream and then we can use the GetString to convert bytes to a string, which we then
                    * print to the user
                */
                NetworkStream stream = client.GetStream();
                byte[] data2 = new byte[512];
                while (liveChat)
                {
                    string responeseData = string.Empty;
                    int bytes = stream.Read(data2, 0, data2.Length);
                    responeseData = System.Text.Encoding.UTF8.GetString(data2, 0, bytes);
                    System.Console.WriteLine(responeseData);
                    Message newMessage = JsonConvert.DeserializeObject<Message>(responeseData);
                    if (newMessage.header == "MESSAGELENGTH")
                    {
                        data2 = new byte[newMessage.length + 256];
                    }
                    if (newMessage.image != string.Empty)
                    {
                        byte[] imageData = Convert.FromBase64String(newMessage.image);
                        string imageDataRaw = Encoding.UTF8.GetString(imageData);
                        using (MemoryStream imageStream = new MemoryStream(imageData))
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(imageStream,true, true);
                            image.Save("file.png", ImageFormat.Png);
                            img = Raylib.LoadImage("file.png");
                            File.Delete("file.png");
                        }
                    }
                    messages.Add(newMessage);
                }
                //if the server closes down for some reason, it will deliver this message to you and return you to the start.
            }
            catch (Exception)
            {
                stream.Close();
                System.Console.WriteLine("Connection with the server broke!");
                return;
            }
        }

        //prints messages sent by server
        //FIX: PRINT MULTIPLE IMAGES, AND IN VERTICAL ARRAY (PRINTS IMAGES, BUT IS NOT STABLE)
        //TODO: PRINT USERNAME WITH MESSAGE
        public void PrintMessages()
        {
            int x = 100;
            int y = 450;
            int xImage = 150;
            int yImage = 500;
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                if (messages[i].messageText != string.Empty)
                {
                    DrawText(messages[i].name+ ": " + messages[i].messageText,x,y,16, Raylib_cs.Color.WHITE);
                    y -= 15;
                    yImage -= 15;
                }
                if (messages[i].image != string.Empty)
                {
                    //FIX: FIRST IMAGE DOES NOT LOAD, instead do bool to toggle if new picture, then turn of bool
                    if (img.data != previousImg.data)
                    {
                        System.Console.WriteLine("loaded image");
                        imageTexture = Raylib.LoadTextureFromImage(img);
                        messages[i].SetImage(imageTexture);
                        previousImg = img;
                    }
                    Texture2D currentImage = messages[i].GetImage();
                    double ratio = 0;
                    if (currentImage.width > currentImage.height && currentImage.width > 500)
                    {
                        ratio = currentImage.width/500;
                        currentImage.width = 500;
                        currentImage.height = (int)(currentImage.height/ratio);
                    }
                    else if(currentImage.height > 500)
                    {
                        ratio = currentImage.height/500;
                        currentImage.height = 500;
                        currentImage.width = (int)(currentImage.width/ratio);
                    }
                    yImage -= currentImage.height + 15;
                    y -= currentImage.height + 15;
                    DrawText(messages[i].name+ ": ",x,yImage,16, Raylib_cs.Color.WHITE);
                    Raylib.DrawTexture(currentImage, xImage, yImage, Raylib_cs.Color.WHITE);
                }
            }
        }

        //Method takes a string to send as message, also takes imagepath and sends as base64
        public void SendMessage(string header, string message = "", string imagePath = "")
        {
            Message newMessage = new Message();
            Message messageLength = new Message();
            //Add Image
            if (imagePath != "")
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
                using (MemoryStream imageStream = new MemoryStream())
                {
                    image.Save(imageStream, image.RawFormat);
                    byte[] imageArray = imageStream.ToArray();
                    newMessage.image = Convert.ToBase64String(imageArray);
                }
            }
            //Add Message
            newMessage.messageText = message;
            //Add Header
            newMessage.header = header;
            //Send
            string json = JsonConvert.SerializeObject(newMessage);
            System.Console.WriteLine(json);
            //Send messageLength first
            messageLength.header = "MESSAGELENGTH";
            messageLength.length = Encoding.UTF8.GetByteCount(json);
            string jsonLength = JsonConvert.SerializeObject(messageLength);
            stream.Write(Encoding.UTF8.GetBytes(jsonLength), 0, Encoding.UTF8.GetBytes(jsonLength).Length);
            //send message
            stream.Write(Encoding.UTF8.GetBytes(json), 0, Encoding.UTF8.GetBytes(json).Length);
        }
    }

}