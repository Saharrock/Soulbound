using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class RegistrationViewModel : ViewModelBase
    {
        #region Properties
        private string messageForUser;
        public string MessageForUser
        {
            get => messageForUser;
            set
            {
                if (value != null)
                {
                    messageForUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, string> FieldMessages { get; set; } = new();

        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                ValidateField("Username", value);
                OnPropertyChanged();
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                ValidateField("Email", value);
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                ValidateField("Password", value);
                OnPropertyChanged();
            }
        }

        private string confirmPassword;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value;
                ValidateField("ConfirmPassword", value);
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand ResetUsernameCommand { get; set; }
        public ICommand ResetEmailCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }
        public ICommand ResetConfirmPasswordCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand GoToLoginCommand { get; set; }
        #endregion

        #region Constructor
        public RegistrationViewModel()
        {
            ResetUsernameCommand = new Command(() => ResetField("Username"));
            ResetEmailCommand = new Command(() => ResetField("Email"));
            ResetPasswordCommand = new Command(() => ResetField("Password"));
            ResetConfirmPasswordCommand = new Command(() => ResetField("ConfirmPassword")); //Not My Code, But this is what i want to do!

            RegisterCommand = new Command(async () => await Register());
            GoToLoginCommand = new Command(async () => await GoToLogin());
        }
        #endregion

        #region Methods
        private void ValidateField(string field, string value)
        {
            switch (field)
            {
                case "Username":
                    FieldMessages["Username"] = string.IsNullOrWhiteSpace(value)
                        ? "Required"
                        : value.Length < 4 ? "Too short" : "";
                    break;
                case "Email":
                    FieldMessages["Email"] = string.IsNullOrWhiteSpace(value)
                        ? "Required"
                        : (!value.Contains("@") ? "Invalid" : "");
                    break;
                case "Password":
                    FieldMessages["Password"] = string.IsNullOrWhiteSpace(value)
                        ? "Required"
                        : value.Length < 6 ? "Too weak" : "";
                    break;
                case "ConfirmPassword":
                    FieldMessages["ConfirmPassword"] = value != Password
                        ? "Doesn't match"
                        : "";
                    break;
            }

            OnPropertyChanged(nameof(FieldMessages));
        }

        private void ResetField(string field)
        {
            switch (field)
            {
                case "Username": Username = ""; break;
                case "Email": Email = ""; break;
                case "Password": Password = ""; break;
                case "ConfirmPassword": ConfirmPassword = ""; break;
            }

            FieldMessages[field] = "";
            OnPropertyChanged(nameof(FieldMessages));
        }

        private async Task Register()
        {
            MessageForUser = "Processing registration...";
            await Task.Delay(1000);
            MessageForUser = "Registration complete!";
        }

        private async Task GoToLogin()
        {
            await Shell.Current.GoToAsync("//LogInPage");
        }
        #endregion
    }
}
