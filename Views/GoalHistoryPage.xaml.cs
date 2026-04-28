using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class GoalHistoryPage : ContentPage
{
    public GoalHistoryPage()
    {
        InitializeComponent();
        BindingContext = new GoalHistoryViewModel();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (BindingContext is GoalHistoryViewModel viewModel)
        {
            await viewModel.RefreshAsync();
        }
    }
}
