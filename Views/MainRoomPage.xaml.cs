using Microsoft.Maui.Storage;

using Soulbound.ViewModels;



namespace Soulbound.Views;



// Главный экран: BindingContext = MainRoomViewModel.

// OnAppearing — handbook redirect, RefreshDataAsync, таймер deadline.

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



        const string handbookKey = "soulbound_handbook_v1";

        // Первый визит после onboarding — показать Handbook

        if (!Preferences.Get(handbookKey, false) && Shell.Current != null)

        {

            await Shell.Current.GoToAsync("//HandbookPage");

            return;

        }



        await viewModel.RefreshDataAsync();

        viewModel.StartDeadlineTickerIfNeeded();

    }



    protected override void OnDisappearing()

    {

        base.OnDisappearing();

        viewModel.StopDeadlineTicker();

    }

}


