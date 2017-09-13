namespace Shunxi.Business.Models.devices
{
    public class CellCultivation:ViewModel
    {

        private int _id;
        public int Id
        {

            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _Description ;
        public string Description
        {

            get => _Description;
            set
            {
                _Description = value;
                OnPropertyChanged();
            }
        }

        private string _Name ;
        public string Name
        {

            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private string _UserName;
        public string UserName
        {

            get => _UserName;
            set
            {
                _UserName = value;
                OnPropertyChanged();
            }
        }

        private string _Cell;
        public string Cell
        {

            get => _Cell;
            set
            {
                _Cell = value;
                OnPropertyChanged();
            }
        }

        private string _BatchNumber;
        public string BatchNumber
        {

            get => _BatchNumber;
            set
            {
                _BatchNumber = value;
                OnPropertyChanged();
            }
        }

        private int _CreatedAt;
        public int CreatedAt
        {

            get => _CreatedAt;
            set
            {
                _CreatedAt = value;
                OnPropertyChanged();
            }
        }

        public override bool Validate(ref string msg)
        {
            return true;
        }
    }
}
