using Soulbound.ViewModels;



namespace Soulbound.Views;



// Расписание по дням недели. OnAppearing — RefreshAsync.
public partial class SchedulePage : ContentPage

{

    private readonly ScheduleViewModel vm;



	public SchedulePage()

	{

		InitializeComponent();

        vm = new ScheduleViewModel();

        BindingContext = vm;

    }



    protected override async void OnAppearing()

    {

        base.OnAppearing();

        await vm.RefreshAsync();

    }

}


