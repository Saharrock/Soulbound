using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class StatisticsPage : ContentPage
{
	public StatisticsPage()
	{
		InitializeComponent();
        BindingContext = new StatisticPageViewModel();
    }
}