using StockLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalAssistant.Models
{
    public class StockInfo
    {
        [Key]
        public int StockInfoID { get; set; }
        [StringLength(50)]
        [Required(AllowEmptyStrings = false)]
        /// <summary>代號/summary>
        public string No { get; set; }
        [StringLength(50)]
        [Required(AllowEmptyStrings = false)]
        /// <summary>類型是TSE或OTC/summary>
        public string Type { get; set; }
        [StringLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [StringLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string FullName { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>最新成交價/summary>
        public decimal LastTradedPrice { get; set; }
        /// <summary>最新一筆交易成交量/summary>
        public int LastVolume { get; set; }
        /// <summary>今日累積成交量</summary>
        public int TotalVolume { get; set; }
        /// <summary>最佳五檔賣出</summary>
        public ICollection<Top5Sell> Top5Sell { get; set; }
        /// <summary>最佳五檔買入</summary>
        public ICollection<Top5Buy> Top5Buy { get; set; }
        [DataType(DataType.DateTime)]
        /// <summary>最後Sync時間</summary>
        public DateTime SyncTime { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>最高價</summary>
        public decimal HighestPrice { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>最低價</summary>
        public decimal LowestPrice { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>開盤價</summary>
        public decimal OpeningPrice { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>昨日收盤價</summary>
        public decimal YesterdayClosingPrice { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>漲停點</summary>
        public decimal LimitUp { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>跌停點</summary>
        public decimal LimitDown { get; set; }
    }
}
