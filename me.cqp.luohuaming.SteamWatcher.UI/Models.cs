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


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
