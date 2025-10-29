using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulbound.ViewModels
{
    internal class LogInPageViewModel : ViewModelBase
    {
        // get and set for MessageForEldan
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
        public bool logInButton;
        public bool LogInButton
        {
            get { return logInButton; }
            set
            {
                logInButton = value;
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
                    logInButton = false;

                }
                else
                {
                    MessageForUser = "You are welcome)";
                    logInButton = true;
                }
                OnPropertyChanged();
            }
        }

        // constructor
        public LogInPageViewModel()
        {
        }
    }
}
