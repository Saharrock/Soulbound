using Soulbound.ViewModels;
namespace Soulbound.Views;

public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();
        BindingContext = new WelcomeViewModel();
    }
}