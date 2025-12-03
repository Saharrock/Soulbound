using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class NewGoalPage : ContentPage
{
	public NewGoalPage()
	{
		InitializeComponent();
        BindingContext = new NewGoalPageViewModel();
    }
}