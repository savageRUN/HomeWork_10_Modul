using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using BotInterfaceWPF.Model;
using BotInterfaceWPF;
using System.Media;
using Newtonsoft.Json;

namespace KstovoTelegramBot
{
    /// <summary>
    /// Подобие своего Телеграмм бот клиента
    /// </summary>
    internal class TelegramBotClient
    {
        /// <summary>
        /// Клиент для Post запросов к Api Telegram
        /// </summary>
        WebClient WebClient { get;}
        /// <summary>
        /// Коллекция сообщений поступающих от пользователей бота
        /// </summary>
        public ObservableCollection<MessageLogger> MessagesLog { get; }
        /// <summary>
        /// Главное окно WPF
        /// </summary>
        MainWindow _mainWindow;
        /// <summary>
        /// Токен бота
        /// </summary>
        readonly string _token;
        public string Token { get { return _token; } }
        const string BaseAdress = "https://api.telegram.org/bot";
        const string DownloadBaseAdress = "https://api.telegram.org/file/bot";
        int _updateId;
        public int UpdateId { get { return _updateId; } private set { _updateId = value; } }
        public TelegramBotClient(string token, MainWindow window)
        {
            WebClient = new WebClient() { Encoding = Encoding.UTF8};
            _token = token;
            _updateId = 0;
            MessagesLog = new ObservableCollection<MessageLogger>();
            _mainWindow = window;
        }
        /// <summary>
        /// Асинхронный метод для обработки сообщений/файлов API телеграм
        /// </summary>
        async internal void ListenerMessage()
        {
            
            while (true)
            {
                string url = $"{BaseAdress}{Token}/getUpdates?offset={UpdateId}";
                string requestStr = WebClient.DownloadString(url);
                JToken [] messages = JObject.Parse(requestStr)["result"].ToArray();
                foreach (JToken message in messages)
                {
                    //Console.WriteLine(message.ToString());
                    if (message["message"]["text"] != null)
                    {
                        UpdateId = Convert.ToInt32(message["update_id"].ToString()) + 1;
                        string userMsg = message["message"]["text"].ToString();
                        string userId = message["message"]["from"]["id"].ToString();
                        string userFirstName = message["message"]["from"]["first_name"].ToString();
                        string text = $"{userFirstName} {userId} {userMsg}";
                        string dateTicks = message["message"]["date"].ToString();
                        var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(dateTicks)).ToLocalTime();
                        _mainWindow.Dispatcher.Invoke(() =>
                        {
                        var newMsg = new MessageLogger(long.Parse(userId), userFirstName, userMsg, date.ToString("G"));
                            MessagesLog.Add(newMsg);
                            SystemSounds.Exclamation.Play();
                            SaveMessage(newMsg);
                        });
                        if (userMsg.ToLower() == "/start")
                        {
                            string messageToUser = $"{userFirstName}, вас приветствует тестовый бот!";
                            url = $"{BaseAdress}{Token}/sendMessage?chat_id={userId}&text={messageToUser}";
                            WebClient.DownloadString(url);
                        }
                        if(userMsg.ToLower() == "/help")
                        {
                            string helpMsg = $"Бот поддерживает команды:\n\t/start - Начало использование бота" +
                                $"\n\t/files - Загружает список сохраненых файлов";
                            url = $"{BaseAdress}{Token}/sendMessage?chat_id={userId}&text={helpMsg}";
                            WebClient.DownloadString(url);
                        }
                        if(userMsg.ToLower() == "/files")
                        {
                            var files = new DirectoryInfo(@"C:\Users\user\Desktop\HomeWork_10.5\BotInterfaceWPF\bin\Debug").GetFiles();
                            foreach (var file in files)
                            {
                                UploadToTelegram(Token, userId, file.FullName, file.Name);
                            }

                        }
                        Debug.WriteLine(text);
                    }
                    else if (message["message"]["document"]!= null)
                    {
                        UpdateId = Convert.ToInt32(message["update_id"].ToString()) + 1;
                        string documentId = message["message"]["document"]["file_id"].ToString();
                        string documentName = message["message"]["document"]["file_name"].ToString();
                        string documentType = message["message"]["document"]["mime_type"].ToString();
                        string fileDownload = $"{BaseAdress}{Token}/getFile?file_id={documentId}";
                        string requestFileInfo = WebClient.DownloadString(fileDownload);
                        string filePath = JObject.Parse(requestFileInfo)["result"]["file_path"].ToString();
                        //Console.WriteLine(filePath);
                        url = $"{DownloadBaseAdress}{Token}\\{filePath}";
                        WebClient.DownloadFile(url, $@"{documentName}");
                        Debug.WriteLine($"Файл - {documentName} сохранен");
                    } else if (message["message"]["photo"] != null)
                    {
                        UpdateId = Convert.ToInt32(message["update_id"].ToString()) + 1;
                        string dateTicks = message["message"]["date"].ToString();
                        var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(dateTicks)).ToLocalTime();
                        //Console.WriteLine(date.ToString("f"));
                        var photos = message["message"]["photo"];
                        Debug.WriteLine($"Добавление фото {date.AddSeconds(1):yyyy MMMM dd H_mm_ss}");
                        foreach (var photo in photos)
                        {
                            string fileDownload = $"{BaseAdress}{Token}/getFile?file_id={photo["file_id"]}";
                            string requestFileInfo = WebClient.DownloadString(fileDownload);
                            string filePath = JObject.Parse(requestFileInfo)["result"]["file_path"].ToString();
                            ///Console.WriteLine(filePath);
                            url = $"{DownloadBaseAdress}{Token}\\{filePath}";
                            //Console.WriteLine(url);
                            WebClient.DownloadFile(url, $@"{date.AddSeconds(1):yyyy MMMM dd H_mm_ss}{Path.GetExtension(filePath)}");
                        }
                        
                    }else if (message["message"]["audio"] != null)
                    {
                        UpdateId = Convert.ToInt32(message["update_id"].ToString()) + 1;
                        string fileDownload = $"{BaseAdress}{Token}/getFile?file_id={message["message"]["audio"]["file_id"]}";
                        string audioName = message["message"]["audio"]["file_name"].ToString();
                        string requestFileInfo = WebClient.DownloadString(fileDownload);
                        string filePath = JObject.Parse(requestFileInfo)["result"]["file_path"].ToString();
                        //Console.WriteLine(filePath);
                        url = $"{DownloadBaseAdress}{Token}\\{filePath}";
                        Debug.WriteLine($"Добавлен аудио файл - {audioName}");
                        WebClient.DownloadFile(url, $@"{audioName}");
                    }else if (message["message"]["video"] != null)
                    {
                        UpdateId = Convert.ToInt32(message["update_id"].ToString()) + 1;
                        string dateTicks = message["message"]["date"].ToString();
                        var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(dateTicks)).ToLocalTime();
                        string fileDownload = $"{BaseAdress}{Token}/getFile?file_id={message["message"]["video"]["file_id"]}";
                        string requestFileInfo = WebClient.DownloadString(fileDownload);
                        string filePath = JObject.Parse(requestFileInfo)["result"]["file_path"].ToString();
                        url = $"{DownloadBaseAdress}{Token}\\{filePath}";
                        WebClient.DownloadFile(url, $@"{date.AddSeconds(1):yyyy MMMM dd H_mm_ss}{Path.GetExtension(filePath)}");
                        Debug.WriteLine($"Добавлен видео файл - {date:yyyy MMMM dd H_mm_ss}{Path.GetExtension(filePath)}");
                    }
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Асинхронный метод для скачивания сохраненых файлов в телеграмм
        /// </summary>
        /// <param name="token">Токен</param>
        /// <param name="chatId">id пользователя отправившего файл</param>
        /// <param name="filePath">путь к файлам</param>
        /// <param name="fileName">имя файла</param>
        /// <returns></returns>
        public static async Task UploadToTelegram(string token, string chatId, string filePath, string fileName)
        {
            var client = new HttpClient();
            var content = new MultipartFormDataContent();
            var fs = new FileStream(filePath, FileMode.Open);
            var sc = new StreamContent(fs);
            sc.Headers.Add("Content-Type", "application/octet-stream");
            string headerValue = FixEnc("form-data; name=\"document\"; filename=\"" + fileName + "\"");
            sc.Headers.Add("Content-Disposition", headerValue);
            content.Add(sc, "document", fileName);
            string url = $"{BaseAdress}{token}/sendDocument?chat_id={chatId}";
            using (var message = await client.PostAsync(url, content))
            {
                var input = await message.Content.ReadAsStringAsync();
            }
            fs.Close();
        }
        /// <summary>
        ///  Метод отправки сообщения выбранному пользователю
        /// </summary>
        /// <param name="id">Id пользователя</param>
        /// <param name="text">Текст сообщения</param>
        /// <returns></returns>
        public async Task SendMessage(string id, string text)
        {
            var client = new WebClient();
            var url = $"{BaseAdress}{Token}/sendMessage?chat_id={id}&text={text}";
            client.DownloadString(url);
        }
        /// <summary>
        /// Метод сериализации сообщений в текстовый файл для каждого пользователя
        /// </summary>
        /// <param name="messageLog"></param>
        void SaveMessage(MessageLogger messageLog)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            using (StreamWriter sw = new StreamWriter($"{Environment.CurrentDirectory}\\{messageLog.Name}_{messageLog.Id}.txt", true, Encoding.UTF8))
            {
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jw, messageLog);
                }
            }
        }
        /// <summary>
        /// Метод перекодировки в UTF-8
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Возвращает перекодированную строку</returns>
        private static string FixEnc(string source)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            source = "";
            foreach (byte b in bytes)
            {
                source += (Char)b;
            }
            return source;
        }
    }
}
