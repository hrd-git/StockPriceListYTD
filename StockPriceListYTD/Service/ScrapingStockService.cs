using AngleSharp.Io;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using System;
using System.Threading.Tasks;
using System.Linq;
using AngleSharp.Xml;
using StockPriceListYTD.Entity;
using System.Collections.Generic;
using StockPriceListYTD.Repository;
using Autofac;

namespace StockPriceListYTD.Service
{
    internal class ScrapingStockService
    {
        private static IContainer _container { get; set; }
        private readonly static string? BASE_URL = System.Configuration.ConfigurationManager.AppSettings["BASE_URL"];
        private readonly string SEARCH_URL = BASE_URL + System.Configuration.ConfigurationManager.AppSettings["SEARCH_URL"];
        private readonly string STOCK_INFO_URL = BASE_URL + System.Configuration.ConfigurationManager.AppSettings["STOCK_INFO_URL"];
        private readonly string nowDate = DateTime.Now.ToString("yyyyMMdd");
        private readonly string DEFAULT_INDUSTRYCODE = "0000";

        private readonly HtmlParser parser = new HtmlParser();
        private readonly IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private readonly IEnumerable<Industry> _industries;

        public ScrapingStockService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StockPriceListYTDRepository>().SingleInstance();
            _container = builder.Build();
            _industries = _container.Resolve<StockPriceListYTDRepository>().GetIndustries();
            var requester = context.GetService<DefaultHttpRequester>();
            if (requester != null)
            {
                requester.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.5481.178 Safari/537.36";
            }
        }

        public void Exec()
        {
            List<StockInfo> stockInfoList =  new List<StockInfo>();
            // 証券コードでをから9まで検索する
            for (int i = 1; i <= 9; i++)
            {
                var searchListPage = GetSearchListPage(SEARCH_URL,
                    new
                    {
                        KEY5 = i,
                        kind = "TTCODE",
                        sort = "+",
                        MAXDISP = 100
                    });

                // 次のページも含め全ての株式情報を取得する
                while (searchListPage is not null && searchListPage.Result is not null)
                {
                    var pageNode = searchListPage.Result.Body.SelectSingleNode("/html/body/table[2]/tbody/tr/td[2]/table[2]/tbody/tr/td/table[3]");
                    var docmkup = parser.ParseDocument(pageNode.ToMarkup());

                    foreach (var tr in docmkup.QuerySelectorAll("tr").Skip(1))
                    {
                        Task.Delay(3000).Wait();
                        var td = tr.GetElementsByTagName("td");
                        var stockInfo = GetStockInfo(STOCK_INFO_URL + td[0].TextContent.Trim(), td[1].TextContent.Trim()).Result;
                        stockInfoList.Add(stockInfo);
                        Console.WriteLine($"{stockInfo.SecuritiesCode} {stockInfo.CompanyName}");
                    }

                    // 一定時間待機後、次のページを取得する
                    Task.Delay(3000).Wait();
                    searchListPage = GetNextPageAsync(searchListPage);
                }
            }
            
            if (stockInfoList.Any())
            {
                using (var scope = _container.BeginLifetimeScope())
                {
                    _container.Resolve<StockPriceListYTDRepository>().InsertStockInfo(stockInfoList);
                }
            }
        }

        /// <summary>
        /// 検索結果一覧ページ取得
        /// </summary>
        /// <param name="url">検索ページURL</param>
        /// <param name="submitObj">検索条件</param>
        /// <returns></returns>
        private async Task<IDocument> GetSearchListPage(string url, object submitObj)
        {
            var doc = await context.OpenAsync(url);
            var form = doc.Forms[0];
            return await form.SubmitAsync(submitObj);
        }

        /// <summary>
        /// 株式情報取得
        /// </summary>
        /// <param name="url">株式情報ページURL</param>
        /// <param name="company">取得対象企業名</param>
        /// <returns></returns>
        private async Task<StockInfo> GetStockInfo(string url, string company)
        {
            var pageDoc = await context.OpenAsync(url);

            var companyNode = pageDoc.Body.SelectSingleNode("/html/body/table[2]/tbody/tr/td[2]/table[2]/tbody/tr[3]/td[1]/table");
            var companyDoc = parser.ParseDocument(companyNode.ToMarkup())
                .QuerySelectorAll("tr")[1]
                .GetElementsByTagName("td");

            var pageNode = pageDoc.Body.SelectSingleNode("/html/body/table[2]/tbody/tr/td[2]/table[2]/tbody/tr[4]/td/table[2]");
            var stockDoc = parser.ParseDocument(pageNode.ToMarkup())
                .QuerySelectorAll("tr")[3]
                .GetElementsByTagName("td");

            Decimal.TryParse(stockDoc[1].TextContent.Trim(), out decimal openingPrice);
            Decimal.TryParse(stockDoc[3].TextContent.Trim(), out decimal closingPrice);

            return new StockInfo()
            {
                SearchDate = nowDate,
                SecuritiesCode = companyDoc[0].TextContent.Trim(),
                CompanyName = company,
                IndustryCode = this._industries
                    .Where(row => companyDoc[2].TextContent.Trim().Equals(row.JapaneseName))
                    .Select(row => row.IndustryCode)
                    .FirstOrDefault() ?? DEFAULT_INDUSTRYCODE,
                MarketSegment = companyDoc[3].TextContent.Trim(),
                OpeningPrice = openingPrice,
                ClosingPrice = closingPrice
            };
        }
        /// <summary>
        /// 次のページ取得
        /// </summary>
        /// <param name="hrefNode"></param>
        /// <returns></returns>
        private async Task<IDocument> GetNextPageAsync(Task<IDocument> searchListPage)
        {
            var hrefNode = searchListPage.Result.Body.SelectSingleNode("/html/body/table[2]/tbody/tr/td[2]/table[2]/tbody/tr/td/table[2]/tbody/tr/td[2]/table/tbody/tr/td[1]/div/a");
            if (hrefNode != null)
            {
                var docmkup = parser.ParseDocument(hrefNode.ToMarkup());
                return await context.OpenAsync(BASE_URL + docmkup.QuerySelectorAll("a").First().GetAttribute("href"));
            }
            else
            {
                return null;
            }
        }
    }
}
