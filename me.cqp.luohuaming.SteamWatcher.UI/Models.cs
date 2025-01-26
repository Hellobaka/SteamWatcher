using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    public class FriendListItem : INotifyPropertyChanged
    {
        private bool _checked;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged(nameof(Checked));
                }
            }
        }

        public string Name { get; set; }

        public string SteamID { get; set; }

        public string NickName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GroupListItem
    {
        public string Name { get; set; }

        public long GroupID { get; set; }
    }

    public class MonitorItemWarpper : INotifyPropertyChanged
    {
        public long GroupId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<Child> TargetId { get; set; }

        public class Child
        {
            public string SteamId { get; set; }

            public string Name { get; set; }

            public string NickName { get; set; }

            public MonitorItemWarpper Parent { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
