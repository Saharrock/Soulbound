using System.Windows.Input;

using Soulbound.Services;



namespace Soulbound.ViewModels

{

    // LogInPage: ввод email/password, TryLogin, переход в auth Shell.
    internal class LogInViewModel : ViewModelBase

    {

        private string messageForUser = string.Empty;

        private bool isLoginEnable;

        private string userInput = string.Empty;

        private string userPassword = string.Empty;



        public string MessageForUser

        {

            get => messageForUser;

            set

            {

                messageForUser = value ?? string.Empty;

                OnPropertyChanged();

            }

        }



        public bool IsLoginEnable

        {

            get => isLoginEnable;

            set

            {

                isLoginEnable = value;

                OnPropertyChanged();

            }

        }



        // В UI как email; минимум 5 символов для активации кнопки

        public string UserInput

        {

            get => userInput;

            set

            {

                userInput = value ?? string.Empty;

                if (userInput.Length < 5)

                {

                    MessageForUser = "The field has less than 5 characters";

                    IsLoginEnable = false;

                }

                else

                {

                    MessageForUser = string.Empty;

                    UpdateLoginEnableForFields();

                }



                OnPropertyChanged();

            }

        }



        public string UserPassword

        {

            get => userPassword;

            set

            {

                userPassword = value ?? string.Empty;

                UpdateLoginEnableForFields();

                OnPropertyChanged();

            }

        }



        public ICommand ResetUsernameCommand { get; }

        public ICommand ResetPasswordCommand { get; }

        public ICommand GotoRegisterPageCommand { get; }

        public ICommand TryLoginCommand { get; }



        public LogInViewModel()

        {

            ResetUsernameCommand = new Command(ResetUserField);

            ResetPasswordCommand = new Command(ResetPasswordField);

            GotoRegisterPageCommand = new Command(async () => await Shell.Current.GoToAsync("//RegistrationPage"));

            TryLoginCommand = new Command(async () => await TryLoginAsync());

            IsLoginEnable = false;

        }



        private void UpdateLoginEnableForFields()

        {

            IsLoginEnable = userInput.Length >= 5 && !string.IsNullOrWhiteSpace(userPassword);

        }



        // Firebase login → SetAuthenticatedShell → MainRoom или PetSelection.
        private async Task TryLoginAsync()

        {

            bool succeeded = await AppService.GetInstance().TryLogin(UserInput.Trim(), UserPassword);

            if (succeeded)

            {

                MessageForUser = string.Empty;

                ((App)Application.Current!).SetAuthenticatedShell();

                string route = AppService.GetInstance().HasCompletedPetOnboarding()

                    ? "//MainRoomPage"

                    : "//PetSelectionPage";

                await Shell.Current.GoToAsync(route);

            }

            else

            {

                MessageForUser = "Login failed. Check email and password.";

            }

        }



        private void ResetUserField()

        {

            UserInput = string.Empty;

            MessageForUser = string.Empty;

        }



        private void ResetPasswordField()

        {

            UserPassword = string.Empty;

            MessageForUser = string.Empty;

        }

    }

}


