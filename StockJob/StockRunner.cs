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
        private const int nextMonthDelayMax = 5000;
        private const int InfoNullDelayMax = 60000;
        public StockRunner(ILogger<StockRunner> logger, IHistoryBuilder historyBuilder, ITSEOTCListBuilder tseOTCListBuilder, IStockInfoBuilder stockInfoBuilder)
        {
            this.logger = logger;
            this.historyBuilder = historyBuilder;
            this.tseOTCListBuilder = tseOTCListBuilder;
            this.stockInfoBuilder = stockInfoBuilder;
        }
        /// <summary>
        /// 一次性的Job，爬股價爬到現在這個月
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="from">輸入從哪年哪月開始爬</param>
        /// <returns></returns>
        public async Task OneTimeCrawler(StockDBContext dbContext, DateTime from)
        {
            var TSEList = tseOTCListBuilder.GetTSEList();
            var OTCList = tseOTCListBuilder.GetOTCList();

            var random = new Random();
            foreach (var tse in TSEList)
            {
                //這邊全部同步去爬，非同步爬小心被鎖IP
                StockInfo info = null;
                while(info == null)
                {
                    info = (await stockInfoBuilder.GetStocksInfo((StockType.TSE, tse))).SingleOrDefault();
                    if(info == null)
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
                        var histories = await historyBuilder.GetStockHistories(tse, currentMonth, StockType.TSE);
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
                        var delayMs = random.Next(nextMonthDelayMax);
                        logger.LogInformation($"{currentMonth:yyyyMM} {info.No} Success. The next one start after {delayMs} ms");
                        currentMonth = currentMonth.AddMonths(-1);
                        await Task.Delay(delayMs);
                    }
                    catch(Exception e)
                    {
                        logger.LogError(e, $"Error when CurrentMonth = {currentMonth:yyyyMM} {info.No} {info.Name}");
                    }
                }
            }

            foreach (var otc in OTCList)
            {
                StockInfo info = null;
                while (info == null)
                {
                    info = (await stockInfoBuilder.GetStocksInfo((StockType.OTC, otc))).SingleOrDefault();
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
                        var histories = await historyBuilder.GetStockHistories(otc, currentMonth, StockType.OTC);
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
                        var delayMs = random.Next(nextMonthDelayMax);
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
