using System;
using System.IO;
using System.Xml.Serialization;

namespace namesettings
{
    public class Settings
    {
        // Для сериализации
        private XmlSerializer formatter;
        public int timeOut;
        public int SendTimeSleep;
        public int GetTimeSleep;
        public int lenghtMessage;
        public string accesToken { get; set; }
        public string secret { get; set; }
        public string uid { get; set; }
        public string computerName { get; set; }

        //ошибки при чтении параметров
        public bool isRead()
        {
            if (accesToken.Length != 85)
            {
                Console.WriteLine("#error invalid accesstoken");
                return false;
            }
            if (secret.Length != 18)
            {
                Console.WriteLine("#error invalid secret");
                return false;
            }
            if (computerName.Length < 1)
            {
                Console.WriteLine("#error invalid computerName, default cmd:");
                computerName = "cmd:";
            }
            if (uid.Length < 1)
            {
                Console.WriteLine("#error invalid uid");
                return false;
            }
            if (timeOut < 0)
            {
                Console.WriteLine("#error timeOut < 0");
                timeOut = 100;
            }
            if (GetTimeSleep < 0)
            {
                Console.WriteLine("#error GetTimeSleep < 0");
                GetTimeSleep = 5000;
            }
            if (SendTimeSleep < 0)
            {
                Console.WriteLine("#error SendTimeSleep < 0");
                SendTimeSleep = 1000;
            }
            if (lenghtMessage < 0)
            {
                Console.WriteLine("#error lenghtMessage < 0");
                lenghtMessage = 600;
            }
            return true;
        }

        public Settings(string _accesToken, string _secret, string _uid, string _computerName, int _timeOut = -1, int _SendTimeSleep = -1, int _GetTimeSleep = -1, int _lenghtMessage = -1)
        {
            formatter = new XmlSerializer(typeof(Settings));
            lenghtMessage = _lenghtMessage;
            GetTimeSleep = _GetTimeSleep;
            SendTimeSleep = _SendTimeSleep;
            timeOut = _timeOut;
            accesToken = _accesToken;
            secret = _secret;
            uid = _uid;
            computerName = _computerName;
        }

        public Settings(Settings seting)
        {
            formatter = new XmlSerializer(typeof(Settings));
            setCloneSeting(seting);
        }

        public void setCloneSeting(Settings seting)
        {
            lenghtMessage = seting.lenghtMessage;
            GetTimeSleep = seting.GetTimeSleep;
            SendTimeSleep = seting.SendTimeSleep;
            timeOut = seting.timeOut;
            accesToken = seting.accesToken;
            secret = seting.secret;
            uid = seting.uid;
            computerName = seting.computerName;
        }

        public Settings()
        {
            formatter = new XmlSerializer(typeof(Settings));
        }

        public bool createFile(string name = "setting.xml")
        {
            lenghtMessage = 600;
            GetTimeSleep = 5000;
            SendTimeSleep = 1000;
            timeOut = 100;
            accesToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            computerName = "com:";
            secret = "xxxxxxxxxxxxxxxxxx";
            uid = "xxxxxxxxx";
            FileStream fs2 = new FileStream(name, FileMode.OpenOrCreate);
            try
            {
                formatter.Serialize(fs2, this);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("#error don't open " + name + " or xml is clear\n#" + ex.Message);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("#error Нет прав доступа к файлу" + name + ", пожалуйста перезапустите программу с правами администратора\n" + ex.Message);
                return false;
            }
            finally
            {
                fs2.Close();
            }
            Console.WriteLine("XML Файл " + name + " создан");
            return true;
        }

        //Считывает настройки vkapi из файла
        public bool readSetting(string filename = "setting.xml")
        {
            //читаем настройки из файла
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            try
            {
                setCloneSeting((Settings)formatter.Deserialize(fs));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("#error don't open setting.xml or xml is clear\n#" + ex.Message);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("#error Нет прав доступа к файлам, пожалуйста перезапустите программу с правами администратора\n" + ex.Message);
                return false;
            }
            finally
            {
                fs.Close();
            }
            return true;
        }
    }


}
