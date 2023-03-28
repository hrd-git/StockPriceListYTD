using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using Dapper;
using StockPriceListYTD.Entity;
using StockPriceListYTD.Repository;
using StockPriceListYTD.Service;

namespace StockPriceListYTD
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int status = 0;
            try
            {
                ScrapingStockService scraping = new ScrapingStockService();
                scraping.Exec();

            }
            catch (Exception ex)
            {
                status = 1;
                Console.WriteLine(ex.StackTrace);
            }

            return status;
        }

        
    }
}