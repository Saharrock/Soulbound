using Microsoft.Extensions.Logging;
using Soulbound.Services;

namespace Soulbound
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            DatabaseService.GetInstance().Initialize();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
