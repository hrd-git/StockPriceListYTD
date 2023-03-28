using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPriceListYTD.Entity
{
    internal class StockInfo
    {
        public string SearchDate { get; set; }
        public string SecuritiesCode {  get; set; }
        public string CompanyName {get; set; }
        public string IndustryCode { get; set; }
        public string MarketSegment { get; set; }
        public decimal OpeningPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }


        public StockInfo()
        {
            this.SearchDate = string.Empty;
            this.SecuritiesCode = string.Empty;
            this.CompanyName = string.Empty;
            this.IndustryCode = string.Empty;
            this.MarketSegment = string.Empty;
            this.CreatedAt = DateTime.Now;
            this.UpdateAt = DateTime.Now;
        }
    }
}
