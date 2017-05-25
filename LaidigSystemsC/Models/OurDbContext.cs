using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace LaidigSystemsC.Models
{
    public class OurDbContext:DbContext
    {
        public OurDbContext() : base("name=LaidigDbConStr") { }

        public DbSet<UserAccount> useraccounts { get; set; }

       // public DbSet<CsvFile> csvfiles { get; set; }

       // public DbSet<DataLogMonitouch> datalogmonitouchs { get; set; }

       // public DbSet<DataLogAB> datalogabs { get; set; }

       // public DbSet<DelayedUpload> DelayedUploads { get; set; }
      //  public DbSet<FileDetail> FileDetails { get; set; }

      //  public DbSet<Machine> Machines { get; set; }

      //  public DbSet<General> Generals { get; set; }

      //  public DbSet<Temperature> Temperatures { get; set; }

       // public DbSet<Flow> Flows { get; set; }

      //  public DbSet<Pressure> Pressures { get; set; }


    }
} 