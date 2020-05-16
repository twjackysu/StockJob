using Microsoft.EntityFrameworkCore;
using StockJob.Models;

namespace StockJob
{
    class StockDBContext : DbContext
    {
        public DbSet<StockHistory> StockHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer("");
    }
}
