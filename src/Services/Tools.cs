using System;
using Microsoft.AspNet.Http;
using System.Linq;
using L2PAPIClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Grp.L2PSite.MobileApp.Services
{
    public static class Tools
    {

        public static String pWd = "Integr$atorPa%ss123!@#$5";
        public static Boolean hasCookieToken = false;

        public static void getAndSetUserToken(IReadableStringCollection cookies, HttpContext context){

            hasCookieToken = false;
            var cookiesList = from cookieToken in cookies
                         where cookieToken.Key == "CRTID" || cookieToken.Key == "CRAID"
                         select cookieToken;
            if (cookiesList.Any())
            {
                foreach(var cookie in cookiesList)
                {
                    if (cookie.Key == "CRTID")
                        Config.setRefreshToken(Encryptor.Decrypt(cookie.Value.First()));
                    else
                        Config.setAccessToken(Encryptor.Decrypt(cookie.Value.First()));         
                }
                if (cookiesList.Count() == 2)
                {
                    hasCookieToken = true;
                    context.Session.Set("LoggedIn", Tools.ObjectToByteArray(LoginStatus.LoggedIn));
                }
                else
                {
                    context.Session.Set("LoggedIn", Tools.ObjectToByteArray(LoginStatus.Waiting));
                }
            }
            else
            {
                context.Session.Set("LoggedIn", Tools.ObjectToByteArray(LoginStatus.Waiting));
            }
        }

        public static bool isUserLoggedInAndAPIActive(HttpContext context)
        {
            LoginStatus login_status = (LoginStatus)Tools.ByteArrayToObject(context.Session["LoggedIn"]);
            bool l2pStatus = AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE;
            return  (login_status.Equals(LoginStatus.LoggedIn) && l2pStatus);
        }

        public static String formatSemesterCode(String code)
        {
            return code.Replace("ss", "Summer Semester ").Replace("ws", "Winter Semester ");
        }

        public static String truncateString(String str, int truncateLength)
        {
            if(str.Length >= truncateLength)
            {
                return str.Substring(0, truncateLength);
            }
            return str;
        }

        public static String toDateTime(double dateNb)
        {
            DateTime date = new DateTime(1970, 1, 1);
            date = date.AddDays(Math.Floor(dateNb / 60 / 60 / 24));
            return date.ToShortDateString();
        }

        public static bool checkURLValidity(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            if (arrBytes == null)
            {
                return null;
            }

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            object obj = (object)binForm.Deserialize(memStream);
            return obj;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public enum LoginStatus:int
        {
            Waiting = -1,
            LoggedOff = 0,
            LoggedIn = 1
                 
        };

        public static T DeserializeObject<T>(Object obj)
        {
            if(obj != null)
            {
                return JsonConvert.DeserializeObject<T>(obj.ToString());
            }
            return default(T);
        }

        public static string ToFileSize(long size)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", size);
        }

        public static string getImagePathByFileName(string fileName)
        {
            if (fileName != null)
            {
                Match match = Regex.Match(fileName, ".pdf", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return "../images/learning_material/PDF.png";
                }
                else if (Regex.Match(fileName, ".png|.jpg|.jpeg|.gif|.bmp", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/Full_Image.png";
                }
                else if (Regex.Match(fileName, ".3g2|.3gp|.asf|.asx|.avi|.flv|.mov|.mp4|.mpg|.rm|.swf|.vob|.wmv", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/Video_Message.png";
                }
                else if (Regex.Match(fileName, ".doc|.docx|.rtf", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/MS_Word.png";
                }
                else if (Regex.Match(fileName, ".log|.txt|.wpd|.wps", RegexOptions.IgnoreCase).Success)
                {
                    return "images/learning_material/Text_Document.png";
                }
                else if (Regex.Match(fileName, ".csv|.xls|.xlsx", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/MS_Excel.png";
                }
                else if (Regex.Match(fileName, ".ppt|.pptx|.pps", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/MS_PowerPoint.png";
                }
                else if (Regex.Match(fileName, ".zip|.rar|.7z|.tar.gz", RegexOptions.IgnoreCase).Success)
                {
                    return "../images/learning_material/ZIP.png";
                }
                else
                {
                    return "../images/learning_material/File.png";
                }
            }
            return "../images/learning_material/File.png";
        }
    }
}
