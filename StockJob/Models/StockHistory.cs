using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace StockJob.Models
{
    class StockHistory
    {
        [Key]
        public int StockHistoryID { get; set; }
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
        /// <summary>日期</summary>
        public DateTime Date { get; set; }
        /// <summary>成交股數</summary>
        public uint TradeVolume { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>成交金額</summary>
        public decimal TurnOverInValue { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>開盤價</summary>
        public decimal OpeningPrice { get; set; }
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
        /// <summary>收盤價</summary>
        public decimal ClosingPrice { get; set; }
        /// <summary>漲跌價差</summary>
        public string DailyPricing { get; set; }
        /// <summary>成交筆數</summary>
        public uint NumberOfDeals { get; set; }
    }
}
