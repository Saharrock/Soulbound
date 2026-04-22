using System.Windows.Input;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    class PetSelectionViewModel : ViewModelBase
    {
        private readonly LocalDataService dataService;

        private string petImage = string.Empty;
        public string PetImage
        {
            get => petImage;
            set { petImage = value; OnPropertyChanged(); }
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
            dataService = LocalDataService.GetInstance();
            LeftCommand = new Command(MoveLeft);
            RightCommand = new Command(MoveRight);
            ConfirmCommand = new Command(async () => await ConfirmAsync());
            LoadCurrentPet();
        }

        private void LoadCurrentPet()
        {
            var pet = dataService.GetCurrentPet();
            PetImage = pet.Image;
            if (string.IsNullOrWhiteSpace(PetName))
            {
                PetName = pet.DefaultName;
            }
        }

        private void MoveLeft()
        {
            var pet = dataService.MovePetLeft();
            PetImage = pet.Image;
            PetName = pet.DefaultName;
        }

        private void MoveRight()
        {
            var pet = dataService.MovePetRight();
            PetImage = pet.Image;
            PetName = pet.DefaultName;
        }

        private async Task ConfirmAsync()
        {
            dataService.ConfirmPetSelection(PetName);
            await Shell.Current.GoToAsync("//MainRoomPage");
        }
    }
}
