using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class GoalHistoryPage : ContentPage
{
	public GoalHistoryPage()
	{
		InitializeComponent();
        BindingContext = new GoalHistoryPageViewModel();
    }
}