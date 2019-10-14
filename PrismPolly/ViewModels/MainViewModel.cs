using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MonkeyCache.SQLite;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using PrismPolly.Message;
using PrismPolly.Models;
using PrismPolly.Services;
using Xamarin.Essentials;

namespace PrismPolly.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        IRssService _service;
        public ObservableCollection<RssData> RSSFeed { get; }

        string _loadingText = "Carregando...";
        public string LoadingText
        {
            get { return _loadingText; }
            set { SetProperty(ref _loadingText, value); }
        }

        protected MainViewModel(INavigationService navigationService,
           IPageDialogService pageDialogService, IRssService rssService, IEventAggregator ea) : base(navigationService, pageDialogService)
        {
            Title = "Pagina Inicial";

            RSSFeed = new ObservableCollection<RssData>();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);

            _service = rssService;

            CarregaRSS(null);

        }

        ~MainViewModel()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var IsNotConnected = e.NetworkAccess != NetworkAccess.Internet;

            if (IsNotConnected)
                await this.PageDialogService.DisplayAlertAsync("Atenção", "Estamos sem internet :(", "OK");
        }

        protected void ShowLoading(string message = null)
        {
            IsBusy = true;
            LoadingText = message ?? "Carregando...";
        }

        protected void DismissLoading()
        {
            IsBusy = false;
        }


        private string _key = "rssFeed"; //Chave com o nome do objeto que armazena os dados.

        private DelegateCommand _refreshNewsFeedCommand;

        public DelegateCommand RefreshNewsFeedCommand =>
        _refreshNewsFeedCommand ?? (_refreshNewsFeedCommand = new DelegateCommand(
        async () =>
        {
            await CarregaRSS(null);
        }));



        //private async Task CarregaRSS(List<RssData> lista = null)
        //{
        //    IsBusy = true;
        //    RSSFeed.Clear();

        //    if (lista == null)
        //    {
        //        lista = Barrel.Current.Get<List<RssData>>(_key) ?? new List<RssData>();

        //    }

        //    foreach (var rssData in lista)
        //    {
        //        RSSFeed.Add(rssData);
        //    }

        //    IsBusy = false;
        //}

        private async Task CarregaRSS(List<RssData> lista = null)
        {
            ShowLoading();
            RSSFeed.Clear();

            lista = await _service.ObterRSS();

            foreach (var rssData in lista)
            {
                RSSFeed.Add(rssData);
            }

            DismissLoading();
        }

        private void MessageReceived(string parametro)
        {
            //Application.Current.MainPage.DisplayAlert("Atenção", "Estamos sem internet :(", "OK");
            var mensagem = parametro;

            LoadingText = parametro;
        }
    }
}
