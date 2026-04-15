namespace Soulbound
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShellNotAuth();
        }

        public void SetAuthenticatedShell()
        {
            MainPage = new AppShellAuth();
        }

        public void SetUnauthenticatedShell()
        {
            MainPage = new AppShellNotAuth();
        }
    }
}