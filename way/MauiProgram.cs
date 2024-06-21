using CommunityToolkit.Maui;
using MauiIcons.Fluent.Filled;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using way.Data;
using way.Handlers;
using way.ViewModels;
using way.Views;

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

            //ViewModels
            services.AddSingleton<TrainingsViewModel>();
            services.AddSingleton<TrainingViewModel>();
            services.AddSingleton<StatisticsViewModel>();
            services.AddTransient<ExerciseViewModel>();
            services.AddTransient<StatisticViewModel>();

            //Views
            services.AddSingleton<TrainingsPage>();
            services.AddSingleton<TrainingPage>();
            services.AddSingleton<StatisticsPage>();
            services.AddTransient<ExercisePage>();
            services.AddTransient<StatisticPage>();

            //Popups
            services.AddTransientPopup<TimeRestPicker, TimeRestPickerViewModel>();
        }
    }
}
