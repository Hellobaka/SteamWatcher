﻿using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    /// <summary>
    /// GroupNoticeConfig.xaml 的交互逻辑
    /// </summary>
    public partial class GroupNoticeConfig : Window, INotifyPropertyChanged
    {
        private bool _notLoading = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public GroupNoticeConfig()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<MonitorItemWarpper> GroupTreeNodes { get; set; } = [];

        public ObservableCollection<GroupListItem> GroupLists { get; set; } = [];

        public ObservableCollection<FriendListItem> ConfigLists { get; set; } = [];

        public bool NotLoading
        {
            get { return _notLoading; }
            set
            {
                if (_notLoading != value)
                {
                    _notLoading = value;
                    Title = value ? "通知群配置" : "拉取数据中...";
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
            if (ConfigList.SelectedItem == null)
            {
                MainWindow.ShowError("请从右侧选择一项");
                return;
            }
            if (GroupTree.SelectedItem == null)
            {
                MainWindow.ShowError("请从左侧选择一项");
                return;
            }
            var selectItem = ConfigList.SelectedItem as FriendListItem;
            if (GroupTree.SelectedItem is MonitorItemWarpper item)
            {
                if (item.TargetId.Any(x=>x.SteamId == selectItem.SteamID))
                {
                    MainWindow.ShowInfo("项目重复");
                    return;
                }
                item.TargetId.Add(new MonitorItemWarpper.Child
                {
                    SteamId = selectItem.SteamID,
                    Name = selectItem.Name,
                    Parent = item
                });
            }
            else if(GroupTree.SelectedItem is MonitorItemWarpper.Child child)
            {
                var parent = child.Parent;
                if (parent.TargetId.Any(x => x.SteamId == selectItem.SteamID))
                {
                    MainWindow.ShowInfo("项目重复");
                    return;
                }
                parent.TargetId.Add(new MonitorItemWarpper.Child
                {
                    SteamId = selectItem.SteamID,
                    Name = selectItem.Name,
                    Parent = parent
                });
            }
            OnPropertyChanged(nameof(GroupTreeNodes));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (GroupTree.SelectedItem == null)
            {
                MainWindow.ShowError("请从左侧选择一项");
                return;
            }
            if (GroupTree.SelectedItem is MonitorItemWarpper item
                && MainWindow.ShowConfirm("确认删除这个群吗？子节点会被一起删除"))
            {
                GroupTreeNodes.Remove(item);
            }
            else if (GroupTree.SelectedItem is MonitorItemWarpper.Child child)
            {
                var parent = child.Parent;
                parent.TargetId.Remove(child);
            }
            OnPropertyChanged(nameof(GroupTreeNodes));
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var item = GroupTreeNodes.FirstOrDefault(x => x.GroupId.ToString() == GroupList.Text);
            if (item != null)
            {
                MainWindow.ShowInfo("项目重复");
                return;
            }
            if (!long.TryParse(GroupList.Text, out var group))
            {
                MainWindow.ShowInfo("群号无效");
                return;
            }
            GroupTreeNodes.Add(new()
            {
                GroupId = group,
                TargetId = []
            });
            OnPropertyChanged(nameof(GroupTreeNodes));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotLoading = false;
            try
            {
                foreach (var item in AppConfig.NoticeGroups)
                {
                    var i = new MonitorItemWarpper()
                    {
                        GroupId = item.GroupId,
                        TargetId = []
                    };
                    foreach(var steamId in item.TargetId)
                    {
                        i.TargetId.Add(new MonitorItemWarpper.Child
                        {
                            Parent = i,
                            SteamId = steamId,
                            Name = steamId,
                        });
                    }
                    GroupTreeNodes.Add(i);
                }
                OnPropertyChanged(nameof(GroupLists));
                OnPropertyChanged(nameof(ConfigLists));
                var list = MainSave.CQApi.GetGroupList();
                foreach (var item in list)
                {
                    GroupLists.Add(new GroupListItem
                    {
                        GroupID = item.Group,
                        Name = $"{item.Name}[{item.Group}]",
                    });
                }
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
            AppConfig.NoticeGroups.Clear();
            foreach (var item in GroupTreeNodes)
            {
                var node = new MonitorItem
                {
                    GroupId = item.GroupId,
                    TargetId = []
                };
                foreach (var target in item.TargetId)
                {
                    node.TargetId.Add(target.SteamId);
                }
                AppConfig.NoticeGroups.Add(node);
            }

            AppConfig.Instance.SetConfig("NoticeGroups", AppConfig.NoticeGroups);
            MainWindow.ShowInfo("保存成功");
        }
    }
}