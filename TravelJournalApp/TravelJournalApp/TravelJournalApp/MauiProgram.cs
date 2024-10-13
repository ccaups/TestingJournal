using Microsoft.Extensions.Logging;

namespace TravelJournalApp
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
				{
                    fonts.AddFont("Roboto-Bold.ttf.ttf", "RobotoBold");
                    fonts.AddFont("Roboto-Light.ttf.ttf", "RobotoLight");
                    fonts.AddFont("Roboto-Medium.ttf.ttf", "RobotMedium");
                    fonts.AddFont("Roboto-Regular.ttf", "RobotRegular");

                });

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
