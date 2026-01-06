using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class GoalHistoryPage : ContentPage
{
    GoalHistoryViewModel vm;
    public GoalHistoryPage()
    {
        InitializeComponent();
        vm = new GoalHistoryViewModel();
        BindingContext = vm;

    }
    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        vm.InitAsync();

    }

}