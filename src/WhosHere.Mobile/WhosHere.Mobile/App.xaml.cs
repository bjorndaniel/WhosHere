using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WhosHere.Mobile.Services;
using WhosHere.Mobile.Views;

namespace WhosHere.Mobile
{
    public partial class App : Application
    {
        public static string FunctionUrl { get; internal set; } = "YOUR_COMPUTER_RUNNING_THE_FUNCTION_IP_ADDRESS:7071";

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
