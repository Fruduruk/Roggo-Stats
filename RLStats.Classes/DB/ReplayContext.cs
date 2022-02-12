using Microsoft.EntityFrameworkCore;

using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;

using System;
using System.IO;

namespace RLStats_Classes.DB
{
    public class ReplayContext : DbContext
    {
        private const string ConnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=rl_stats_db;";

        public DbSet<Replay> Replays { get; set; }
        public DbSet<AdvancedReplay> AdvancedReplays { get; set; }

        public string DbPath { get; }

        public ReplayContext() : base()
        {
            var folder = Environment.SpecialFolder.MyDocuments;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "replays.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var version = new MariaDbServerVersion(new Version("1.0.0.0"));
            options.UseMySql(ConnectionString, version);
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Replay>().HasKey(replay => replay.Id);
            model.Entity<Team>().HasKey(team => team.Id);
            model.Entity<Player>().HasKey(player => player.Name);
            model.Entity<AdvancedPlayer>().HasKey(advancedPlayer => advancedPlayer.CustomId);
            model.Entity<AdvancedTeam>().HasKey(team => team.Id);
            model.Entity<Ball>().HasKey(ball => ball.Id);
            model.Entity<Boost>().HasKey(boost => boost.Id);
            model.Entity<Camera>().HasKey(cam => cam.Id);
            model.Entity<Id>().HasKey(id => id.CustomId);
        }
    }
}
