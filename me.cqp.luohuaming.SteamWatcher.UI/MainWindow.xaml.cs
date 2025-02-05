using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _notLoading = true;

        private const long SteamIDOffset = 76561197960265728;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //AppConfig appConfig = new AppConfig("Config.json");
            //Monitors monitors = new();
            //monitors.PlayingChanged += Monitors_PlayingChanged;
            //monitors.StartCheckTimer();
            //MainSave.ImageDirectory = "";
            //MainSave.AppDirectory = "";
        }

        private void Monitors_PlayingChanged(System.Collections.Generic.List<MonitorNoticeItem> notices)
        {
            StringBuilder sb = new();
            foreach (var notice in notices)
            {
                sb.AppendLine(notice.ToString());
                if (AppConfig.EnableDraw && (notice.NoticeType == NoticeType.Playing || notice.NoticeType == NoticeType.GetAchievement))
                {
                    if (notice.DownloadAvatar())
                    {
                        string filePath = notice.Draw();
                        if (File.Exists(Path.Combine("data", "image", filePath)))
                        {
                            sb.AppendLine(CQApi.CQCode_Image(filePath).ToSendString());
                        }
                    }
                    else
                    {
                        MainSave.CQLog?.Warning("下载头像", $"下载 {notice.PlayerName}[{notice.SteamID}] 用户头像时失败");
                    }
                }
            }
            sb.RemoveNewLine();
            Debug.WriteLine(sb.ToString());
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
                    NickName = item.NickName
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
            if (ConfigSteamID.Text.Length != 17 && long.TryParse(ConfigSteamID.Text, out long friendCode))
            {
                ConfigSteamID.Text = (friendCode + SteamIDOffset).ToString();
            }

            var summaries = await GetPlayerSummary.Get([ConfigSteamID.Text]);
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                NotLoading = true;
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
                ConfigLists.Add(new()
                {
                    Checked = false,
                    Name = name,
                    SteamID = info.steamid,
                    NickName = info.personaname
                });
            }
            else
            {
                ShowError("拉取项目时出现错误，获取到的 SteamID 无效");
            }

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
            if (FriendSteamID.Text.Length != 17 && long.TryParse(FriendSteamID.Text, out long friendCode))
            {
                FriendSteamID.Text = (friendCode + SteamIDOffset).ToString();
            }
            var list = await GetFriendList.Get(FriendSteamID.Text);
            if (list == null)
            {
                ShowError("拉取项目时出现错误");
                NotLoading = true;
                return;
            }
            var summaries = await GetPlayerSummary.Get(list.friends.Select(x => x.steamid).ToList());
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                NotLoading = true;
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
                    NickName = info?.personaname
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
            if (AppConfig.MonitorPlayers.Count == 0)
            {
                NotLoading = true;
                return;
            }
            ConfigLists = [];
            var summaries = await GetPlayerSummary.Get(AppConfig.MonitorPlayers);
            if (summaries == null)
            {
                ShowError("拉取项目时出现错误");
                NotLoading = true;
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
                    NickName = item.personaname
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
            form.Close();
        }

        private void ParamButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new ParamSetting();
            form.ShowDialog();
            form.Close();
        }

        private void CopyNick_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).DataContext as FriendListItem;
            if (item is not null)
            {
                Clipboard.SetText(item.NickName);
            }
            else
            {
                ShowError("选中项无效");
            }
        }

        private void CopySteamID_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).DataContext as FriendListItem;
            if (item is not null)
            {
                Clipboard.SetText(item.SteamID);
            }
            else
            {
                ShowError("选中项无效");
            }
        }

        private void CopyFriendCode_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).DataContext as FriendListItem;
            if (item is not null)
            {
                if (long.TryParse(item.SteamID, out long steamID))
                {
                    Clipboard.SetText((steamID - SteamIDOffset).ToString());
                }
                else
                {
                    ShowError("SteamID无效");
                }
            }
            else
            {
                ShowError("选中项无效");
            }
        }

        private void NickConfig_Click(object sender, RoutedEventArgs e)
        {
            var form = new NickNameConfig();
            form.ConfigLists = ConfigLists;
            form.ShowDialog();
            form.Close();
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            if (ShowConfirm("刷新列表前请确保已保存更改，是否继续？"))
            {
                Dispatcher.BeginInvoke(() => Window_Loaded(sender, e));
            }
        }
    }
}