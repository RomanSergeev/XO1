using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

// ctrl cv
namespace XO1.Models
{
    public class GameContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
    }
}