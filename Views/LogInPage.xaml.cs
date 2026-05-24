using Soulbound.ViewModels;



namespace Soulbound.Views;



// Страница входа. LogInViewModel.
public partial class LogInPage : ContentPage

{

	public LogInPage()

	{

        InitializeComponent();

        BindingContext = new LogInViewModel();

    }

}


