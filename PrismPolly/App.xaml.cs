using System;
using Matcha.BackgroundService;
using MonkeyCache.SQLite;
using Prism;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Services;
using PrismPolly.Message;
using PrismPolly.Services;
using PrismPolly.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PrismPolly
{
    public partial class App : PrismApplication
    {
        IEventAggregator _eventAggregator { get; set; }
        public App()
            : this(null)
        {

        }

        public App(IPlatformInitializer initializer)
            : this(initializer, true)
        {

        }

        public App(IPlatformInitializer initializer, bool setFormsDependencyResolver)
            : base(initializer, setFormsDependencyResolver)
        {

        }


        protected override async void OnInitialized()
        {
            InitializeComponent();

            Barrel.ApplicationId = "NewsfeedAppId";

          //  StartBackgroundService();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainViewModel>();
            containerRegistry.RegisterSingleton<IRssService, RssService>();
        }

        private static void StartBackgroundService()
        {
            //Atualiza o RSS a cada 1 Minuto
            BackgroundAggregatorService.Add(() => new PeriodicRSSFeedService(1));

            //Inicia o Background Service
            BackgroundAggregatorService.StartBackgroundService();
        }

    }
}
