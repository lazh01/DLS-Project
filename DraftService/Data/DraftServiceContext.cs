using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DraftService.Models;

namespace DraftService.Data
{
    public class DraftServiceContext : DbContext
    {
        public DraftServiceContext (DbContextOptions<DraftServiceContext> options)
            : base(options)
        {
        }

        public DbSet<DraftService.Models.Draft> Draft { get; set; } = default!;
    }
}
