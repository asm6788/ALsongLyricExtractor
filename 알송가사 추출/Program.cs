using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace 알송가사_추출
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("mp3파일을 실행파일에 드래그 해주세요");
                Console.ReadLine();
                Environment.Exit(0);
            }
            int headerloc = 0;
            string musicmd5 = "";
            byte[] MusicByte = File.ReadAllBytes(args[0]);
            for (int i = 0; i != MusicByte.Length; i++)
            {
                if (ReadByte(MusicByte, i,3) == "ID3")
                {
                    headerloc = MusicByte[i + 6] << 21 | MusicByte[i + 7] << 14 | MusicByte[i + 8] << 7 | MusicByte[i + 9] + 10;
                    break;
                }
            }
            musicmd5 = MD5(ReadByteReturnByte(MusicByte, headerloc, 163840));
            Console.WriteLine(musicmd5);

            String callUrl = "http://lyrics.alsong.co.kr/alsongwebservice/service1.asmx";

            String postData = "<?xml version="+'\u0022'+ "1.0" + '\u0022'+ " encoding="+ '\u0022' + "UTF-8" + '\u0022'+ "?>"+ "<SOAP-ENV:Envelope  xmlns:SOAP-ENV="+'\u0022'+"http://www.w3.org/2003/05/soap-envelope"+'\u0022'+ " xmlns:SOAP-ENC="+'\u0022'+"http://www.w3.org/2003/05/soap-encoding"+'\u0022'+ " xmlns:xsi="+'\u0022'+"http://www.w3.org/2001/XMLSchema-instance"+'\u0022'+ " xmlns:xsd="+'\u0022'+"http://www.w3.org/2001/XMLSchema"+'\u0022'+ " xmlns:ns2="+'\u0022'+"ALSongWebServer/Service1Soap"+'\u0022'+ " xmlns:ns1="+'\u0022'+"ALSongWebServer"+'\u0022'+ " xmlns:ns3="+'\u0022'+"ALSongWebServer/Service1Soap12"+'\u0022'+ ">"+ "<SOAP-ENV:Body>"+ "<ns1:GetLyric5>"+ "<ns1:stQuery>"+ "<ns1:strChecksum>"+ musicmd5 + "</ns1:strChecksum>"+ "<ns1:strVersion>3.36</ns1:strVersion>"+ "<ns1:strMACAddress>00ff667f9a08</ns1:strMACAddress>"+ "<ns1:strIPAddress>xxx.xxx.xxx.xxx</ns1:strIPAddress>"+"</ns1:stQuery>"+ "</ns1:GetLyric5>"+ "</SOAP-ENV:Body>"+ "</SOAP-ENV:Envelope>";


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
            httpWebRequest.ContentType = "application/soap+xml; charset=UTF-8";
            httpWebRequest.UserAgent = "gSOAP/2.7";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(sendData, 0, sendData.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            string respone = streamReader.ReadToEnd();
           // Console.WriteLine(respone);
            string temp = respone.Remove(0, respone.IndexOf("strLyric") + 9);
            temp = temp.Remove(temp.IndexOf("strLyric") -2);
            temp = temp.Replace("&lt;br&gt;", "\r\n");
            temp = temp.Replace("[00:00.00]\r\n", "");
            Console.WriteLine(temp);
            Console.ReadLine();
        }

        static string ReadByte(byte[] buffer,int index,int count)
        {
            return Encoding.UTF8.GetString(buffer, index, count);
        }

        static byte[] ReadByteReturnByte(byte[] buffer, int index, int count)
        {
            byte[] temp = new byte[count];
            for(int i = 0; i != count;i++)
            {
                temp[i] = buffer[index+i];
            }
            return temp;
        }

        private static string MD5(byte[] bytes)
        {
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            mD5CryptoServiceProvider.ComputeHash(bytes);
            byte[] hash = mD5CryptoServiceProvider.Hash;
            StringBuilder stringBuilder = new StringBuilder();
            byte[] array = hash;
            for (int i = 0; i < array.Length; i++)
            {
                byte b = array[i];
                stringBuilder.Append(string.Format("{0:x2}", b));
            }
            return stringBuilder.ToString();
        }

    }
}
