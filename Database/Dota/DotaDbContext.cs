using Microsoft.EntityFrameworkCore;
using RD2LPowerRankings.Database.Dota.Models;

namespace RD2LPowerRankings.Database.Dota;

public class DotaDbContext : DbContext
{
    public DotaDbContext(DbContextOptions<DotaDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Match> Matches { get; set; } = null!;
    public virtual DbSet<Player> Players { get; set; } = null!;

    public virtual DbSet<Team> Teams { get; set; } = null!;
    public virtual DbSet<PlayerMatch> PlayerMatches { get; set; } = null!;
    public virtual DbSet<PlayerMatchAbility> PlayerMatchAbilities { get; set; } = null!;
    public virtual DbSet<PlayerMatchItemUse> PlayerMatchItemUses { get; set; } = null!;
    public virtual DbSet<PlayerWord> PlayerWords { get; set; } = null!;
    public virtual DbSet<UnParsedMatch> UnParsedMatches { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasIndex(e => e.GameMode, "IX_Matches_GameMode");

            entity.HasIndex(e => e.LeagueId, "IX_Matches_LeagueId");

            entity.HasIndex(e => e.LobbyType, "IX_Matches_LobbyType");

            entity.HasIndex(e => e.Rank, "IX_Matches_Rank");

            entity.Property(e => e.MatchId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasIndex(e => e.MmrEstimate, "IX_Players_MmrEstimate");

            entity.HasIndex(e => e.RankTier, "IX_Players_RankTier");

            entity.Property(e => e.PlayerId).ValueGeneratedNever();
        });

        modelBuilder.Entity<PlayerMatch>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.MatchId });

            entity.HasIndex(e => e.HeroId, "IX_PlayerMatches_HeroId");

            entity.HasIndex(e => e.MatchId, "IX_PlayerMatches_MatchId");

            entity.HasIndex(e => e.PlayerId, "IX_PlayerMatches_PlayerId");

            entity.HasIndex(e => e.StartTime, "IX_PlayerMatches_StartTime");

            entity.HasOne(d => d.Match)
                .WithMany(p => p.PlayerMatches)
                .HasForeignKey(d => d.MatchId);
        });

        modelBuilder.Entity<Team>(entity => { entity.HasKey(e => new { e.TeamCaptainId, e.SeasonName }); });


        modelBuilder.Entity<PlayerMatchAbility>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.MatchId, e.AbilityId });

            entity.HasIndex(e => e.MatchId, "IX_PlayerMatchAbilities_MatchId");

            entity.HasIndex(e => e.PlayerId, "IX_PlayerMatchAbilities_PlayerId");

            entity.HasOne(d => d.Match)
                .WithMany(p => p.PlayerMatchAbilities)
                .HasForeignKey(d => d.MatchId);
        });

        modelBuilder.Entity<PlayerMatchItemUse>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.MatchId, e.ItemId });

            entity.HasIndex(e => e.MatchId, "IX_PlayerMatchItemUses_MatchId");

            entity.HasIndex(e => e.PlayerId, "IX_PlayerMatchItemUses_PlayerId");

            entity.HasOne(d => d.Match)
                .WithMany(p => p.PlayerMatchItemUses)
                .HasForeignKey(d => d.MatchId);
        });

        modelBuilder.Entity<PlayerWord>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.Word });

            entity.HasIndex(e => e.PlayerId, "IX_PlayerWords_PlayerId");

            entity.HasIndex(e => e.Word, "IX_PlayerWords_Word");

            entity.HasOne(d => d.Player)
                .WithMany(p => p.PlayerWords)
                .HasForeignKey(d => d.PlayerId);
        });

        modelBuilder.Entity<UnParsedMatch>(entity =>
        {
            entity.HasKey(e => e.MatchId);

            entity.Property(e => e.MatchId).ValueGeneratedNever();
        });
    }
}