using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockJob
{
    class StockRunner
    {
        private readonly ILogger<StockRunner> logger;
        private readonly IHistoryBuilder historyBuilder;
        private readonly ITSEOTCListBuilder tseOTCListBuilder;
        private readonly IStockInfoBuilder stockInfoBuilder;
        private const int nextMonthDelayMin = 4000;
        private const int nextMonthDelayMax = 12000;
        private const int InfoNullDelayMax = 60000;
        public StockRunner(ILogger<StockRunner> logger, IHistoryBuilder historyBuilder, ITSEOTCListBuilder tseOTCListBuilder, IStockInfoBuilder stockInfoBuilder)
        {
            this.logger = logger;
            this.historyBuilder = historyBuilder;
            this.tseOTCListBuilder = tseOTCListBuilder;
            this.stockInfoBuilder = stockInfoBuilder;
        }
        /// <summary>
        /// 一次性爬所有股票的Job，爬股價爬到現在這個月
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="from">輸入從哪年哪月開始爬</param>
        /// <returns></returns>
        public async Task OneTimeCrawler(StockDBContext dbContext, DateTime from)
        {
            var TSEList = tseOTCListBuilder.GetTSEList();
            var OTCList = tseOTCListBuilder.GetOTCList();

            foreach (var tse in TSEList)
            {
                await OneTimeCrawler(TSEList, tse, StockType.TSE, dbContext, from);
            }

            foreach (var otc in OTCList)
            {
                await OneTimeCrawler(OTCList, otc, StockType.OTC, dbContext, from);
            }
        }
        /// <summary>
        /// 一次性爬單支股票的Job，爬股價爬到現在這個月
        /// </summary>
        /// <param name="stockNo">股票編號</param>
        /// <param name="stockType">股票類型</param>
        /// <param name="dbContext"></param>
        /// <param name="from">輸入從哪年哪月開始爬</param>
        /// <returns></returns>
        public async Task OneTimeCrawler(string stockNo, StockType stockType, StockDBContext dbContext, DateTime from)
        {
            switch (stockType)
            {
                case StockType.TSE:
                    var TSEList = tseOTCListBuilder.GetTSEList();
                    await OneTimeCrawler(TSEList, stockNo, StockType.TSE, dbContext, from);
                    break;
                case StockType.OTC:
                    var OTCList = tseOTCListBuilder.GetOTCList();
                    await OneTimeCrawler(OTCList, stockNo, StockType.OTC, dbContext, from);
                    break;
            }
        }
        private async Task OneTimeCrawler(HashSet<string> nowStockList, string stockNo, StockType stockType, StockDBContext dbContext, DateTime from)
        {
            if (!nowStockList.Contains(stockNo))
            {
                logger.LogInformation($"The current StockType: {stockType} doesn't have this stock {stockNo}");
                return;
            }
            var random  = new Random();
            //這邊全部同步去爬，非同步爬小心被鎖IP
            StockInfo info = null;
            while (info == null)
            {
                info = (await stockInfoBuilder.GetStocksInfo((stockType, stockNo))).SingleOrDefault();
                if (info == null)
                {
                    var delayMs = random.Next(InfoNullDelayMax);
                    logger.LogInformation($"Get StockInfo Fail. Retry in {delayMs} ms");
                    await Task.Delay(delayMs);
                }
            }
            var utcNow = DateTime.UtcNow;
            var twNow = utcNow.AddHours(8);
            var currentMonth = twNow;
            while (currentMonth > from)
            {
                try
                {
                    var histories = await historyBuilder.GetStockHistories(stockNo, currentMonth, stockType);
                    if (histories == null || histories.Length == 0)
                    {
                        break;
                    }
                    //需要一些額外的方式偵測當IP被鎖時的情況

                    foreach (var history in histories)
                    {
                        dbContext.Add(ConvertDBStockHistory(history, info.No, info.Type.ToString(), info.Name, info.FullName));
                    }
                    await dbContext.SaveChangesAsync();
                    var delayMs = random.Next(nextMonthDelayMin, nextMonthDelayMax);
                    logger.LogInformation($"{currentMonth:yyyyMM} {info.No} Success. The next one start after {delayMs} ms");
                    currentMonth = currentMonth.AddMonths(-1);
                    await Task.Delay(delayMs);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error when CurrentMonth = {currentMonth:yyyyMM} {info.No} {info.Name}");
                }
            }
        }
        private Models.StockHistory ConvertDBStockHistory(StockHistory stockHistory, string no, string type, string name, string fullName)
        {
            return new Models.StockHistory()
            {
                No = no,
                Type = type,
                Name = name,
                FullName = fullName,
                ClosingPrice = stockHistory.ClosingPrice,
                DailyPricing = stockHistory.DailyPricing,
                Date = stockHistory.Date,
                NumberOfDeals = stockHistory.NumberOfDeals,
                TradeVolume = stockHistory.TradeVolume,
                HighestPrice = stockHistory.HighestPrice,
                LowestPrice = stockHistory.LowestPrice,
                OpeningPrice = stockHistory.OpeningPrice,
                TurnOverInValue = stockHistory.TurnOverInValue,
            };
        }
    }
}
