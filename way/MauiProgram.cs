using CommunityToolkit.Maui;
using MauiIcons.Fluent.Filled;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using way.Data;
using way.Handlers;
using way.ViewModels;
using way.Views;
using way.Services;

namespace way
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .UseFluentFilledMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            AddServices(builder.Services);

            FormHandler.RemoveBorders();

            return builder.Build();
        }

        private static void AddServices(IServiceCollection services)
        {
            //Services
            services.AddSingleton<DataBaseContext>();
            services.AddSingleton<ImuSensorService>();
		    services.AddSingleton<ImuDataProcessor>();
		    services.AddSingleton<ExerciseAnalyzer>();

            //ViewModels
            services.AddSingleton<TrainingsViewModel>();
            services.AddSingleton<TrainingViewModel>();
            services.AddTransient<WorkoutViewModel>();
            services.AddTransient<TrainingDetailsViewModel>();

            //Views
            services.AddSingleton<TrainingsPage>();
            services.AddSingleton<TrainingPage>();
            services.AddTransient<WorkoutPage>();
            services.AddTransient<TrainingDetailsPage>();

            //Popups
            services.AddTransientPopup<TimeRestPicker, TimeRestPickerViewModel>();
        }
    }
}
