using Soulbound.ViewModels;



namespace Soulbound.Views;



// Выбор питомца после регистрации или если onboarding не завершён.
public partial class PetSelectionPage : ContentPage

{

    public PetSelectionPage()

    {

        InitializeComponent();

        BindingContext = new PetSelectionViewModel();

    }

}


