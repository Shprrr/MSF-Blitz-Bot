using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

namespace MSFBlitzBot
{
    internal class HeroManager
    {
        public class Hero
        {
            public int UnlockStars = -1;

            public string Id;

            public string Name;

            public FImage Portrait;
        }

        private static readonly Dictionary<string, Hero> _hero = new();

        private static int _maxWidth = -1;

        private static int _maxHeight = -1;

        public static string[] HeroIds => _hero.Keys.ToArray();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Ne pas ignorer les résultats des méthodes", Justification = "We want default values and not exceptions")]
        public static void Load(string path)
        {
            var pathHeroSheet = Path.Combine(path, "Config", "heroes", "M3HeroSheet.json");
            ClearPortraits();
            _hero.Clear();

            using StreamReader streamReader2 = new(pathHeroSheet);
            foreach (JProperty item in JObject.Parse(streamReader2.ReadToEnd()).Children().ToList())
            {
                var name = item.Name;
                bool.TryParse(item.Value["player_Character"]!.ToString().ToLower(), out var result7);
                if (result7)
                {
                    if (!File.Exists(Path.Combine(path, "RosterPortraits\\Portrait_" + name + ".png")))
                    {
                        //LogManager.Log("HeroManager: Can't find portrait of " + name);
                        continue;
                    }
                    Hero hero = new()
                    {
                        Id = item.Name
                    };
                    int.TryParse(item.Value["unlock_StarLevel"]!.ToString(), out hero.UnlockStars);
                    _hero[name.ToUpper(CultureInfo.InvariantCulture)] = hero;
                }
            }
            LoadHeroNames(path);
        }

        public static void LoadHeroNames(string path)
        {
            using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine(path, "Config\\m3localization\\", UserDataManager.CurrentLanguage!.Id, "heroes.csv"));
            textFieldParser.CommentTokens = new string[1] { "#" };
            textFieldParser.SetDelimiters(",");
            textFieldParser.HasFieldsEnclosedInQuotes = true;
            textFieldParser.ReadLine();
            while (!textFieldParser.EndOfData)
            {
                string[] array = textFieldParser.ReadFields();
                string text = array[0];
                string name = array[1];
                if (text.StartsWith("ID_SHARD_") && text.EndsWith("_NAME"))
                {
                    text = text["ID_SHARD_".Length..^"_NAME".Length];
                    if (_hero.ContainsKey(text))
                    {
                        _hero[text].Name = name;
                    }
                }
            }
        }

        public static void LoadPortraits(string path, int maxWidth, int maxHeight)
        {
            if (_maxWidth == maxWidth && _maxHeight == maxHeight)
            {
                return;
            }
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
            ClearPortraits();
            foreach (KeyValuePair<string, Hero> item in _hero)
            {
                using FImage fImage = new(Path.Combine(path, "RosterPortraits", "Portrait_" + item.Value.Id + ".png"));
                item.Value.Portrait = fImage.GetCenteredScale(maxWidth, maxHeight);
            }
        }

        public static void ClearPortraits()
        {
            foreach (Hero value in _hero.Values)
            {
                if (value.Portrait != null)
                {
                    value.Portrait.Dispose();
                }
            }
        }

        public static string GetId(string id)
        {
            id = id.ToUpper(CultureInfo.InvariantCulture);
            return _hero.ContainsKey(id) ? _hero[id].Id : id;
        }

        public static string GetName(string id)
        {
            id = id.ToUpper(CultureInfo.InvariantCulture);
            return _hero.ContainsKey(id) && !string.IsNullOrEmpty(_hero[id].Name) ? _hero[id].Name : id;
        }

        public static FImage GetPortrait(string id)
        {
            id = id.ToUpper(CultureInfo.InvariantCulture);
            return _hero.ContainsKey(id) ? _hero[id].Portrait : null;
        }
    }
}
