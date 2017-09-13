using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shunxi.Business.Models.devices
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual bool Validate(ref string msg)
        {
            return true;
        }
    }
}
