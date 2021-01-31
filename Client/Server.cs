using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Raylib_cs;

namespace Client
{
    public class Server
    {
        public InteractUi ui;
        public string ip;
        public int port;
        public string username;
        public byte[] password;

        //These may need to change to update password, I read that these shouldn't be the same? But it keeps complaining without them being the same.
        byte[] decryptedKey = Encoding.UTF8.GetBytes("r4u7x!A%D*G-KaPd");
        byte[] decryptedIV = Encoding.UTF8.GetBytes("r4u7x!A%D*G-KaPd");
        string serverImage;
        string nameColor;

        TcpClient client;
        List<string> messages = new List<string>();

        private bool liveChat = true;

        [DllImport(Raylib.nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawText([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int posX, int posY, int fontSize, Color color);
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
            password = Encrypt(newPassword, decryptedKey, decryptedIV);
        }
        public string GetPassword()
        {
            return Decrypt(password, decryptedKey, decryptedIV);
        }
        private byte[] Encrypt(string decryptedString, byte[] key, byte[] IV)
        {
            byte[] decryptedStringBytes = Encoding.UTF8.GetBytes(decryptedString);
            byte[] encrypted;
            System.Console.WriteLine(IV.Length);
            //aes handles keys, ivs, encryptors etc.
            using (Aes aes = new AesManaged())
            {
                aes.IV = key;
                aes.Key = IV;
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

         public void Join()
        {
            try
            {
                client = new TcpClient(ip, port);
                string password = GetPassword();
                Byte[] credentialsData = Encoding.UTF8.GetBytes($"{username},{password}");
                NetworkStream stream = client.GetStream();
                try
                {
                    stream.Write(credentialsData, 0, credentialsData.Length);

                    try
                    {
                        /*this while loop uses stream.Read to get data from the server, the stream we are using is the server and by reading it
                            * we can get the bytes coming from the stream and then we can use the GetString to convert bytes to a string, which we then
                            * print to the user
                        */
                        byte[] data2 = new byte[256];
                        string responeseData = string.Empty;
                        int bytes = stream.Read(data2, 0, data2.Length);
                        System.Console.WriteLine("hello");
                        responeseData = System.Text.Encoding.UTF8.GetString(data2, 0, bytes);
                        if (responeseData == "yes")
                        {
                            Thread liveChat = new Thread(() => LiveChat());
                            liveChat.Start();
                            return;
                        }
                        else if (responeseData == "no")
                        {
                            new Notification().NotificationPopup("Invalid credentials, removing server");
                            return;
                        }
                        System.Console.WriteLine("man");
                    }
                    //if the server closes down for some reason, it will deliver this message to you and return you to the start.
                    catch (Exception)
                    {
                        Console.WriteLine("It looks the the connection with the server broke");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("It looks the the connection with the server broke");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect, doublecheck the ip/port and try again");
            }
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
                while (liveChat)
                {
                    byte[] data2 = new byte[256];
                    string responeseData = string.Empty;
                    int bytes = stream.Read(data2, 0, data2.Length);
                    responeseData = System.Text.Encoding.UTF8.GetString(data2, 0, bytes);
                    System.Console.WriteLine(responeseData);
                    messages.Add(responeseData);
                }
            }
            //if the server closes down for some reason, it will deliver this message to you and return you to the start.
            catch (Exception)
            {
                Console.WriteLine("It looks the the connection with the server broke");
            }
        }

        public void PrintMessages()
        {
            int x = 100;
            int y = 450;
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                DrawText(messages[i],x,y,16,Color.WHITE);
                y -= 15;
            }
        }

        //Method takes a string and sends it to server
        public void SendMessage(string message)
        {
            try
            {
                Byte[] messageByte = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(messageByte, 0, messageByte.Length);
            }
            catch (Exception)
            {
                 System.Console.WriteLine("It looks the the connection with the server broke");
            }
        }
    }

}