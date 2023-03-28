using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPriceListYTD.Entity
{
    internal class Industry
    {
        public string IndustryCode { get; set; }
        public string JapaneseName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public Industry()
        {
            this.IndustryCode =  string.Empty;
            this.JapaneseName = string.Empty;
        }
    }
}
