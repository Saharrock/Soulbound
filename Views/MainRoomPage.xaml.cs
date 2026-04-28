using Soulbound.ViewModels;
namespace Soulbound.Views;

public partial class MainRoomPage : ContentPage
{
    private readonly MainRoomViewModel viewModel;

	public MainRoomPage()
	{
		InitializeComponent();
        viewModel = new MainRoomViewModel();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.RefreshDataAsync();
    }
}