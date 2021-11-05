using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace MSFBlitzBot
{
    public static class SQLEngine
    {
        private const string DatabasePath = "blitzData.db";
        private static readonly SQLiteConnectionString ConnectionString = new(DatabasePath, SQLiteOpenFlags.ReadWrite, false,
            dateTimeStringFormat: "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");

        public static async Task<IEnumerable<BlitzFight>> LoadBlitzFightsAsync()
        {
            SQLiteAsyncConnection db = new(ConnectionString);

            var fights = await db.Table<Fight>().ToArrayAsync();
            var fightHeroes = await db.Table<FightHero>().ToArrayAsync();
            var outdatedData = Newtonsoft.Json.JsonConvert.DeserializeObject<OutdatedData[]>(System.IO.File.ReadAllText("gameFiles/outdatedData.json"));

            return fights
                .GroupJoin(fightHeroes, f => f.Id, fh => fh.FightId, (f, fh) => (fight: f, heroes: fh))
                .Where(f => !outdatedData.Any(o => f.fight.DateTime <= o.DateBefore && f.heroes.Any(fh => fh.CharacterId == o.CharacterId)))
                .Select(f => new BlitzFight(f.fight.Id, BuildBlitzHeroes(f.heroes, HeroSide.Player), BuildBlitzHeroes(f.heroes, HeroSide.Opponent), (BlitzFight.FightResult)f.fight.Result, f.fight.DateTime.ToUniversalTime()));

            static BlitzHero[] BuildBlitzHeroes(IEnumerable<FightHero> fh, HeroSide side)
                => fh.Where(h => h.Side == (int)side).OrderBy(h => h.Index)
                    .Select(h => new BlitzHero(h.CharacterId, h.Power.ToString(CultureInfo.InvariantCulture), h.Accuracy)).ToArray();
        }

        public static async Task SaveFightsAsync(IEnumerable<BlitzFight> blitzFights)
        {
            SQLiteAsyncConnection db = new(ConnectionString);

            await db.RunInTransactionAsync(c =>
            {
                foreach (var blitzFight in blitzFights)
                {
                    Fight fight = new(blitzFight);
                    c.InsertOrReplace(fight);
                    blitzFight.Id = fight.Id;

                    for (int i = 0; i < blitzFight.PlayerHeroes.Length; i++)
                    {
                        FightHero fightHero = new(blitzFight.PlayerHeroes[i], fight.Id.Value, HeroSide.Player, i);
                        c.InsertOrReplace(fightHero);
                    }

                    for (int i = 0; i < blitzFight.OpponentHeroes.Length; i++)
                    {
                        FightHero fightHero = new(blitzFight.OpponentHeroes[i], fight.Id.Value, HeroSide.Opponent, i);
                        c.InsertOrReplace(fightHero);
                    }
                }
            });
        }

        [Table("fights")]
        private class Fight
        {
            [PrimaryKey, AutoIncrement]
            [Column("fight_id")]
            public int? Id { get; set; }

            [Column("result")]
            public int Result { get; set; }

            [Column("date_time")]
            public DateTime DateTime { get; set; }

            public Fight()
            {
            }

            public Fight(BlitzFight blitzFight)
            {
                Id = blitzFight.Id;
                Result = (int)blitzFight.Result;
                DateTime = blitzFight.DateTime;
            }
        }

        private enum HeroSide
        {
            Player,
            Opponent
        }

        [Table("fight_heroes")]
        private class FightHero
        {
            [PrimaryKey]
            [Column("fight_id")]
            public int FightId { get; set; }

            [PrimaryKey]
            [Column("side")]
            public int Side { get; set; }

            [PrimaryKey]
            [Column("index")]
            public int Index { get; set; }

            [Column("character_id")]
            public string CharacterId { get; set; }

            [Column("power")]
            public int Power { get; set; }

            [Column("accuracy")]
            public float Accuracy { get; set; }

            public FightHero()
            {
            }

            public FightHero(BlitzHero blitzHero, int fightId, HeroSide side, int index)
            {
                FightId = fightId;
                Side = (int)side;
                Index = index;
                CharacterId = blitzHero.Id;
                Power = blitzHero.Power.Value;
                Accuracy = blitzHero.Accuracy;
            }
        }

        private class OutdatedData
        {
            public string CharacterId { get; set; }
            public DateTime DateBefore { get; set; }
        }
    }
}
