using Microsoft.EntityFrameworkCore;
using NgTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NgTemplate.Data
{
    public class JobDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public DbSet<Requirement> Requirements { get; set; }

        public DbSet<Benefit> Benefits { get; set; }

        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
