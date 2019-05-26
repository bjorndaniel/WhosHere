using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using WhosHere.Common;

namespace WhosHere.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfigurationRoot Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<App>();
            Configuration = builder.Build();
            var services = new ServiceCollection();
            services
                .Configure<ConfigValues>(Configuration.GetSection(nameof(ConfigValues)))
                .AddOptions()
                .AddSingleton(typeof(MainWindow))
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();
            ServiceProvider = serviceProvider;
            var window = ServiceProvider.GetService<MainWindow>();
            window.Show();
        }
    }
}
