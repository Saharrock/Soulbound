using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class WelcomePageViewModel : ViewModelBase
    {
        #region get set

        #endregion

        #region Commands
        public ICommand GoToLoginCommand { get; set; }
        public ICommand GoToRegistrationCommand { get; set; }
        #endregion

        #region Constructor
        public WelcomePageViewModel()
        {

            GoToLoginCommand = new Command(async () => await GoToLogin());
            GoToRegistrationCommand = new Command(async () => await GoToRegistration());
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
        #endregion

        /*
         #region get set
         #endregion

        #region Commands
         #endregion

        #region Constructor
         #end region

        #region Methods
         #endregion
         * */
    }
}
