using Soulbound.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class LogInViewModel : ViewModelBase
    {
        #region get set
        private string messageForUser;
        public string MessageForUser
        {
            get { return messageForUser; }
            set
            {
                // Basic
                //messageForEldan = value;
                //PropertyChanged();

                // Even Better
                if (value != null)
                {
                    messageForUser = value;
                    OnPropertyChanged();
                }
            }
        }

        // get and set for Button
        private bool isLoginEnable;
        public bool IsLoginEnable
        {
            get { return isLoginEnable; }
            set
            {
                isLoginEnable = value;
                OnPropertyChanged();
            }
        }

        // get and set for UserInput
        private string userInput;
        public string UserInput
        {
            get { return userInput; }
            set
            {
                userInput = value;
                if (userInput != null && userInput.Length < 5)
                {
                    MessageForUser = "The field has less then 5 characters";
                    IsLoginEnable = false;

                }
                else
                {
                    MessageForUser = "";
                    IsLoginEnable = true;
                }
                OnPropertyChanged();
            }
        }

        private string userPassword;
        public string UserPassword
        {
            get { return userPassword; }
            set
            {
                userPassword = value;
                OnPropertyChanged();
            }
            
        }
        #endregion

        #region Commands
        public ICommand ResetUsernameCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }
        public ICommand GotoRegisterPageCommand { get; set; }
        public ICommand TryLoginCommand { get; set; }
        #endregion

        #region constructor
        public LogInViewModel()
        {
            // Defining the Command for a non async Function
            ResetUsernameCommand = new Command(ResetUserField);
            ResetPasswordCommand = new Command(ResetPasswordField);
            // Defining the Command for an async Function
            GotoRegisterPageCommand = new Command(async () => await Shell.Current.GoToAsync("//RegistrationPage"));
            TryLoginCommand = new Command(async () => await TryLoginAsync());
        }
        #endregion

        #region  Methods

        private async Task TryLoginAsync()//
        {
            bool successed = await LocalDataService.GetInstance().TryLoginAsync(UserInput, UserPassword);
            if (successed)
            {
                await Shell.Current.GoToAsync("//MainRoomPage");
            } else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Login Failed",
                    "Invalid username or password",
                    "OK"
                );
            }
        }
        private void ResetUserField()
        {
            UserInput = "";
            MessageForUser = "";

        }

        private void ResetPasswordField()
        {
            UserPassword = "";
            MessageForUser = "";
        }

        #endregion
    }
}
