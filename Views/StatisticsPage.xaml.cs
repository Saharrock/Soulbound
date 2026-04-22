using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsViewModel viewModel;

	public StatisticsPage()
	{
		InitializeComponent();
        viewModel = new StatisticsViewModel();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Refresh();
    }
}