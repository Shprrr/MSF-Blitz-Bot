namespace MSFBlitzBot
{
    internal class UserDataManager
    {
        public class Language
        {
            public string Id;

            public string Name;

            public Language(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static int OverlayUpdateDelay { get; internal set; } = 200;
        public static int EngineUpdateDelay { get; internal set; } = 100;
        public static Language CurrentLanguage { get; internal set; } = new Language("EN", "English");
    }
}