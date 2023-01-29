using RD2LPowerRankings.Helpers;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Services.DotaRanking;

public class OpenAIPlayerSentenceBuilders
{
    public static List<string> ReviewPrefixWords = new()
        { "funny", "snarky", "serious", "thought-provoking", "witty", "exciting", "brutally honest", "hilarious" };

    public static List<string> RankWords = new() { "rank", "bracket", "tier", "level" };

    public static List<string> ToxicityTier1Words = new() { "extremely toxic", "very toxic", "exceptionally toxic" };

    public static List<string> ToxicityTier2Words = new() { "somewhat toxic", "slightly toxic", "a little bit toxic" };

    public static List<string> WholesomeWords = new()
        { "wholesome player", "friendly player", "nice dude" };

    public static List<string> PlayerWordUsage = new() { "one", "two" };

    public static List<string> TeamWordUsage = new() { "three", "four", "five" };

    public static List<string> SuffixToxicBadWords = new() { "mother fucker", "shit", "hell", "damn", "fuck" };

    public static List<string> SuffixWholesomeWords = new() { "badass", "real one", "chad", "based" };

    public static List<string> GoodHeroWords = new()
    {
        "amazing at", "solid at", "skilled at", "exceptional at", "amazing with", "skillful with", "plays a good"
    };

    public static List<string> TeamCaptainWords = new()
    {
        "team captain", "team leader", "captain of team", "team owner", "the captain"
    };

    public static List<string> GenerateTeamRankWords(int rank, IEnumerable<int?> teamRanks)
    {
        var badgeName = string.Concat(Enum.GetName(typeof(DotaEnums.Badge), rank).Where(char.IsLetter));

        if ((badgeName == "Immortal" || badgeName == "Divine") && teamRanks.Count(x => x >= rank) == 1)
        {
            return new List<string>
            {
                "best player on team", "most skillful on team", "the GOAT of the team", "the heavy lifter", "team MVP",
                "top performer"
            };
        }

        if (badgeName == "Immortal")
        {
            return GeneratePlayerRankWords(rank);
        }

        if (badgeName == "Divine")
        {
            return GeneratePlayerRankWords(rank);
        }

        if (badgeName == "Ancient")
        {
            return GeneratePlayerRankWords(rank);
        }

        if (badgeName == "Legend")
        {
            return GeneratePlayerRankWords(rank);
        }

        if (badgeName == "Archon")
        {
            return GeneratePlayerRankWords(rank);
        }

        if (badgeName == "Crusader")
        {
            return GeneratePlayerRankWords(rank);
        }

        return GeneratePlayerRankWords(rank);
    }

    public static List<string> GeneratePlayerRankWords(int rank)
    {
        var badgeName = string.Concat(Enum.GetName(typeof(DotaEnums.Badge), rank).Where(char.IsLetter));

        if (badgeName == "Immortal")
        {
            return new List<string>
            {
                "top ranked", "immortal rank", "amazing game sense", "borderline pro player", "great map awareness",
                "makes almost no mistakes"
            };
        }

        if (badgeName == "Divine")
        {
            return new List<string>
            {
                "divine rank", "divine tier", "good game sense", "solid mechanical skills", "solid map awareness",
                "very good player"
            };
        }

        if (badgeName == "Ancient")
        {
            return new List<string> { "ancient tier", "ancient rank", "decent skill", "good player" };
        }

        if (badgeName == "Legend")
        {
            return new List<string> { "average player", "middle of the pack" };
        }

        if (badgeName == "Archon")
        {
            return new List<string> { "mediocre rank", "makes mistakes", "needs to improve", "newb" };
        }

        if (badgeName == "Crusader")
        {
            return new List<string>
                { "low rank", "makes a lot of mistakes", "no map awareness", "terrible game sense" };
        }


        return new List<string> { "bad game sense", "makes many mistakes", "terrible player" };
    }

    public static List<string> GetHeroDescriptionWords(DotaEnums.Hero hero)
    {
        if (hero == DotaEnums.Hero.AntiMage)
        {
            return new List<string> { "mana-burns people", "afk jungles" };
        }

        if (hero == DotaEnums.Hero.Axe)
        {
            return new List<string> { "enjoys cutting the wave", "solid blink calls" };
        }

        if (hero == DotaEnums.Hero.Bane)
        {
            return new List<string> { "gives people nightmares", "fiends grips people" };
        }

        if (hero == DotaEnums.Hero.Bloodseeker)
        {
            return new List<string> { "uses annoying rupture", "loves cookies" };
        }

        if (hero == DotaEnums.Hero.Crystal_Maiden)
        {
            return new List<string> { "very cool player", "hates her sister", "loves wolves" };
        }

        if (hero == DotaEnums.Hero.Drow_Ranger)
        {
            return new List<string> { "afk jungles", "uses gust" };
        }

        if (hero == DotaEnums.Hero.Earthshaker)
        {
            return new List<string> { "great at 5-man echos", "loves basketball" };
        }

        if (hero == DotaEnums.Hero.Juggernaut)
        {
            return new List<string> { "loves to spin to win", "doesn't let healing ward die" };
        }

        if (hero == DotaEnums.Hero.Mirana)
        {
            return new List<string> { "arrows creeps", "you will need detection against this player" };
        }

        if (hero == DotaEnums.Hero.Morphling)
        {
            return new List<string> { "loves to morph into every hero", "hates ancient apparition" };
        }

        if (hero == DotaEnums.Hero.Shadow_Fiend)
        {
            return new List<string> { "will triple raze you", "eats your soul" };
        }

        if (hero == DotaEnums.Hero.Phantom_Lancer)
        {
            return new List<string> { "cancerous player", "has many illusions" };
        }

        if (hero == DotaEnums.Hero.Puck)
        {
            return new List<string> { "annoying dream coils", "slippery" };
        }

        if (hero == DotaEnums.Hero.Pudge)
        {
            return new List<string> { "has legendary hooks", "will chop you up" };
        }

        if (hero == DotaEnums.Hero.Razor)
        {
            return new List<string> { "annoying to lane against", "will zap your ass" };
        }

        if (hero == DotaEnums.Hero.Sand_King)
        {
            return new List<string> { "solid blink stuns", "good epicenter usage in teamfights" };
        }

        if (hero == DotaEnums.Hero.Storm_Spirit)
        {
            return new List<string> { "targets squishy heroes", "zip zaps you to death" };
        }

        if (hero == DotaEnums.Hero.Sven)
        {
            return new List<string> { "fast jungler", "prays for 5 man cleaves" };
        }

        if (hero == DotaEnums.Hero.Tiny)
        {
            return new List<string> { "tosses you under tower", "loves the rolling stones" };
        }

        if (hero == DotaEnums.Hero.Vengeful_Spirit)
        {
            return new List<string> { "pair them with drow", "has good swap usage" };
        }

        if (hero == DotaEnums.Hero.Windranger)
        {
            return new List<string> { "great shackle shots", "snipes you with power shots" };
        }

        if (hero == DotaEnums.Hero.Zeus)
        {
            return new List<string> { "good at dewarding", "need pipe versus this player" };
        }

        if (hero == DotaEnums.Hero.Kunkka)
        {
            return new List<string> { "loves to drink rum", "good boat usage" };
        }

        if (hero == DotaEnums.Hero.Lina)
        {
            return new List<string> { "cancerous to lane against", "will laguna blade you" };
        }

        if (hero == DotaEnums.Hero.Lion)
        {
            return new List<string> { "will finger you", "great at landing earth spikes" };
        }

        if (hero == DotaEnums.Hero.Shadow_Shaman)
        {
            return new List<string> { "objective gamer", "good at placing serpent wards" };
        }

        if (hero == DotaEnums.Hero.Slardar)
        {
            return new List<string> { "will constantly bash you" };
        }

        if (hero == DotaEnums.Hero.Tidehunter)
        {
            return new List<string> { "good 5 man ravages", "tanky player" };
        }

        if (hero == DotaEnums.Hero.Witch_Doctor)
        {
            return new List<string> { "good at solo killings cores", "annoys offlane players with maledict" };
        }

        if (hero == DotaEnums.Hero.Lich)
        {
            return new List<string> { "knows when to cast chain frost", "has good frost shield usage" };
        }

        if (hero == DotaEnums.Hero.Riki)
        {
            return new List<string> { "casts annoying smokescreens", "loves to sneak around for kills" };
        }

        if (hero == DotaEnums.Hero.Enigma)
        {
            return new List<string> { "has great 5-man blackholes", "solid micro with eidolons" };
        }

        if (hero == DotaEnums.Hero.Tinker)
        {
            return new List<string> { "cancerous player", "will make you constantly use healing salves in lane" };
        }

        if (hero == DotaEnums.Hero.Sniper)
        {
            return new List<string> { "loves defending highground", "annoying in lane", "farms with shrapnel" };
        }

        if (hero == DotaEnums.Hero.Necrophos)
        {
            return new List<string> { "will burst you down quickly", "good against tanky heroes" };
        }

        if (hero == DotaEnums.Hero.Warlock)
        {
            return new List<string> { "annoying heals in lane", "good rock usage in teamfights" };
        }

        if (hero == DotaEnums.Hero.Beastmaster)
        {
            return new List<string> { "solid boar micro", "zoo gamer", "loves to split push" };
        }

        if (hero == DotaEnums.Hero.Queen_of_Pain)
        {
            return new List<string> { "loves to gank other lanes", "good at picking people off" };
        }

        if (hero == DotaEnums.Hero.Venomancer)
        {
            return new List<string> { "places veno wards everywhere", "good ult usage in teamfights" };
        }

        if (hero == DotaEnums.Hero.Faceless_Void)
        {
            return new List<string> { "solid chronos", "good time walk usage" };
        }

        if (hero == DotaEnums.Hero.Wraith_King)
        {
            return new List<string> { "knows when to use skeletons", "farms pretty fast" };
        }

        if (hero == DotaEnums.Hero.Death_Prophet)
        {
            return new List<string> { "objective gamer", "gets team together to five-man push with ult" };
        }

        if (hero == DotaEnums.Hero.Phantom_Assassin)
        {
            return new List<string> { "will one-shot kill you with dagger", "farms deso quickly" };
        }

        if (hero == DotaEnums.Hero.Pugna)
        {
            return new List<string> { "places netherward out of sight", "annoyingly ult usage" };
        }

        if (hero == DotaEnums.Hero.Templar_Assassin)
        {
            return new List<string> { "puts traps in good places", "has good psi-blade positioning" };
        }

        if (hero == DotaEnums.Hero.Viper)
        {
            return new List<string> { "cancerous in lane", "will poison you to death" };
        }

        if (hero == DotaEnums.Hero.Luna)
        {
            return new List<string> { "lucent beams your ass", "good ult usage" };
        }

        if (hero == DotaEnums.Hero.Dragon_Knight)
        {
            return new List<string> { "loves tanky heroes", "objective gamer" };
        }

        if (hero == DotaEnums.Hero.Dazzle)
        {
            return new List<string> { "has clutch shallow grave usage", "annoying poison touch usage in lane" };
        }

        if (hero == DotaEnums.Hero.Clockwerk)
        {
            return new List<string> { "good at ganking with hookshot", "targets squishy heroes" };
        }

        if (hero == DotaEnums.Hero.Leshrac)
        {
            return new List<string> { "destroys towers quicly", "good at landing split earth" };
        }

        if (hero == DotaEnums.Hero.Natures_Prophet)
        {
            return new List<string> { "split-push gamer", "good map awareness", "solid treant micro" };
        }

        if (hero == DotaEnums.Hero.Lifestealer)
        {
            return new List<string> { "knows when to use rage", "hates tanky heroes" };
        }

        if (hero == DotaEnums.Hero.DarkSeer)
        {
            return new List<string> { "nice wall/vaccum usage", "pushes waves with ion shell" };
        }

        if (hero == DotaEnums.Hero.Clinkz)
        {
            return new List<string> { "slippery player", "farms well with barrage" };
        }

        if (hero == DotaEnums.Hero.Omniknight)
        {
            return new List<string> { "has clutch heals", "clutch guardian angel usage" };
        }

        if (hero == DotaEnums.Hero.Enchantress)
        {
            return new List<string> { "untouchable player", "good creep micro" };
        }

        if (hero == DotaEnums.Hero.Huskar)
        {
            return new List<string> { "cancerous player", "cancerous to lane against" };
        }

        if (hero == DotaEnums.Hero.Night_Stalker)
        {
            return new List<string> { "will stalk your ass", "loves to hunt supports" };
        }

        if (hero == DotaEnums.Hero.Broodmother)
        {
            return new List<string> { "mother comes for you", "cancerous player" };
        }

        if (hero == DotaEnums.Hero.Bounty_Hunter)
        {
            return new List<string> { "will track you", "loves to collect gold" };
        }

        if (hero == DotaEnums.Hero.Weaver)
        {
            return new List<string> { "slippery player", "annoying to play against" };
        }

        if (hero == DotaEnums.Hero.Jakiro)
        {
            return new List<string> { "typical rd2l slave", "good macropyre setup" };
        }

        if (hero == DotaEnums.Hero.Batrider)
        {
            return new List<string> { "annoying to lane against", "excellent lasso usage" };
        }

        if (hero == DotaEnums.Hero.Chen)
        {
            return new List<string> { "zoo keeper", "has clutch global heals" };
        }

        if (hero == DotaEnums.Hero.Spectre)
        {
            return new List<string> { "afk farms until a teamfight breaksout", "scary player late game" };
        }

        if (hero == DotaEnums.Hero.Ancient_Apparition)
        {
            return new List<string> { "the bane of all huskar players", "clutch ice blasts" };
        }

        if (hero == DotaEnums.Hero.Doom)
        {
            return new List<string> { "greedy motherfucker", "will blink on and doom you" };
        }

        if (hero == DotaEnums.Hero.Ursa)
        {
            return new List<string> { "will fury swipe you", "will blink on you and end you" };
        }

        if (hero == DotaEnums.Hero.Spirit_Breaker)
        {
            return new List<string> { "will charge your ass", "can't run away from them" };
        }

        if (hero == DotaEnums.Hero.Gyrocopter)
        {
            return new List<string> { "will barrage you to death", "annoying in lane" };
        }

        if (hero == DotaEnums.Hero.Alchemist)
        {
            return new List<string> { "very greedy", "loves to afk farm" };
        }

        if (hero == DotaEnums.Hero.Invoker)
        {
            return new List<string> { "has good sunstrikes", "has good cold snaps" };
        }

        if (hero == DotaEnums.Hero.Silencer)
        {
            return new List<string> { "plus two gamer", "casts key global silences" };
        }

        if (hero == DotaEnums.Hero.Outworld_Devourer)
        {
            return new List<string> { "good with meteor hammer", "has solid setup with astral imprisonment" };
        }

        if (hero == DotaEnums.Hero.Lycan)
        {
            return new List<string> { "will run your ass down", "good wolf micro" };
        }

        if (hero == DotaEnums.Hero.Brewmaster)
        {
            return new List<string> { "micros pandas very well", "good cyclone usage" };
        }

        if (hero == DotaEnums.Hero.Shadow_Demon)
        {
            return new List<string> { "great at stacking up poison on heroes", "has clutch disruption saves" };
        }

        if (hero == DotaEnums.Hero.Lone_Druid)
        {
            return new List<string> { "good bear micromanagement", "objective gamer", "demolishes towers" };
        }

        if (hero == DotaEnums.Hero.Chaos_Knight)
        {
            return new List<string> { "hard to lane against", "deletes squishy heroes" };
        }

        if (hero == DotaEnums.Hero.Meepo)
        {
            return new List<string> { "good poof combos", "amazing micro skills" };
        }

        if (hero == DotaEnums.Hero.Treant_Protector)
        {
            return new List<string> { "right clicks you in lane", "solid ult usage" };
        }

        if (hero == DotaEnums.Hero.Ogre_Magi)
        {
            return new List<string> { "prays to the casino gods", "blood lusts core heroes" };
        }

        if (hero == DotaEnums.Hero.Undying)
        {
            return new List<string> { "annoying in lane", "good tombstone placement" };
        }

        if (hero == DotaEnums.Hero.Rubick)
        {
            return new List<string> { "steals key spells", "clutch telekinesis usage" };
        }

        if (hero == DotaEnums.Hero.Disruptor)
        {
            return new List<string> { "will send you back to fountain", "great static storms" };
        }

        if (hero == DotaEnums.Hero.Nyx_Assassin)
        {
            return new List<string> { "lands impales every time", "sneaks around invisibly" };
        }

        if (hero == DotaEnums.Hero.Naga_Siren)
        {
            return new List<string> { "good song setup", "split-pushes with illusions well" };
        }

        if (hero == DotaEnums.Hero.Keeper_of_the_Light)
        {
            return new List<string> { "annoyingly pushes wave", "shares mana with everyone" };
        }

        if (hero == DotaEnums.Hero.Io)
        {
            return new List<string> { "clutch saves with ult", "keeps cores alive" };
        }

        if (hero == DotaEnums.Hero.Visage)
        {
            return new List<string> { "good bird micro", "bursts squishy heroes down easily" };
        }

        if (hero == DotaEnums.Hero.Slark)
        {
            return new List<string> { "evades enemies", "gets lots of stacks" };
        }

        if (hero == DotaEnums.Hero.Medusa)
        {
            return new List<string> { "knows when to cast stone gaze", "afk farms" };
        }

        if (hero == DotaEnums.Hero.Troll_Warlord)
        {
            return new List<string> { "bash lord", "takes roshan early" };
        }

        if (hero == DotaEnums.Hero.Centaur_Warrunner)
        {
            return new List<string> { "knows when to cast stampede", "hard to deal with in lane" };
        }

        if (hero == DotaEnums.Hero.Magnus)
        {
            return new List<string> { "has clutch five-man RPs", "will skewer you under tower" };
        }

        if (hero == DotaEnums.Hero.Timbersaw)
        {
            return new List<string> { "the bane of strength heroes", "annoying for enemies" };
        }

        if (hero == DotaEnums.Hero.Bristleback)
        {
            return new List<string>
                { "enemies need to bring a wand in lane", "will run you down", "will get snot all over you" };
        }

        if (hero == DotaEnums.Hero.Tusk)
        {
            return new List<string> { "has clutch snowball saves", "good at ganking" };
        }

        if (hero == DotaEnums.Hero.Skywrath_Mage)
        {
            return new List<string> { "will solo pickoff enemies", "loves to play squishy heroes" };
        }

        if (hero == DotaEnums.Hero.Abaddon)
        {
            return new List<string> { "key aphotic shield usage", "tanky boy" };
        }

        if (hero == DotaEnums.Hero.Elder_Titan)
        {
            return new List<string> { "good stomp casts", "good at using earth-splitter" };
        }

        if (hero == DotaEnums.Hero.Legion_Commander)
        {
            return new List<string> { "accumulates tons of duel damage", "always looking to duel" };
        }

        if (hero == DotaEnums.Hero.Techies)
        {
            return new List<string> { "annoying for enemies", "will burst you with their mine combo" };
        }

        if (hero == DotaEnums.Hero.Ember_Spirit)
        {
            return new List<string> { "slippery player", "hard to catch player" };
        }

        if (hero == DotaEnums.Hero.Earth_Spirit)
        {
            return new List<string> { "ganks mid a lot", "good roamer" };
        }

        if (hero == DotaEnums.Hero.Underlord)
        {
            return new List<string> { "loves to be tanky", "hard to kill" };
        }

        if (hero == DotaEnums.Hero.Terrorblade)
        {
            return new List<string> { "farms well with illusions", "afk farms" };
        }

        if (hero == DotaEnums.Hero.Phoenix)
        {
            return new List<string>
                { "good at landing fire spirits", "solid supernova positioning", "will dive enemies" };
        }

        if (hero == DotaEnums.Hero.Oracle)
        {
            return new List<string> { "amazing false promise saves", "keeps core heroes alive" };
        }

        if (hero == DotaEnums.Hero.Winter_Wyvern)
        {
            return new List<string> { "good curse usage", "good at keeping wave pushed" };
        }

        if (hero == DotaEnums.Hero.Arc_Warden)
        {
            return new List<string> { "annoying to play against", "good clone micromanagement" };
        }

        if (hero == DotaEnums.Hero.Monkey_King)
        {
            return new List<string> { "annoying for melee heroes in lane", "hides in trees" };
        }

        if (hero == DotaEnums.Hero.Dark_Willow)
        {
            return new List<string> { "solo skills enemy heroes with burst damage", "great terroize usage in fights" };
        }

        if (hero == DotaEnums.Hero.Pangolier)
        {
            return new List<string>
                { "creates chaos with rolling thunder", "keeps waves pushed", "annoyingly slippery player" };
        }

        if (hero == DotaEnums.Hero.Grimstroke)
        {
            return new List<string> { "keeps waves pushed", "coordinates ultimate well with teammates" };
        }

        if (hero == DotaEnums.Hero.Hood_Wink)
        {
            return new List<string> { "hides in trees", "will snipe you from afar" };
        }

        if (hero == DotaEnums.Hero.Void_Spirit)
        {
            return new List<string> { "annoyingly slippery player", "will burst you down" };
        }

        if (hero == DotaEnums.Hero.Snapfire)
        {
            return new List<string> { "great at landing kisses", "annoying to deal with in lane" };
        }

        if (hero == DotaEnums.Hero.Mars)
        {
            return new List<string> { "great blink wall usage", "lands spears very well" };
        }

        if (hero == DotaEnums.Hero.Dawnbreaker)
        {
            return new List<string> { "knows when to use solar guardian", "chases you with celestial hammer" };
        }

        if (hero == DotaEnums.Hero.Marci)
        {
            return new List<string> { "throws you under tower", "leaps at you" };
        }

        if (hero == DotaEnums.Hero.Primal_Beast)
        {
            return new List<string> { "stampedes toward enemies ferociously", "executes pulverize often" };
        }

        throw new ArgumentOutOfRangeException(nameof(hero), hero, null);
    }

    public static List<string> Tier1MidLaneWords = new()
    {
        "great mid-laner", "amazing mid player", "skillful in midlane", "dominates mid"
    };

    public static List<string> Tier1SafeLaneWords = new()
    {
        "great safelane player", "amazing position 1 player", "skillful in the safelane",
        "dominates as a position 1", "great last-hitting"
    };

    public static List<string> Tier1OffLaneWords = new()
    {
        "great offlane player", "amazing position 3 player", "skillful in the offlane", "dominates as an offlaner",
        "destroys safelaners"
    };

    public static List<string> Tier1SoftSupportWords = new()
    {
        "great position 4 player", "amazing soft support player", "skillful in the offlane as a support",
        "dominates as an position 4"
    };

    public static List<string> Tier1HardSupportWords = new()
    {
        "great support player", "amazing position 5 player", "dominates as a support", "makes offlane players cry"
    };


    public static List<string> UnknownPlayerWords = new()
    {
        "mysterious player", "dark horse", "unknown", "enigmatic player", "new player"
    };

    public static List<string> UnknownPlayerEndingSentences = new()
    {
        "Make up a funny dota2 fact about this player.",
        "Make up a dota2 joke about this player.",
        "Use big words."
    };

    public static List<string> GetTeamRankWords(int rank, decimal percentileRank)
    {
        if (rank == 1)
        {
            return new List<string> { "best team in league", "#1 rank team" };
        }

        if (rank == 2)
        {
            return new List<string> { "2nd best team", "#2 rank team" };
        }

        if (percentileRank <= 0.2M)
        {
            return new List<string> { "incredible team", "top tier team", "unstoppable team" };
        }

        if (percentileRank <= 0.3M)
        {
            return new List<string> { "good team", "solid team", "high ranked team" };
        }

        if (percentileRank <= 0.4M)
        {
            return new List<string> { "decent team", "respectable team", "commendable team" };
        }

        if (percentileRank <= 0.6M)
        {
            return new List<string> { "okay team", "stuck near the middle", "acceptable team" };
        }

        if (percentileRank <= 0.8M)
        {
            return new List<string> { "could be better", "mediocre team", "needs to improve" };
        }

        return new List<string>
            { "bottom of the barrel team", "bad skilled team", "one of the worst teams", "low rank team" };
    }

    public static List<string> TeamEndingSentences = new()
    {
        "Use sophisticated words.",
        $"Use the word {SuffixToxicBadWords.PickRandom()} {TeamWordUsage.PickRandom()} times.",
        $"Use the word {SuffixWholesomeWords.PickRandom()} {TeamWordUsage.PickRandom()} times.",
        "Talk about their team name.",
        "Say a vulgar word in every sentence."
    };
}