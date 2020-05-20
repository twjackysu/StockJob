using Microsoft.Extensions.Logging;
using StockLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StockJob
{
    class StockRunner
    {
        private readonly ILogger<StockRunner> logger;
        private readonly IHistoryBuilder historyBuilder;
        private readonly ITSEOTCListBuilder tseOTCListBuilder;
        private readonly IStockInfoBuilder stockInfoBuilder;
        private readonly StockDBContext dbContext;
        private const int nextMonthDelayMin = 4000;
        private const int nextMonthDelayMax = 12000;
        private const int IPLockDelayMin = 1800000; //half hour
        private const int IPLockDelayMax = 3600000; //one hour
        public StockRunner(ILogger<StockRunner> logger, IHistoryBuilder historyBuilder, ITSEOTCListBuilder tseOTCListBuilder, IStockInfoBuilder stockInfoBuilder, StockDBContext dbContext)
        {
            this.logger = logger;
            this.historyBuilder = historyBuilder;
            this.tseOTCListBuilder = tseOTCListBuilder;
            this.stockInfoBuilder = stockInfoBuilder;
            this.dbContext = dbContext;
        }
        /// <summary>
        /// 一次性爬所有股票的資料，爬股價爬到現在這個月
        /// </summary>
        /// <param name="from">輸入從哪年哪月開始爬</param>
        /// <param name="skipByMonth">若當月資料庫內已有資料，是否跳過(如果月中爬過的話，下次要爬那個月必須為false，否則會跳過那個月)</param>
        public async Task OneTimeCrawler(DateTime from, bool skipByMonth = false)
        {
            var utcNow = DateTime.UtcNow;
            var twNow = utcNow.AddHours(8);
            await OneTimeCrawler(from, twNow, skipByMonth);
        }

        /// <summary>
        /// 一次性爬所有股票的資料，爬股價爬到你指定的那個月
        /// </summary>
        /// <param name="from">從哪年哪月開始爬</param>
        /// <param name="to">爬到哪年哪月結束</param>
        /// <param name="skipByMonth">若當月資料庫內已有資料，是否跳過(如果月中爬過的話，下次要爬那個月必須為false，否則會跳過那個月)</param>
        public async Task OneTimeCrawler(DateTime from, DateTime to, bool skipByMonth = false)
        {
            var TSEList = tseOTCListBuilder.GetTSEList();
            var OTCList = tseOTCListBuilder.GetOTCList();

            foreach (var tse in TSEList)
            {
                await OneTimeCrawler(TSEList, tse, StockType.TSE, from, to, skipByMonth);
            }

            foreach (var otc in OTCList)
            {
                await OneTimeCrawler(OTCList, otc, StockType.OTC, from, to, skipByMonth);
            }
        }
        /// <summary>
        /// 一次性爬單支股票的資料，爬股價爬到現在這個月
        /// </summary>
        /// <param name="stockNo">股票編號</param>
        /// <param name="stockType">股票類型</param>
        /// <param name="from">輸入從哪年哪月開始爬</param>
        /// <param name="skipByMonth">若當月資料庫內已有資料，是否跳過(如果月中爬過的話，下次要爬那個月必須為false，否則會跳過那個月)</param>
        public async Task OneTimeCrawler(string stockNo, StockType stockType, DateTime from, bool skipByMonth = false)
        {
            var utcNow = DateTime.UtcNow;
            var twNow = utcNow.AddHours(8);
            await OneTimeCrawler(stockNo, stockType, from, twNow, skipByMonth);
        }
        /// <summary>
        /// 一次性爬單支股票的資料，爬股價爬到你指定的那個月
        /// </summary>
        /// <param name="stockNo">股票編號</param>
        /// <param name="stockType">股票類型</param>
        /// <param name="from">從哪年哪月開始爬</param>
        /// <param name="to">爬到哪年哪月結束</param>
        /// <param name="skipByMonth">若當月資料庫內已有資料，是否跳過(如果月中爬過的話，下次要爬那個月必須為false，否則會跳過那個月)</param>
        public async Task OneTimeCrawler(string stockNo, StockType stockType, DateTime from, DateTime to, bool skipByMonth = false)
        {
            switch (stockType)
            {
                case StockType.TSE:
                    var TSEList = tseOTCListBuilder.GetTSEList();
                    await OneTimeCrawler(TSEList, stockNo, StockType.TSE, from, to, skipByMonth);
                    break;
                case StockType.OTC:
                    var OTCList = tseOTCListBuilder.GetOTCList();
                    await OneTimeCrawler(OTCList, stockNo, StockType.OTC, from, to, skipByMonth);
                    break;
            }
        }
        private async Task OneTimeCrawler(HashSet<string> nowStockList, string stockNo, StockType stockType, DateTime from, DateTime to, bool skipByMonth = false)
        {
            if (!nowStockList.Contains(stockNo))
            {
                logger.LogInformation($"The current StockType: {stockType} doesn't have this stock {stockNo}");
                return;
            }
            var random  = new Random();
            //這邊全部同步去爬，非同步爬小心被鎖IP
            StockInfo info = null;
            from = new DateTime(from.Year, from.Month, 1);
            var currentMonth = new DateTime(to.Year, to.Month, 1);

            while (currentMonth >= from)
            {
                try
                {
                    var currentMonthEnd = new DateTime(currentMonth.Year, currentMonth.Month, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    var dateHashSet = dbContext.StockHistory.Where(x => x.No == stockNo && x.Date >= currentMonth && x.Date <= currentMonthEnd).Select(x => x.Date).ToHashSet();

                    if (skipByMonth)
                    {
                        if (dateHashSet.Count > 0)
                        {
                            logger.LogInformation($"{currentMonth:yyyyMM} {stockNo} data already exists.");
                            currentMonth = currentMonth.AddMonths(-1);
                            continue;
                        }
                    }
                    while (info == null)
                    {
                        info = (await stockInfoBuilder.GetStocksInfo((stockType, stockNo))).SingleOrDefault();
                        if (info == null)
                        {
                            var delayMs2 = random.Next(IPLockDelayMin, IPLockDelayMax);
                            logger.LogInformation($"Get StockInfo Fail. Retry in {delayMs2} ms");
                            await Task.Delay(delayMs2);
                        }
                    }
                    var delayMs = random.Next(nextMonthDelayMin, nextMonthDelayMax);
                    var histories = await historyBuilder.GetStockHistories(stockNo, currentMonth, stockType);
                    if (histories == null || histories.Length == 0)
                    {
                        currentMonth = currentMonth.AddMonths(-1);
                        await Task.Delay(delayMs);
                        logger.LogWarning($"{currentMonth:yyyyMM} {info.No} No Data. The next one start after {delayMs} ms");
                        continue;
                    }
                    foreach (var history in histories)
                    {
                        if (!dateHashSet.Contains(history.Date))
                        {
                            dbContext.Add(ConvertDBStockHistory(history, info.No, info.Type.ToString(), info.Name, info.FullName));
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation($"{currentMonth:yyyyMM} {info.No} Success. The next one start after {delayMs} ms");
                    currentMonth = currentMonth.AddMonths(-1);
                    await Task.Delay(delayMs);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error when CurrentMonth = {currentMonth:yyyyMM} {info.No} {info.Name}");
                    if (e is HttpRequestException)
                    {
                        //IP被鎖
                        var delayMs = random.Next(IPLockDelayMin, IPLockDelayMax);
                        logger.LogInformation($"Your IP has been blocked and will be restarted after {delayMs} ms delay.");
                        await Task.Delay(delayMs);
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
