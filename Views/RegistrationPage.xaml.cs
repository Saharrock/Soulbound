using Soulbound.ViewModels;
namespace Soulbound.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
        BindingContext = new RegistrationViewModel();
    }
}