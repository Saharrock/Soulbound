using Soulbound.ViewModels;



namespace Soulbound.Views;



// Форма создания цели. CreateGoalViewModel.
public partial class CreateGoalPage : ContentPage

{

	public CreateGoalPage()

	{

		InitializeComponent();

        BindingContext = new CreateGoalViewModel();

    }

}


