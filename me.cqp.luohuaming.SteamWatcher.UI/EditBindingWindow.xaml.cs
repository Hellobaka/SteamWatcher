using System;
using System.Windows;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// EditBindingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditBindingWindow : Window
    {
        private const long SteamIDOffset = 76561197960265728;

        public string QQ { get; private set; }
        public string SteamID { get; private set; }

        public EditBindingWindow(BindingItem item)
        {
            InitializeComponent();

            // 填充初始数据
            QQTextBox.Text = item.QQ;
            SteamIDTextBox.Text = item.SteamID;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QQTextBox.Text))
            {
                MessageBox.Show("请输入QQ号", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!long.TryParse(QQTextBox.Text, out long qq))
            {
                MessageBox.Show("QQ号格式不正确", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SteamIDTextBox.Text))
            {
                MessageBox.Show("请输入SteamID", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string steamId = SteamIDTextBox.Text;
            if (steamId.Length != 17 && long.TryParse(steamId, out long friendCode))
            {
                steamId = (friendCode + SteamIDOffset).ToString();
                SteamIDTextBox.Text = steamId;
            }

            if (!long.TryParse(steamId, out _))
            {
                MessageBox.Show("SteamID格式不正确", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            QQ = qq.ToString();
            SteamID = steamId;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}