using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulbound.ViewModels
{
    class MainRoomViewModel : ViewModelBase
    {
        #region get set
        double physicalValue;
        public double PhysicalValue
        {
            get => physicalValue;
            set { physicalValue = value; OnPropertyChanged(); }
        }

        double mentalValue;
        public double MentalValue
        {
            get => mentalValue;
            set { mentalValue = value; OnPropertyChanged(); }
        }

        double intellectualValue;
        public double IntellectualValue
        {
            get => intellectualValue;
            set { intellectualValue = value; OnPropertyChanged(); }
        }

        #endregion


        #region Commands

        #endregion


        #region Constructor

        # endregion


        #region Methods

        #endregion
    }
}
