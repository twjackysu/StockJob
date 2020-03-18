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
        private readonly StockInfoBuilder stockInfoBuilder;
        public StockRunner(ILogger<StockRunner> logger)
        {
            this.logger = logger;
            stockInfoBuilder = new StockInfoBuilder();
        }
        public async Task SaveStockInfoToDataBase(StockDBContext dbContext)
        {
            TSEOTCList test = new TSEOTCList();
            var TSEList = test.GetTSEList();
            var OTCList = test.GetOTCList();
            //API限制，因此一次只能取50筆
            var tseTotalLoop = (TSEList.Count / 50) + 1;
            var otcTotalLoop = (OTCList.Count / 50) + 1;
            var taskCount = 0;
            var tseTaskQuerys = new Task<(StockType, string)[]>[tseTotalLoop];
            var tseTasks = new Task<List<StockInfo>>[tseTotalLoop];
            while (taskCount < tseTotalLoop)
            {
                var take50List = TSEList.Skip(taskCount * 50);
                if (taskCount != tseTotalLoop - 1)
                {
                    take50List = take50List.Take(50);
                }
                var queries = take50List.Select(
                    x => OTCList.Any(y => y == x) ? (StockType.OTC, x) : (StockType.TSE, x)
                ).ToArray();
                var result = stockInfoBuilder.GetStocksInfo(false, queries);
                tseTasks[taskCount] = result;
                taskCount++;
            }
            taskCount = 0;
            var tseResult = await Task.WhenAll(tseTasks);
            var otcTasks = new Task<List<StockInfo>>[otcTotalLoop];
            while (taskCount < otcTotalLoop)
            {
                var take50List = OTCList.Skip(taskCount * 50);
                if (taskCount != otcTotalLoop - 1)
                {
                    take50List = take50List.Take(50);
                }
                var queries = take50List.Select(
                    x => OTCList.Any(y => y == x) ? (StockType.OTC, x) : (StockType.TSE, x)
                ).ToArray();
                var result = stockInfoBuilder.GetStocksInfo(false, queries);
                otcTasks[taskCount] = result;
                taskCount++;
            }
            var otcResult = await Task.WhenAll(otcTasks);

            var isSyncTimeExist = false;
            var isfirstLoop = true;

            foreach (var each50Stock in tseResult)
            {
                if (isSyncTimeExist)
                {
                    break;
                }
                if (each50Stock != null)
                {
                    foreach (var stockInfo in each50Stock)
                    {
                        if (isfirstLoop)
                        {
                            isSyncTimeExist = dbContext.StockInfo.Any(x => x.SyncTime == stockInfo.SyncTime);
                            isfirstLoop = false;
                        }
                        if (isSyncTimeExist)
                        {
                            break;
                        }
                        dbContext.Add(ConvertDBStockInfo(stockInfo));
                    }
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException e)
                    {
                        logger.LogInformation($"Sync Fail.");
                        logger.LogError(e.ToString());
                    }
                }
            }
            foreach (var each50Stock in otcResult)
            {
                if (isSyncTimeExist)
                {
                    break;
                }
                if (each50Stock != null)
                {
                    foreach (var stockInfo in each50Stock)
                    {
                        if (isfirstLoop)
                        {
                            isSyncTimeExist = dbContext.StockInfo.Any(x => x.SyncTime == stockInfo.SyncTime);
                            isfirstLoop = false;
                        }
                        if (isSyncTimeExist)
                        {
                            break;
                        }
                        dbContext.Add(ConvertDBStockInfo(stockInfo));
                    }
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException e)
                    {
                        logger.LogInformation($"Sync Fail.");
                        logger.LogError(e.ToString());
                    }
                }
            }
        }
        private Models.StockInfo ConvertDBStockInfo(StockInfo stockInfo)
        {
            var top5Sell = new List<Models.Top5Sell>();
            var top5Buy = new List<Models.Top5Buy>();
            for (int i = 0; i < stockInfo.Top5SellPrice.Length; i++)
            {
                top5Sell.Add(new Models.Top5Sell()
                {
                    Price = (decimal)stockInfo.Top5SellPrice[i],
                    Volume = (int)stockInfo.Top5SellVolume[i]
                });
            }
            for (int i = 0; i < stockInfo.Top5BuyPrice.Length; i++)
            {
                top5Buy.Add(new Models.Top5Buy()
                {
                    Price = (decimal)stockInfo.Top5BuyPrice[i],
                    Volume = (int)stockInfo.Top5BuyVolume[i]
                });
            }
            return new Models.StockInfo()
            {
                No = stockInfo.No,
                Type = stockInfo.Type.ToString(),
                Name = stockInfo.Name,
                FullName = stockInfo.FullName,
                LastTradedPrice = (decimal)stockInfo.LastTradedPrice,
                LastVolume = (int)stockInfo.LastVolume,
                TotalVolume = (int)stockInfo.TotalVolume,
                Top5Sell = top5Sell,
                Top5Buy = top5Buy,
                SyncTime = stockInfo.SyncTime,
                HighestPrice = (decimal)stockInfo.HighestPrice,
                LowestPrice = (decimal)stockInfo.LowestPrice,
                OpeningPrice = (decimal)stockInfo.OpeningPrice,
                YesterdayClosingPrice = (decimal)stockInfo.YesterdayClosingPrice,
                LimitUp = (decimal)stockInfo.LimitUp,
                LimitDown = (decimal)stockInfo.LimitDown
            };
        }
    }
}
