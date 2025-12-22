using Microsoft.Extensions.Logging;
using RecipePOC.DB;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
#if WINDOWS
using Microsoft.Maui.Handlers;
#endif
namespace RecipePOC
{
    public static class MauiProgram
    {
        public static IServiceProvider Services;

        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Quicksand-SemiBold.ttf", "OpenSansRegular");
                    fonts.AddFont("Quicksand-Regular.ttf", "OpenSansSemibold");
                });
            builder.Services.AddCustomApiHttpClient();

            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IRecipeService, RecipeService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();

            builder.Services.AddTransient<AIAssistantShopping>();

            // ❗ Pages
            builder.Services.AddSingleton<MainPage>();      // Shell root ONLY
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<IngredientDetail>();
            builder.Services.AddTransient<CreateRecipe>();
            builder.Services.AddTransient<Notifications>();
            builder.Services.AddTransient<Search>();
            /* builder.Services.AddCustomApiHttpClient();

             builder.Services.AddSingleton<IAuthService, AuthService>();
             builder.Services.AddSingleton<IRecipeService, RecipeService>(); 

             builder.Services.AddSingleton<INotificationService, NotificationService>();
             builder.Services.AddTransient<AIAssistantShopping>();

             builder.Services.AddSingleton<MainPage>();
             builder.Services.AddTransient<HomePage>();
             builder.Services.AddSingleton<IngredientDetail>();
             builder.Services.AddSingleton<CreateRecipe>();
             builder.Services.AddSingleton<Notifications>();
             builder.Services.AddSingleton<Search>(); **/



#if DEBUG
            builder.Logging.AddDebug();
#endif

#if WINDOWS
            SwitchHandler.Mapper.AppendToMapping("NoText", (handler, view) =>
    {
        var toggle = handler.PlatformView;
        toggle.OnContent = null;
        toggle.OffContent = null;
    });
#endif

            var app =  builder.Build();
            Services = app.Services;   // <-- initialize

            return app; 
        }
    }
    public interface IPlatformHttpMessageHandler
    {
        HttpMessageHandler GetHttpMessageHandler();
    }

    public static class MauiProgramExtensions
    {
        public static IServiceCollection AddCustomApiHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(AppConstants.HttpClientName, httpClient =>
            {
                // Use your local API during development
                httpClient.BaseAddress = new Uri("https://localhost:7222");
                //httpClient.BaseAddress = new Uri("http://10.0.2.2:5285");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                // This is optional, but good if you’re using a self-signed certificate
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            return services;
        }
    }
}
