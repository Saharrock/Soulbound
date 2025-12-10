using Soulbound.ViewModels;

namespace Soulbound.Views;

public partial class EntranceMenuPage : ContentPage
{
	public EntranceMenuPage()
	{
		InitializeComponent();
        BindingContext = new EntranceMenuViewModel();
    }
}