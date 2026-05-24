using System.Windows.Input;



namespace Soulbound.ViewModels

{

    // WelcomePage: навигация на Login или Register.
    internal class WelcomeViewModel : ViewModelBase

    {

        public ICommand GoToLoginCommand { get; }

        public ICommand GoToRegistrationCommand { get; }



        public WelcomeViewModel()

        {

            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//LogInPage"));

            GoToRegistrationCommand = new Command(async () => await Shell.Current.GoToAsync("//RegistrationPage"));

        }

    }

}


