using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StockPriceListYTD.Entity;
using StockPriceListYTD.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace StockPriceListYTDDTest
{
    [TestClass]
    [DeploymentItem("StockPriceListYTDDTest.dll.config")]
    public class UnitTest1
    {
        private static IContainer _container { get; set; }

        [TestInitialize]
        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StockPriceListYTDRepository>().SingleInstance();
            _container = builder.Build();
        }

        [TestMethod]
        public virtual void GetIndustries()
        {
            var mock = new Mock<StockPriceListYTDRepository>();
            List<Industry> expectedIndustries  = new List<Industry>();
            for (int i = 0; i < 33; i++) expectedIndustries.Add(new Industry());

            var industries = _container.Resolve<StockPriceListYTDRepository>().GetIndustries();

            Assert.AreEqual(expectedIndustries.Count(), industries.Count());
        }
    }
}