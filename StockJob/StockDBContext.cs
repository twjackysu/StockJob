using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockJob
{
    class StockDBContext : DbContext
    {
        public DbSet<StockInfo> StockInfo { get; set; }
        public DbSet<Top5Buy> Top5Buy { get; set; }
        public DbSet<Top5Sell> Top5Sell { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer("");
    }
}
