using Soulbound.Services;



namespace Soulbound

{

    // Shell для залогиненного пользователя: Flyout-меню со всеми игровыми страницами + Logout.

    public partial class AppShellAuth : Shell

    {

        public AppShellAuth()

        {

            InitializeComponent();

        }



        private void MenuItem_Logout_Clicked(object sender, EventArgs e)

        {

            bool succeeded = AppService.GetInstance().Logout();

            if (succeeded)

            {

                ((App)Application.Current).SetUnauthenticatedShell();

            }

        }

    }

}


