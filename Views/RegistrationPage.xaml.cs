using Soulbound.ViewModels;



namespace Soulbound.Views;



// Регистрация. RegistrationViewModel.
public partial class RegistrationPage : ContentPage

{

	public RegistrationPage()

	{

		InitializeComponent();

        BindingContext = new RegistrationViewModel();

    }

}


