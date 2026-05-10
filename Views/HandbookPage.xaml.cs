using Microsoft.Maui.Storage;

namespace Soulbound.Views;

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
