using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotInterfaceWPF.Model;
using KstovoTelegramBot;
namespace BotInterfaceWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TelegramBotClient _client;
        WindowForMessage _windowForMessage;
        MessageLogger _selectedLog;
        public MainWindow()
        {
            InitializeComponent();
            _client = new TelegramBotClient("5474272294:AAHphoytgHRiAsnd17x95QHYSH0Ti249ZRg", this);
            msgLogList.ItemsSource = _client.MessagesLog;
            _windowForMessage = new WindowForMessage();
            Task task = new Task(new Action(_client.ListenerMessage));
            task.Start();
        }
        /// <summary>
        /// Обработчик двойного клика по строке с сообщением, показывает сообщение в отдельном окне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MsgLogList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageLogger logger = msgLogList.SelectedItem as MessageLogger;
            _windowForMessage.ExtMessageBlock.Text = logger.Message;
            if(!_windowForMessage.IsVisible)
                _windowForMessage.Show();
            if (!_windowForMessage.IsFocused)
            {
                _windowForMessage.Focus();
            }
        }
        /// <summary>
        /// Обработчик кнопки отправки сообщения выбранному пользователю
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendMsgBtn_Click(object sender, RoutedEventArgs e)
        {
            if(_selectedLog == null)
            {
                MessageBox.Show("Не выбран пользователь для ответа!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await _client.SendMessage(_selectedLog.Id.ToString(), MessageTxtBx.Text);
            MessageTxtBx.Text = "";
        }
        /// <summary>
        /// Обработчик смены выбранного сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msgLogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedLog = msgLogList.SelectedItem as MessageLogger;
            TooltipTxBlck.Text = $"Ответ пользователю - {_selectedLog.Name} с Id - {_selectedLog.Id}";
        }
    }
}
