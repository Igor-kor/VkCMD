using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using namevkapi;

namespace namevk
{
    public class vk
    {
        // Для общения с вк
        private vkapi start;
        // Для сериализации vkapi
        private XmlSerializer formatter;
        XmlDocument textResponce = new XmlDocument();
        // Для остановки цикла
        bool breakMainLoop = false;

        string command = null;

        //Передаем из маина аргументы
        public vk(string[] args)
        {
            //инициализируем наш класс и сериализацию для него
            start = new vkapi();
            formatter = new XmlSerializer(typeof(vkapi));
            //если надо то это создаст файл с сериализуемыми полями
            if (args.Length > 0)
            {
                if (args[0].Equals("xml"))
                {
                    createFile();
                }
            }
            if (readSetting() == false)
            {
                createFile();
                if (readSetting() == false)
                {
                    Console.WriteLine("#ERROR Попытка создать файл настроек не удалась.");
                }
            }
        }

        //Создает файл если это необходимо(для первоначальной настройки)
        private bool createFile()
        {
            start.lenghtMessage = 600;
            start.GetTimeSleep = 5000;
            start.SendTimeSleep = 1000;
            start.timeOut = 100;
            start.accesToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            start.computerName = "com:";
            start.secret = "xxxxxxxxxxxxxxxxxx";
            start.uid = "xxxxxxxxx";
            FileStream fs2 = new FileStream("setting.xml", FileMode.OpenOrCreate);
            try
            {
                formatter.Serialize(fs2, start);
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
                fs2.Close();
            }
            Console.WriteLine("XML Файл setting.xml создан");
            return true;
        }

        //Считывает настройки vkapi из файла
        private bool readSetting(string filename = "setting.xml")
        {
            //читаем настройки из файла
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            try
            {
                start = (vkapi)formatter.Deserialize(fs);
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

        //Получение последнего сообщения
        private string getLastMessage()
        {
            string temp = start.sendRequest("messages.get.xml?count=1");
            if (temp.Length < 1)
            {
                Console.WriteLine("#error Сервер не овечает");
                System.Threading.Thread.Sleep(start.GetTimeSleep);
                throw new Exception("Сервер не овечает");
            }
            // todo обработка присылаемых ошибок
            //Console.WriteLine(temp);
            return temp;
        }

        private bool parseUid(XmlElement root)
        {
            XmlNodeList elemList = root.GetElementsByTagName("uid");
            string tempCommand = null;
            if (elemList.Count <= 0)
            {
                Console.WriteLine("#error Не удалось прочесть uid, elemList.Count <= 0");
                System.Threading.Thread.Sleep(start.SendTimeSleep);
                return false;
            }
            for (int i = 0; i < elemList.Count; i++)
            {
                tempCommand = elemList[i].InnerXml;
                Console.WriteLine("[отправил:" + tempCommand + "]");
            }
            //если отправил нужный нам человек с юид
            if (tempCommand.Equals(start.uid))
            {
                return true;
            }
            return false;
        }

        private bool parsePC(XmlElement root)
        {
            XmlNodeList elemList = root.GetElementsByTagName("body");
            string tempCommand = null;
            if (elemList.Count <= 0)
            {
                Console.WriteLine("#error Не удалось прочесть body, elemList.Count <= 0");
                System.Threading.Thread.Sleep(start.SendTimeSleep);
                return false;
            }
            for (int i = 0; i < elemList.Count; i++)
            {
                tempCommand = elemList[i].InnerXml;
                Console.WriteLine("[сообщение:]" + tempCommand);
            }
            //если это наш компьютер то покинем ожидание выполнения
            if (tempCommand.StartsWith(start.computerName))
            {
                command = tempCommand;
                return true;
            }
            return false;
        }

        private string createProccess(string command)
        {
            string tempCommand = command.Remove(0, start.computerName.Length);
            string commandCmd = " " + tempCommand + ">test.txt";
            File.WriteAllText(@"command.bat", commandCmd);
            //создаем процесс для запуска батника
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C command.bat";
            process.StartInfo = startInfo;
            process.Start();
            //ожидаем пока процесс завершится
            int tempTimeOut = 0;
            while (!process.WaitForExit(1000))
            {
                string temp = start.sendRequest("messages.get.xml?count=1");
                if (temp.Length < 1)
                {
                    Console.WriteLine("#error Сервер не овечает");
                    System.Threading.Thread.Sleep(start.GetTimeSleep);
                    break;
                }
                //СЧИТЫВАЕМ ЗНАЧЕНИЕ
                textResponce.LoadXml(temp);
                XmlElement root = textResponce.DocumentElement;
                //если отправил нужный нам человек с юид
                if (parseUid(root))
                {
                    //если это наш компьютер то покинем ожидание выполнения
                    if (parsePC(root))
                    {
                        tempCommand = tempCommand.Remove(0, start.computerName.Length);
                        if (tempCommand.Equals("stop")) break;
                    }
                }
                System.Threading.Thread.Sleep(1000);
                tempTimeOut++;

                if (tempTimeOut == 60 * 60) break;
            }
            string textOut;
            try
            {
                textOut = File.ReadAllText(@"test.txt", Encoding.GetEncoding(866));
            }
            catch (IOException)
            {
                textOut = "Не удалось открыть файл, так как он занят!";
            }
            return textOut;
        }

        private bool sendCMDResponse(string textOut)
        {
            //======================= это нужно чтобы по частям отсылать сообщения lMessage - длинна сообщения в символах
            int symbolCount = 0;
            int LenghtMessage = start.lenghtMessage;
            //====================================цикл для отправки
            int tempTimeOut = 0;
            string tempStringText = null;
            while (symbolCount < textOut.Length)
            {
                Console.WriteLine("tempTimeOut = " + tempTimeOut);
                if (tempTimeOut == start.timeOut)
                {
                    Console.WriteLine("#error превышен интервал ошибок отправки, отправка будет прервана!");
                    return false;
                }
                if (symbolCount + start.lenghtMessage < textOut.Length)
                {
                    tempStringText = textOut.Substring(symbolCount, start.lenghtMessage);
                }
                else
                {
                    tempStringText = textOut.Substring(symbolCount);
                }

                //отправляем запрос и записываем ответ сервера в temp2
                string temp2 = start.sendRequest("messages.send.xml?user_id=" + start.uid + "&message=" + tempStringText);
                LenghtMessage = start.lenghtMessage;
                if (temp2.Length < 1)
                {
                    Console.WriteLine("#error не получен ответ от сервера");
                    System.Threading.Thread.Sleep(start.SendTimeSleep);
                    tempTimeOut++;
                    LenghtMessage = 0;
                    continue;
                }
                //читаем ответ сервера
                textResponce.LoadXml(temp2);
                string tempCommand = null;
                XmlElement root = textResponce.DocumentElement;
                XmlNodeList elemList = root.GetElementsByTagName("error_code");
                for (int i = 0; i < elemList.Count; i++)
                {
                    tempCommand = elemList[i].InnerXml;
                    Console.WriteLine("#error_code:" + tempCommand);
                }
                //если нет ошибок при отправки
                if (tempCommand == null)
                {
                    LenghtMessage = start.lenghtMessage;
                    tempTimeOut = 0;
                }
                else
                {
                    tempTimeOut++;
                    Console.WriteLine("#error отсервера получено сообщение об ошибке:\n" + temp2);
                    System.Threading.Thread.Sleep(start.SendTimeSleep);
                    LenghtMessage = 0;
                }
                Console.WriteLine("переданно:" + (int)(((float)symbolCount / (float)textOut.Length) * 100) + "%");
                System.Threading.Thread.Sleep(start.SendTimeSleep);
                symbolCount += LenghtMessage;
            }
            return true;
        }

        private bool parseError(XmlElement root)
        {
            XmlNodeList error = root.GetElementsByTagName("error");
            if (error.Count > 0)
            {
                XmlNodeList error_code = root.GetElementsByTagName("error_code");
                XmlNodeList error_msg = root.GetElementsByTagName("error_msg");
                Console.WriteLine("#Server Error:" + error_code[0].InnerXml + " error_msg:" + error_msg[0].InnerXml);
                return true;
            }
            return false;
        }

        public bool mainLoop()
        {
            //главный цикл в котором все реализованно
            while (true)
            {
                if (breakMainLoop)
                {
                    return true;
                }
                //отправляем запрос на получение 1 сообщения
                Console.WriteLine("==============================================================");
                textResponce.LoadXml(getLastMessage());
                XmlElement root = textResponce.DocumentElement;
                //проверяем сначало на ошибки
                if (parseError(root))
                {
                    Console.WriteLine(DateTime.Now);
                    //сделаем задержку между получением сообщения
                    System.Threading.Thread.Sleep(start.GetTimeSleep);
                    continue;
                }
                //todo переделать в массив uids
                //если uidсовпадает
                if (parseUid(root))
                {
                    //если это наш компьютер то выполняем
                    if (parsePC(root))
                    {
                        string textOut = createProccess(command);
                        sendCMDResponse(textOut);
                    }
                }
                DateTime localDate = DateTime.Now;
                Console.WriteLine(localDate);
                //сделаем задержку между получением сообщения
                System.Threading.Thread.Sleep(start.GetTimeSleep);
            }
        }


    }
}
