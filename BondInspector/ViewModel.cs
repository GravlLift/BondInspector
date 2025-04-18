using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BondInspector
{
    public sealed class ViewModel : INotifyPropertyChanged
    {
        private string bondText;

        public string BondText
        {
            get { return bondText; }
            set
            {
                bondText = value;
                NotifyPropertyChanged();
            }
        }

        public void Clear()
        {
            BondText = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
