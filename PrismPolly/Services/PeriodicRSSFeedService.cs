using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Matcha.BackgroundService;
using Microsoft.Toolkit.Parsers.Rss;
using MonkeyCache.SQLite;
using Polly;
using Prism.Events;
using PrismPolly.Message;
using PrismPolly.Models;
using Xamarin.Forms;

namespace PrismPolly.Services
{
    public class PeriodicRSSFeedService : IPeriodicTask
    {
        private string _key = "rssFeed"; //Chave com o nome do objeto que armazena os dados.

        public PeriodicRSSFeedService(int minutes)
        {
            Interval = TimeSpan.FromMinutes(minutes);
        }

        public TimeSpan Interval { get; set; }

        public async Task<bool> StartJob()
        {
            bool existeNoticiaNova = false;
            string feed = null;

            try
            {

                var existingList = Barrel.Current.Get<List<RssData>>(_key) ?? new List<RssData>();

                using (var client = new HttpClient())
                {

                    var timeoutPolicy = Policy.TimeoutAsync(15);
                    HttpResponseMessage httpResponse = await timeoutPolicy
                        .ExecuteAsync(
                          async ct => await client.GetAsync("https://medium.com/feed/@bertuzzi", ct),
                          CancellationToken.None
                          );

                    feed = await httpResponse.Content.ReadAsStringAsync();
                }


                var parser = new RssParser();
                var rss = parser.Parse(feed).OrderByDescending(e => e.PublishDate).ToList();

                if (feed != null)
                {
                    foreach (var rssSchema in rss)
                    {
                        var isExist = existingList.Any(e => e.Guid == rssSchema.InternalID);

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

                        if (!isExist)
                        {
                            //Se Existiu alguma noticia nova, pelo menos 1 vez
                            existeNoticiaNova = true;
                            existingList.Add(rssdata);
                        }
                    }

                    if (existeNoticiaNova)
                        MessagingCenter.Send(existingList, "Update");
                }

                existingList = existingList.OrderByDescending(e => e.PubDate).ToList();

                Barrel.Current.Add(_key, existingList, TimeSpan.FromDays(30));

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
