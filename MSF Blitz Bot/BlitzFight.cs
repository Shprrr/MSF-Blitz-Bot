using System;
using System.Linq;
using MSFBlitzBot.GamePages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSFBlitzBot
{
    public class BlitzFight
    {
        public enum FightResult
        {
            Victory,
            Defeat
        }

        public BlitzFight()
        {
        }

        private BlitzData.Hero[] playerHeroes;
        private BlitzData.Hero[] opponentHeroes;
        private string playerHeroesString;
        private string opponentHeroesString;

        [JsonIgnore]
        public bool IsEmpty => PlayerHeroes == null || OpponentHeroes == null || PlayerHeroes.All(h => h.Id == null) || OpponentHeroes.All(h => h.Id == null);

        public BlitzData.Hero[] PlayerHeroes
        {
            get => playerHeroes;
            set
            {
                playerHeroes = value;
                playerHeroesString = string.Join(" + ", playerHeroes.Select(h => $"{h.Name} ({h.PowerString})"));
            }
        }
        public BlitzData.Hero[] OpponentHeroes
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
