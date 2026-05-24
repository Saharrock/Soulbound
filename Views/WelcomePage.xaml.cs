using Soulbound.ViewModels;



namespace Soulbound.Views;



// Стартовая страница гостя. WelcomeViewModel — кнопки Login / Register.
public partial class WelcomePage : ContentPage

{

	public WelcomePage()

	{

		InitializeComponent();

        BindingContext = new WelcomeViewModel();

    }

}


