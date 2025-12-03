using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class SchedulePage : ContentPage
{
	public SchedulePage()
	{
		InitializeComponent();
        BindingContext = new SchedulePageViewModel();
    }
}