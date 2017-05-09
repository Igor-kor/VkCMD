using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using namesettings;

namespace namevkapi
{
    [Serializable]
    public class vkapi
    {
        public Settings setting;
        [NonSerialized()]
        private string tempRequest;
        [NonSerialized()]
        private string textRequest;
        [NonSerialized()]
        WebRequest request;
        [NonSerialized()]
        WebResponse response;
        [NonSerialized()]
        private string sig;

        //получение хэша
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        //для хэша
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return (0 == comparer.Compare(hashOfInput, hash));
        }

        //отправка запроса
        public string sendRequest(string method)
        {
            tempRequest = "/method/" + method + "&access_token=" + setting.accesToken + setting.secret;
            using (MD5 md5Hash = MD5.Create())
            {
                sig = GetMd5Hash(md5Hash, tempRequest);
                //Console.WriteLine("sig=" + sig);
            }
            textRequest = "https://api.vk.com/method/" + method + "&access_token=" + setting.accesToken + "&sig=" + sig;
            //костыль, ибо + не конвертирует в %2B для передачи в url, возможно есть еще мешающие символы
            textRequest = textRequest.Replace("+", "%2B");
            request = WebRequest.Create(textRequest);
            request.Credentials = CredentialCache.DefaultCredentials;

            response = null;
            try
            {
                response = request.GetResponse();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("\n#Error NullReferenceException\n#" + ex.Message);
                return "";
            }
            catch (WebException ex)
            {
                Console.WriteLine("\n#Error WebException\n#" + ex.Message);
                return "";
            }
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            //Console.WriteLine(responseFromServer);
            reader.Close();
            response.Close();
            return responseFromServer;
        }

        public vkapi()
        {

        }

        public vkapi(Settings set)
        {
            setting = set;
        }
    }
}
