using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class PetSelectionPage : ContentPage
{
    public PetSelectionPage()
    {
        InitializeComponent();
        BindingContext = new PetSelectionViewModel();
    }
}
