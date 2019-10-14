using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Toolkit.Parsers.Rss;
using Polly;
using Prism.Events;
using PrismPolly.Message;
using PrismPolly.Models;
using PrismPolly.Services.Polly;

namespace PrismPolly.Services
{

    public class RssService : IRssService
    {
        IEventAggregator _eventAggregator;
        readonly INetworkService _networkService;


        public RssService(IEventAggregator ea)
        {
            _eventAggregator = ea;

            _networkService = new NetworkService();
        }



       

        //Task OnRetry(Exception e, int retryCount)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        var mensagem = $"Ocorreu um erro ao baixar os dados: {e.Message}, tentando novamente ({retryCount})..";
        //        _eventAggregator.GetEvent<MessageSentEvent>().Publish(mensagem);
        //        Console.WriteLine(mensagem);
        //    });
        //}


        //public async Task<string> ObterRSSOnRetry()
        //{
        //    var httpClient = new HttpClient();
        //    var response = await httpClient.GetAsync("https://medium.com/feed/@bertuzzi");

        //    return await response.Content.ReadAsStringAsync();
        //}



        public async Task<List<RssData>> ObterRSS()
        {
            string feed = null;
            List<RssData> rssRetorno = new List<RssData>();
            int cont = 0;
            try
            {
                using (var client = new HttpClient())
                {
                    await Policy
                           // .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                           .Handle<HttpRequestException>()
                           .WaitAndRetryAsync
                           (
                               retryCount: 3,
                               sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                               onRetry: (ex, time) =>
                               {
                                   cont++;
                                   var mensagem = $"Ocorreu um erro ao baixar os dados: {ex.Message}, tentando novamente ({cont})..";
                                   _eventAggregator.GetEvent<MessageSentEvent>().Publish(mensagem);
                                   Console.WriteLine(mensagem);
                               }
                           )
                           .ExecuteAsync(async () =>
                           {
                               Console.WriteLine($"Obtendo rss...");

                               feed = await client.GetStringAsync("https://medium.com/feed/@bertuzzi");
                           });



                }

                //var func = new Func<Task<string>>(() => ObterRSSOnRetry());
                //feed = await _networkService.Retry<string>(func, 3, OnRetry);

                var parser = new RssParser();
                var rss = parser.Parse(feed).OrderByDescending(e => e.PublishDate).ToList();

                if (feed != null)
                {
                    foreach (var rssSchema in rss)
                    {
                        var rssdata = new RssData
                        {
                            Title = rssSchema.Title,
                            PubDate = rssSchema.PublishDate,
                            Link = rssSchema.FeedUrl,
                            Guid = rssSchema.InternalID,
                            Author = rssSchema.Author,
                            Thumbnail = string.IsNullOrWhiteSpace(rssSchema.ImageUrl) ? $"https://placeimg.com/80/80/nature" : rssSchema.ImageUrl,
                            Description = rssSchema.Summary
                        };

                        rssRetorno.Add(rssdata);

                    }

                }

                return rssRetorno;
            }
            catch(Exception ex)
            {
                return new List<RssData>();
            }
        }
    }
}
