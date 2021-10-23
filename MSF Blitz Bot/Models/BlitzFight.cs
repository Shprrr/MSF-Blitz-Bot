using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSFBlitzBot
{
    public class BlitzFight
    {
        public enum FightResult
        {
            Defeat,
            Victory
        }

        public BlitzFight()
        {
        }

        public BlitzFight(int? id, BlitzHero[] playerHeroes, BlitzHero[] opponentHeroes, FightResult result, DateTime dateTime)
        {
            Id = id;
            PlayerHeroes = playerHeroes ?? throw new ArgumentNullException(nameof(playerHeroes));
            OpponentHeroes = opponentHeroes ?? throw new ArgumentNullException(nameof(opponentHeroes));
            Result = result;
            DateTime = dateTime;
        }

        public int? Id { get; set; }

        private BlitzHero[] playerHeroes;
        private BlitzHero[] opponentHeroes;
        private string playerHeroesString;
        private string opponentHeroesString;

        [JsonIgnore]
        public bool IsEmpty => PlayerHeroes == null || OpponentHeroes == null || PlayerHeroes.All(h => h.Id == null) || OpponentHeroes.All(h => h.Id == null);

        public BlitzHero[] PlayerHeroes
        {
            get => playerHeroes;
            set
            {
                playerHeroes = value;
                playerHeroesString = string.Join(" + ", playerHeroes.Select(h => $"{h.Name} ({h.PowerString})"));
            }
        }
        public BlitzHero[] OpponentHeroes
        {
            get => opponentHeroes;
            set
            {
                opponentHeroes = value;
                opponentHeroesString = string.Join(" + ", opponentHeroes.Select(h => $"{h.Name} ({h.PowerString})"));
            }
        }
        public FightResult Result { get; set; }
        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            var s = "";
            switch (Result)
            {
                case FightResult.Victory:
                    s += "V";
                    break;
                case FightResult.Defeat:
                    s += "D";
                    break;
            }

            s += $" {playerHeroesString} vs {opponentHeroesString}";

            return s;
        }
    }

    public class BlitzFightJsonConverter : JsonConverter<BlitzFight>
    {
        private readonly DateTime fileDateTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDateTime">DateTime to use when DateTime was not in data.</param>
        public BlitzFightJsonConverter(DateTime fileDateTime)
        {
            this.fileDateTime = fileDateTime;
        }

        public override BlitzFight ReadJson(JsonReader reader, Type objectType, BlitzFight existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var value = token.ToObject<BlitzFight>();

            // Reverse results because enum values has been inverted.
            value.Result = value.Result == BlitzFight.FightResult.Victory ? BlitzFight.FightResult.Defeat : BlitzFight.FightResult.Victory;

            if (value.DateTime.Ticks == 0)
                value.DateTime = fileDateTime;
            return value;
        }

        public override void WriteJson(JsonWriter writer, BlitzFight value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
