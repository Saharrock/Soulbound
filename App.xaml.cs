namespace Soulbound

{

    // Точка входа приложения: стартовый Shell и переключение auth / non-auth.

    public partial class App : Application

    {

        public App()

        {

            InitializeComponent();

            // Гость видит Welcome / Login / Register

            MainPage = new AppShellNotAuth();

        }



        // После успешного login/register — авторизованное меню.
        public void SetAuthenticatedShell()

        {

            MainPage = new AppShellAuth();

        }



        // После Logout — возврат к гостевому TabBar.
        public void SetUnauthenticatedShell()

        {

            MainPage = new AppShellNotAuth();

        }

    }

}


