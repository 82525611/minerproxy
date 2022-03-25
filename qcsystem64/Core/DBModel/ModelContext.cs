using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class ModelContext : DbContext
    {
        public ModelContext()
        {
            Database.EnsureCreated();
        }
        public ModelContext(DbContextOptions<ModelContext> options)
                  : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "logdb.db";

            optionsBuilder.UseSqlite("Data source=" + path);    //创建文件夹的位置        
        }

        public DbSet<ClientLogs> ClientLogs { get; set; }
        public DbSet<ServerLogs> ServerLogs { get; set; }
        public DbSet<newportset> ServerSetting { get; set; }
        public DbSet<ConsoleLog> ConsoleLog { get; set; }
    }
    public class ConsoleLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string text { get; set; }
        public DateTime createtime { get; set; }


    }
    public class ClientLogs
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int? rid { get; set; }
        public string? method { get; set; }
        public string text { get; set; }
        public DateTime createtime { get; set; }
        public string guid { get; set; }
        public string? address { get; set; }
        public string? device_name { get; set; }
        public string? res_text { get; set; }
        public int? benefits { get; set; }

    }
    public class ServerLogs
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int? rid { get; set; }
        public string text { get; set; }
        public DateTime createtime { get; set; }
        public string guid { get; set; }
        public string? pow_hash { get; set; }
        public string? address { get; set; }
        public int benefits { get; set; }
    }
    public class newportset
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string name { get; set; }
        public int serverport { get; set; }
        public string serverip { get; set; }
        public int bserverport { get; set; }
        public string bserverip { get; set; }
        public int localport { get; set; }
        public string benefits_address { get; set; }
        public decimal benefits_ratio { get; set; }

        public string? proxyip { get; set; }
        public int? proxyport { get; set; }
    }


}
