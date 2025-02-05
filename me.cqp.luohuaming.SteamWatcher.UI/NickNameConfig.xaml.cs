using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// NickNameConfig.xaml 的交互逻辑
    /// </summary>
    public partial class NickNameConfig : Window, INotifyPropertyChanged
    {
        public NickNameConfig()
        {
            InitializeComponent();
            DataContext = this;
        }

        private bool _notLoading = true;

        public bool NotLoading
        {
            get { return _notLoading; }
            set
            {
                if (_notLoading != value)
                {
                    _notLoading = value;
                    Title = value ? "昵称配置" : "拉取数据中...";
                    OnPropertyChanged(nameof(NotLoading));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<NickNameItemWarpper> GroupTreeNodes { get; set; } = [];

        public ObservableCollection<GroupListItem> GroupLists { get; set; } = [];

        public ObservableCollection<FriendListItem> ConfigLists { get; set; } = [];

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotLoading = false;
            try
            {
                var list = MainSave.CQApi?.GetGroupList();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        GroupLists.Add(new GroupListItem
                        {
                            GroupID = item.Group,
                            Name = $"{item.Group}[{item.Name}]",
                        });
                    }
                }

                foreach (var item in ConfigLists)
                {
                    var i = new NickNameItemWarpper()
                    {
                        SteamID = item.SteamID,
                        Name = ConfigLists.FirstOrDefault(x => x.SteamID == item.SteamID)?.Name ?? item.SteamID.ToString(),
                        Groups = []
                    };
                    foreach (var nickname in AppConfig.NickNames.FirstOrDefault(x=>x.SteamID == item.SteamID)?.Groups ?? [])
                    {
                        var groupItem = GroupLists.FirstOrDefault(x=>x.GroupID == nickname.GroupID);
                        i.Groups.Add(new NickNameItemWarpper.Child
                        {
                            Parent = i,
                            GroupID = nickname.GroupID,
                            Name = $"{nickname.GroupID}[{groupItem?.Name}]: {nickname.NickName}",
                            NickName = nickname.NickName
                        });
                    }
                    GroupTreeNodes.Add(i);
                }

                OnPropertyChanged(nameof(GroupLists));
                OnPropertyChanged(nameof(ConfigLists));
                OnPropertyChanged(nameof(GroupTreeNodes));
            }
            catch (Exception ex)
            {
                MainWindow.ShowError($"无法拉取群列表：{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                NotLoading = true;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.NickNames.Clear();
            foreach (var item in GroupTreeNodes.Where(x=>x.Groups.Count > 0))
            {
                var node = new NickNameItem
                {
                    SteamID = item.SteamID,
                    Groups = []
                };
                foreach (var target in item.Groups)
                {
                    node.Groups.Add(new NickNameGroupItem
                    {
                        GroupID = target.GroupID,
                        NickName = target.NickName
                    });
                }
                AppConfig.NickNames.Add(node);
            }

            AppConfig.Instance.SetConfig("NickNames", AppConfig.NickNames);
            MainWindow.ShowInfo("保存成功");
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string parse = GroupList.Text.Contains("[") ? GroupList.Text.Split('[').First() : GroupList.Text;
            if (!long.TryParse(parse, out var group))
            {
                MainWindow.ShowInfo("群号无效");
                return;
            }
            if(string.IsNullOrWhiteSpace(NickNameValue.Text))
            {
                MainWindow.ShowInfo("昵称不能为空");
                return;
            }
            NickNameItemWarpper parent;
            if (GroupTree.SelectedItem is NickNameItemWarpper item)
            {
                parent = item;
            }
            else if (GroupTree.SelectedItem is NickNameItemWarpper.Child child)
            {
                parent = child.Parent;
            }
            else
            {
                MainWindow.ShowError("请从左侧选择一项");
                return;
            }
            if (parent.Groups.Any(x => x.GroupID == group))
            {
                var g = parent.Groups.First(x => x.GroupID == group);
                g.NickName = NickNameValue.Text;
                g.Name = $"{GroupList.Text}: {NickNameValue.Text}";
            }
            else
            {
                parent.Groups.Add(new NickNameItemWarpper.Child
                {
                    GroupID = group,
                    NickName = NickNameValue.Text,
                    Parent = parent,
                    Name = $"{GroupList.Text}: {NickNameValue.Text}",
                });
            }
            OnPropertyChanged(nameof(GroupTreeNodes));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (GroupTree.SelectedItem is NickNameItemWarpper)
            {
                MainWindow.ShowError("请选择以群号开头的子项目");
                return;
            }
            else if (GroupTree.SelectedItem is NickNameItemWarpper.Child child)
            {
                child.Parent.Groups.Remove(child);
            }
            else
            {
                MainWindow.ShowError("请从左侧选择一项");
                return;
            }
            OnPropertyChanged(nameof(GroupTreeNodes));
        }

        private void GroupTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NickNameItemWarpper.Child child)
            {
                if (GroupLists.Any(x=>x.GroupID == child.GroupID))
                {
                    GroupList.SelectedItem = GroupLists.First(x => x.GroupID == child.GroupID);
                }
                else
                {
                    GroupList.Text = child.GroupID.ToString();
                }

                NickNameValue.Text = child.NickName;
            }
        }
    }
}
