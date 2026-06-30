using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using tas.Data;
using tas.Services;

namespace tas
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddDbContext<TasDbContext>();
            services.AddScoped<UserService>();
            services.AddScoped<TaskService>();
            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }
        public static void SwitchTheme(string themeName)
        {
            string themePath = themeName == "Dark" ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
            var dict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };
            Application.Current.Resources = dict;
        }
    }
}