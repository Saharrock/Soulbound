using Microsoft.Maui.Storage;



namespace Soulbound.Views;



// Справочник правил игры. Continue → флаг в Preferences → MainRoomPage.
public partial class HandbookPage : ContentPage

{

	public HandbookPage()

	{

		InitializeComponent();

	}



	private async void OnContinueClicked(object sender, EventArgs eventArgs)

	{

		const string key = "soulbound_handbook_v1";

		Preferences.Set(key, true);



		await Shell.Current.GoToAsync("//MainRoomPage");

	}

}


