using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// SteamBindingConfig.xaml 的交互逻辑
    /// </summary>
    public partial class SteamBindingConfig : Window, INotifyPropertyChanged
    {
        private const long SteamIDOffset = 76561197960265728;

        public SteamBindingConfig()
        {
            InitializeComponent();
            DataContext = this;
            LoadBindings();
        }

        private ObservableCollection<BindingItem> _bindingItems = [];
        public ObservableCollection<BindingItem> BindingItems
        {
            get => _bindingItems;
            set
            {
                _bindingItems = value;
                OnPropertyChanged(nameof(BindingItems));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadBindings()
        {
            BindingItems.Clear();
            foreach (var binding in AppConfig.SteamBinding)
            {
                BindingItems.Add(new BindingItem
                {
                    QQ = binding.QQ.ToString(),
                    SteamID = binding.SteamId.ToString(),
                    SteamName = "未获取",
                    Status = "待刷新"
                });
            }
            RefreshSteamNames();
        }

        private async void RefreshSteamNames()
        {
            var steamIds = BindingItems.Select(x => x.SteamID).ToList();
            if (steamIds.Count == 0) return;

            var summaries = await GetPlayerSummary.Get(steamIds);
            if (summaries?.players != null)
            {
                foreach (var item in BindingItems)
                {
                    var player = summaries.players.FirstOrDefault(p => p.steamid == item.SteamID);
                    if (player != null)
                    {
                        item.SteamName = player.personaname;
                        item.Status = "正常";
                    }
                    else
                    {
                        item.Status = "获取失败";
                    }
                }
                OnPropertyChanged(nameof(BindingItems));
            }
        }

        private async void FetchUserInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SteamIDTextBox.Text))
            {
                ShowError("请输入SteamID");
                return;
            }

            string steamId = SteamIDTextBox.Text;
            if (steamId.Length != 17 && long.TryParse(steamId, out long friendCode))
            {
                steamId = (friendCode + SteamIDOffset).ToString();
                SteamIDTextBox.Text = steamId;
            }

            var summaries = await GetPlayerSummary.Get([steamId]);
            if (summaries?.players?.FirstOrDefault() != null)
            {
                var player = summaries.players.First();
                ShowInfo($"获取成功，Steam名称为: {player.personaname}");
            }
            else
            {
                ShowError("获取用户信息失败");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QQTextBox.Text))
            {
                ShowError("请输入QQ号");
                return;
            }

            if (!long.TryParse(QQTextBox.Text, out long qq))
            {
                ShowError("QQ号格式不正确");
                return;
            }

            if (string.IsNullOrWhiteSpace(SteamIDTextBox.Text))
            {
                ShowError("请输入SteamID");
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
                ShowError("SteamID格式不正确");
                return;
            }

            // 检查重复
            if (BindingItems.Any(x => x.QQ == qq.ToString() || x.SteamID == steamId))
            {
                ShowError("QQ号或SteamID已存在");
                return;
            }

            var newItem = new BindingItem
            {
                QQ = qq.ToString(),
                SteamID = steamId,
                SteamName = "待获取",
                Status = "待刷新"
            };

            BindingItems.Add(newItem);

            // 立即刷新新添加的用户信息
            RefreshSteamNames();

            ClearButton_Click(null, null);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = BindingListView.SelectedItem as BindingItem;
            if (selectedItem == null)
            {
                ShowError("请选择要编辑的绑定项");
                return;
            }

            // 弹出编辑窗口
            var editWindow = new EditBindingWindow(selectedItem);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                // 更新数据
                selectedItem.QQ = editWindow.QQ;
                selectedItem.SteamID = editWindow.SteamID;
                selectedItem.SteamName = "待获取";
                selectedItem.Status = "待刷新";

                // 刷新用户信息
                RefreshSteamNames();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = BindingListView.SelectedItem as BindingItem;
            if (selectedItem == null)
            {
                ShowError("请选择要删除的绑定项");
                return;
            }

            if (ShowConfirm($"确定要删除QQ号 {selectedItem.QQ} 的绑定吗？"))
            {
                BindingItems.Remove(selectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshSteamNames();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.SteamBinding.Clear();
            foreach (var item in BindingItems)
            {
                if (long.TryParse(item.QQ, out long qq) && long.TryParse(item.SteamID, out long steamId))
                {
                    AppConfig.SteamBinding.Add(new QQSteamBinding
                    {
                        QQ = qq,
                        SteamId = steamId
                    });
                }
            }

            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);
            ShowInfo("保存成功");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            QQTextBox.Clear();
            SteamIDTextBox.Clear();
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool ShowConfirm(string message)
        {
            return MessageBox.Show(message, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }

    public class BindingItem : INotifyPropertyChanged
    {
        private string _qq;
        private string _steamID;
        private string _steamName;
        private string _status;

        public string QQ
        {
            get => _qq;
            set
            {
                _qq = value;
                OnPropertyChanged();
            }
        }

        public string SteamID
        {
            get => _steamID;
            set
            {
                _steamID = value;
                OnPropertyChanged();
            }
        }

        public string SteamName
        {
            get => _steamName;
            set
            {
                _steamName = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}