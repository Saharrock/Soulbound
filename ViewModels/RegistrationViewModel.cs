using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class RegistrationViewModel : ViewModelBase
    {
        private readonly AppService appService;
        private string username = string.Empty;
        private string email = string.Empty;
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        private string messageForUser = string.Empty;

        public string Username
        {
            get => username;
            set
            {
                username = value ?? string.Empty;
                ValidateAllFields();
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value ?? string.Empty;
                ValidateAllFields();
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value ?? string.Empty;
                ValidateAllFields();
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value ?? string.Empty;
                ValidateAllFields();
                OnPropertyChanged();
            }
        }

        public string MessageForUser
        {
            get => messageForUser;
            set
            {
                messageForUser = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, string> FieldMessages { get; } = new()
        {
            ["Username"] = string.Empty,
            ["Email"] = string.Empty,
            ["Password"] = string.Empty,
            ["ConfirmPassword"] = string.Empty
        };

        public ICommand ResetUsernameCommand { get; }
        public ICommand ResetEmailCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ResetConfirmPasswordCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegistrationViewModel()
        {
            appService = AppService.GetInstance();
            ResetUsernameCommand = new Command(() => Username = string.Empty);
            ResetEmailCommand = new Command(() => Email = string.Empty);
            ResetPasswordCommand = new Command(() => Password = string.Empty);
            ResetConfirmPasswordCommand = new Command(() => ConfirmPassword = string.Empty);
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//LogInPage"));
            ValidateAllFields();
        }

        private bool ValidateAllFields()
        {
            FieldMessages["Username"] = string.IsNullOrWhiteSpace(Username) || Username.Length >= 5
                ? string.Empty
                : "Username must contain at least 5 characters";

            FieldMessages["Email"] = string.IsNullOrWhiteSpace(Email) || (Email.Contains("@") && Email.Contains("."))
                ? string.Empty
                : "Invalid email";

            if (string.IsNullOrWhiteSpace(Password))
            {
                FieldMessages["Password"] = "Password is required";
            }
            else if (Password.Length < 8)
            {
                FieldMessages["Password"] = "Password must be at least 8 characters";
            }
            else if (!Password.Any(char.IsUpper))
            {
                FieldMessages["Password"] = "Password must contain at least one uppercase letter";
            }
            else if (!Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                FieldMessages["Password"] = "Password must contain at least one special character";
            }
            else
            {
                FieldMessages["Password"] = string.Empty;
            }

            FieldMessages["ConfirmPassword"] = string.IsNullOrWhiteSpace(ConfirmPassword) || ConfirmPassword == Password
                ? string.Empty
                : "Passwords do not match";

            OnPropertyChanged(nameof(FieldMessages));

            return FieldMessages.Values.All(string.IsNullOrEmpty) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private async Task RegisterAsync()
        {
            MessageForUser = string.Empty;
            if (!ValidateAllFields())
            {
                MessageForUser = "Please fix the highlighted fields";
                return;
            }

            bool success = await appService.TryRegister(Username.Trim(), Email.Trim(), Password);
            if (!success)
            {
                MessageForUser = "Registration failed. Check email/password format or existing account.";
                return;
            }

            ((App)Application.Current).SetAuthenticatedShell();
            await Shell.Current.GoToAsync("//PetSelectionPage");
        }
    }
}
