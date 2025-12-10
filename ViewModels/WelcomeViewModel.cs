using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class WelcomeViewModel : ViewModelBase
    {
        #region get set

        #endregion

        #region Commands
        public ICommand GoToLoginCommand { get; set; }
        public ICommand GoToRegistrationCommand { get; set; }
        public ICommand GoToAsGuestCommand { get; set; }
        #endregion

        #region Constructor
        public WelcomeViewModel()
        {

            GoToLoginCommand = new Command(async () => await GoToLogin());
            GoToRegistrationCommand = new Command(async () => await GoToRegistration());
            GoToAsGuestCommand = new Command(async () => await GoToAsGuest());
        }
        #endregion

        #region Methods
        private async Task GoToLogin()
        {
            await Shell.Current.GoToAsync("//LogInPage");
        }

        private async Task GoToRegistration()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }

        private async Task GoToAsGuest()
        {
            await Shell.Current.GoToAsync("//MainRoomPage");
        }
        #endregion

        /*
         #region get set

        #endregion


        #region Commands

        #endregion


        #region Constructor

        # endregion


        #region Methods

        #endregion
          */
    }
}
