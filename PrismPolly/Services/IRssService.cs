using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrismPolly.Models;

namespace PrismPolly.Services
{
    public interface IRssService
    {
        Task<List<RssData>> ObterRSS();
    }
}
