using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class LogInPageViewModel : ViewModelBase
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
        public string userInput;
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
                    MessageForUser = "You are welcome)";
                    IsLoginEnable = true;
                }
                OnPropertyChanged();
            }
        }

        public string userPassword;
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
        public ICommand GotoAnotherPageCommand { get; set; }
        #endregion

        #region constructor
        public LogInPageViewModel()
        {
            // Defining the Command for a non async Function
            ResetUsernameCommand = new Command(ResetUserField);
            ResetPasswordCommand = new Command(ResetPasswordField);
            // Defining the Command for an async Function
            GotoAnotherPageCommand = new Command(async () => await GotoAnotherPage());
        }
        #endregion

        #region  Methods
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
        private async Task GotoAnotherPage()
        {
            MainPage mp = new MainPage();
            await Application.Current.MainPage.Navigation.PushAsync(mp);

        }
        #endregion
    }
}
