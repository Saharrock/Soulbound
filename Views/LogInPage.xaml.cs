using Soulbound.ViewModels;
namespace Soulbound.Views;


public partial class LogInPage : ContentPage
{
	public LogInPage()
	{
        InitializeComponent();
        BindingContext = new LogInViewModel();
    }
}