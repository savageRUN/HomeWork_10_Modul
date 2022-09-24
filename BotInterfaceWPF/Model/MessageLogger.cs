using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotInterfaceWPF.Model
{
    /// <summary>
    /// Класс представляющий пользовательскую модель сообщения
    /// </summary>
    internal class MessageLogger
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string DateTime { get; private set; }
        public string Message { get; set; }
        public MessageLogger(long id, string name, string message, string dateTime)
        {
            Id = id;
            Name = name;
            Message = message;
            DateTime = dateTime;
        }
    }
}
