using Soulbound.Services;

namespace Soulbound
{
    public partial class AppShellAuth : Shell
    {
        public AppShellAuth()
        {
            InitializeComponent();
        }

        private void MenuItem_Logout_Clicked(object sender, EventArgs e)
        {
            var successed = AppService.GetInstance().Logout();
            if (successed)
            {
                ((App)Application.Current).SetUnauthenticatedShell();
            }
        }
    }
}
