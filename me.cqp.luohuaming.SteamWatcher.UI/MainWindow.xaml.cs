using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _notLoading = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            // AppConfig appConfig = new AppConfig("Config.json");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<FriendListItem> ConfigLists { get; set; } = [];

        public ObservableCollection<FriendListItem> FriendLists { get; set; } = [];

        public bool NotLoading
        {
            get { return _notLoading; }
            set
            {
                if (_notLoading != value)
                {
                    _notLoading = value;
                    Title = value ? "配置窗口" : "拉取数据中...";
                    OnPropertyChanged(nameof(NotLoading));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FriendLists.Where(x => x.Checked))
            {
                if (ConfigLists.Any(x => x.SteamID == item.SteamID))
                {
                    continue;
                }
                ConfigLists.Add(new FriendListItem
                {
                    Name = item.Name,
                    SteamID = item.SteamID,
                });
            }
            foreach (var item in FriendLists)
            {
                item.Checked = false;
            }
            OnPropertyChanged(nameof(FriendLists));
            OnPropertyChanged(nameof(ConfigLists));
        }

        private async void FetchConfigList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ConfigSteamID.Text))
            {
                ShowError("SteamID无效");
                return;
            }
            if (ConfigLists.Any(x => x.SteamID == ConfigSteamID.Text))
            {
                ShowError("项目重复");
                return;
            }
            NotLoading = false;

            var summaries = await GetPlayerSummary.Get([ConfigSteamID.Text]);
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                return;
            }
            string name = "";
            var info = summaries.players.FirstOrDefault();
            if (info != null)
            {
                name = $"{info.personaname} [{info.steamid}]";
                if (!string.IsNullOrEmpty(info.gameid))
                {
                    name += $" {info.gameextrainfo}";
                }
            }
            else
            {
                name = $"[{info.steamid}] [拉取失败]";
            }
            ConfigLists.Add(new()
            {
                Checked = false,
                Name = name,
                SteamID = info.steamid,
            });

            OnPropertyChanged(nameof(ConfigLists));
            NotLoading = true;
        }

        private async void FetchFriendList_Click(object sender, RoutedEventArgs e)
        {
            FriendLists = [];
            if (string.IsNullOrWhiteSpace(FriendSteamID.Text))
            {
                ShowError("SteamID无效");
                return;
            }
            NotLoading = false;
            var list = await GetFriendList.Get(FriendSteamID.Text);
            if (list == null)
            {
                ShowError("拉取项目时出现错误");
                return;
            }
            var summaries = await GetPlayerSummary.Get(list.friends.Select(x => x.steamid).ToList());
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                return;
            }
            foreach (var item in list.friends)
            {
                var info = summaries.players.FirstOrDefault(x => x.steamid == item.steamid);
                string name = "";
                if (info != null)
                {
                    name = $"{info.personaname} [{item.steamid}]";
                    if (!string.IsNullOrEmpty(info.gameid))
                    {
                        name += $" {info.gameextrainfo}";
                    }
                }
                else
                {
                    name = $"[{item.steamid}] [拉取失败]";
                }
                FriendLists.Add(new()
                {
                    Checked = false,
                    Name = name,
                    SteamID = item.steamid,
                });
            }

            OnPropertyChanged(nameof(FriendLists));
            NotLoading = true;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ConfigLists.Where(x => x.Checked).ToList())
            {
                ConfigLists.Remove(item);
            }
            OnPropertyChanged(nameof(ConfigLists));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.MonitorPlayers.Clear();
            foreach (var player in ConfigLists)
            {
                AppConfig.MonitorPlayers.Add(player.SteamID);
            }
            AppConfig.Instance.SetConfig("MonitorPlayers", AppConfig.MonitorPlayers);
            ShowInfo("保存成功");
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool select = FriendLists.Count != FriendLists.Count(x => x.Checked);
            foreach (var item in FriendLists)
            {
                item.Checked = select;
            }
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotLoading = false;
            if (string.IsNullOrEmpty(AppConfig.WebAPIKey))
            {
                ShowError("WebAPIKey无效，请前往 https://steamcommunity.com/dev/apikey 申请");
                return;
            }

            var summaries = await GetPlayerSummary.Get(AppConfig.MonitorPlayers);
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                return;
            }
            foreach (var item in summaries.players)
            {
                string name = "";
                if (item != null)
                {
                    name = $"{item.personaname} [{item.steamid}]";
                    if (!string.IsNullOrEmpty(item.gameid))
                    {
                        name += $" {item.gameextrainfo}";
                    }
                }
                else
                {
                    name = $"[{item.steamid}] [拉取失败]";
                }
                ConfigLists.Add(new()
                {
                    Checked = false,
                    Name = name,
                    SteamID = item.steamid,
                });
            }
            OnPropertyChanged(nameof(ConfigLists));
            NotLoading = true;
        }

        private void GroupConfig_Click(object sender, RoutedEventArgs e)
        {
            var form = new GroupNoticeConfig();
            form.ConfigLists = ConfigLists;
            form.ShowDialog();
        }
    }
}