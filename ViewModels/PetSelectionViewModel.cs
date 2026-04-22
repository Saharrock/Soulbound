using System.Windows.Input;
using Microsoft.Maui.Controls;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class PetSelectionViewModel : ViewModelBase
    {
        private readonly CharacterService characterService;

        private ImageSource petAvatar = PetImageHelper.CreateSafeImageSource(null);

        /// <summary>
        /// Preview image with a safe fallback if the file name is wrong.
        /// </summary>
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
            characterService = CharacterService.GetInstance();
            LeftCommand = new Command(MoveLeft);
            RightCommand = new Command(MoveRight);
            ConfirmCommand = new Command(async () => await ConfirmAsync());
            LoadCurrentPet();
        }

        private void LoadCurrentPet()
        {
            PetOption pet = characterService.GetCurrentPetTemplate();
            PetAvatar = PetImageHelper.CreateSafeImageSource(pet.Image);
            if (string.IsNullOrWhiteSpace(PetName))
            {
                PetName = pet.DefaultName;
            }
        }

        private void MoveLeft()
        {
            PetOption pet = characterService.MovePetLeft();
            PetAvatar = PetImageHelper.CreateSafeImageSource(pet.Image);
            PetName = pet.DefaultName;
        }

        private void MoveRight()
        {
            PetOption pet = characterService.MovePetRight();
            PetAvatar = PetImageHelper.CreateSafeImageSource(pet.Image);
            PetName = pet.DefaultName;
        }

        private async Task ConfirmAsync()
        {
            characterService.ConfirmPetSelection(PetName);
            await Shell.Current.GoToAsync("//MainRoomPage");
        }
    }
}
