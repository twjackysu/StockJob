using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalAssistant.Models
{
    public class Top5Sell
    {
        [Key]
        public int ID { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(16,2)")]
        /// <summary>最佳五檔賣出價格</summary>
        public decimal Price { get; set; }
        /// <summary>最佳五檔賣出數量</summary>
        public int Volume { get; set; }
    }
}
