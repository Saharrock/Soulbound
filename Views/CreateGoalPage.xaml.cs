using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class CreateGoalPage : ContentPage
{
	public CreateGoalPage()
	{
		InitializeComponent();
        BindingContext = new CreateGoalViewModel();
    }
}