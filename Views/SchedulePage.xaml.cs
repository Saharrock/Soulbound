using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel vm;

	public SchedulePage()
	{
		InitializeComponent();
        vm = new ScheduleViewModel();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.Refresh();
    }
}