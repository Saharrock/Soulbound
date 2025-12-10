using Soulbound.ViewModels;
namespace Soulbound.Views;

public partial class MainRoomPage : ContentPage
{
	public MainRoomPage()
	{
		InitializeComponent();
        BindingContext = new MainRoomViewModel();
    }
}