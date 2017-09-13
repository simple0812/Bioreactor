
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Models
{
    public class Consumable :ViewModel
    {
        public int _id;
        public int Id
        {

            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string _name;
        public string Name
        {

            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string _origin;
        public string Origin
        {

            get => _origin;
            set
            {
                _origin = value;
                OnPropertyChanged();
            }
        }

        public string _ConsumableSerialNumber;
        public string ConsumableSerialNumber
        {
            get => _ConsumableSerialNumber;
            set
            {
                _ConsumableSerialNumber = value;
                OnPropertyChanged();
            }
        }

        private string _ConsumableName;
        public string ConsumableName
        {
            get => _ConsumableName;
            set
            {
                _ConsumableName = value;
                OnPropertyChanged();
            }
        }

        public int _ConsumableUsedTimes;
        public int ConsumableUsedTimes
        {

            get => _ConsumableUsedTimes;
            set
            {
                _ConsumableUsedTimes = value;
                OnPropertyChanged();
            }
        }

    }
}
