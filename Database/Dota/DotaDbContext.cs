using Microsoft.EntityFrameworkCore;
using RD2LPowerRankings.Database.Dota.Models;

namespace RD2LPowerRankings.Database.Dota;

public class DotaDbContext : DbContext
{
    public DotaDbContext(DbContextOptions<DotaDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Match> Matches { get; set; } = default!;

    public virtual DbSet<UnParsedMatch> UnParsedMatches { get; set; } = default!;
    public virtual DbSet<Player> Players { get; set; } = default!;
    public virtual DbSet<PlayerMatch> PlayerMatches { get; set; } = default!;
    public virtual DbSet<PlayerMatchAbility> PlayerMatchAbilities { get; set; } = default!;
    public virtual DbSet<PlayerMatchAction> PlayerMatchActions { get; set; } = default!;
    public virtual DbSet<PlayerMatchItemFirstPurchase> PlayerMatchItemFirstPurchases { get; set; } = default!;
    public virtual DbSet<PlayerMatchItemUse> PlayerMatchItemUses { get; set; } = default!;
    public virtual DbSet<PlayerWord> PlayerWords { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerMatch>()
            .HasKey(
                nameof(PlayerMatch.PlayerId), nameof(PlayerMatch.MatchId)
            );

        modelBuilder.Entity<PlayerMatchAbility>()
            .HasKey(
                nameof(PlayerMatchAbility.PlayerId), nameof(PlayerMatchAbility.MatchId),
                nameof(PlayerMatchAbility.Ability)
            );

        modelBuilder.Entity<PlayerMatchAction>()
            .HasKey(
                nameof(PlayerMatchAction.PlayerId), nameof(PlayerMatchAction.MatchId),
                nameof(PlayerMatchAction.Action)
            );

        modelBuilder.Entity<PlayerMatchItemFirstPurchase>()
            .HasKey(
                nameof(PlayerMatchItemFirstPurchase.PlayerId), nameof(PlayerMatchItemFirstPurchase.MatchId),
                nameof(PlayerMatchItemFirstPurchase.Item)
            );

        modelBuilder.Entity<PlayerMatchItemUse>()
            .HasKey(
                nameof(PlayerMatchItemUse.PlayerId), nameof(PlayerMatchItemUse.MatchId),
                nameof(PlayerMatchItemUse.Item)
            );

        modelBuilder.Entity<PlayerWord>()
            .HasKey(
                nameof(PlayerWord.PlayerId), nameof(PlayerWord.Word)
            );
    }
}