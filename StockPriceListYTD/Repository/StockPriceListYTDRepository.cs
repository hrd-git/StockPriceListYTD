using Dapper;
using StockPriceListYTD.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("StockPriceListYTDDTest")]

namespace StockPriceListYTD.Repository
{
    internal class StockPriceListYTDRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["StockPriceListYTDConnectionString"].ConnectionString;

        public IEnumerable<Industry> GetIndustries()
        {
            IEnumerable<Industry> industries;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM INDUSTRY with(nolock)";
                industries = connection.Query<Industry>(sql);
            }

            return industries;
        }

        public void InsertStockInfo(List<StockInfo> stockInfoList)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "INSERT INTO STOCK_INFO VALUES (@SearchDate, @SecuritiesCode, @IndustryCode, @CompanyName, @MarketSegment, @OpeningPrice, @ClosingPrice, @CreatedAt, @UpdateAt)";
                var ret = connection.Execute(sql, stockInfoList);
            }
        }
    }
}
