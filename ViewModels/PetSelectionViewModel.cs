using System.Windows.Input;
using Microsoft.Maui.Controls;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class PetSelectionViewModel : ViewModelBase
    {
        private readonly AppService appService;
        private readonly string[] petImages;
        private int petIndex;

        private ImageSource petAvatar = ImageSource.FromFile("dotnet_bot.png");
        public ImageSource PetAvatar
        {
            get => petAvatar;
            set { petAvatar = value; OnPropertyChanged(); }
        }

        private string petName = string.Empty;
        public string PetName
        {
            get => petName;
            set { petName = value; OnPropertyChanged(); }
        }

        public ICommand LeftCommand { get; }

        public ICommand RightCommand { get; }

        public ICommand ConfirmCommand { get; }

        public PetSelectionViewModel()
        {
            appService = AppService.GetInstance();
            petImages = appService.GetPetImages();
            LeftCommand = new Command(MoveLeft);
            RightCommand = new Command(MoveRight);
            ConfirmCommand = new Command(async () => await ConfirmAsync());
            _ = LoadCurrentPetAsync();
        }

        private async Task LoadCurrentPetAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            string selected = appService.GetProgress().SelectedPetImage;
            int found = Array.IndexOf(petImages, selected);
            petIndex = found >= 0 ? found : 0;
            PetAvatar = ImageSource.FromFile(petImages[petIndex]);
            if (string.IsNullOrWhiteSpace(PetName))
            {
                PetName = appService.GetProgress().PetName;
            }
        }

        private void MoveLeft()
        {
            petIndex--;
            if (petIndex < 0)
            {
                petIndex = petImages.Length - 1;
            }

            PetAvatar = ImageSource.FromFile(petImages[petIndex]);
        }

        private void MoveRight()
        {
            petIndex++;
            if (petIndex >= petImages.Length)
            {
                petIndex = 0;
            }

            PetAvatar = ImageSource.FromFile(petImages[petIndex]);
        }

        private async Task ConfirmAsync()
        {
            await appService.UpdatePetSelectionAsync(petImages[petIndex], PetName);
            await Shell.Current.GoToAsync("//MainRoomPage");
        }
    }
}
