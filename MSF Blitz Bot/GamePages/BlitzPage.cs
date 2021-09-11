using System.Drawing;

namespace MSFBlitzBot.GamePages
{
    public class BlitzData
    {
        public struct Hero
        {
            public string Id { get; private set; }
            public string Name { get; private set; }
            public int? Power { get; private set; }
            public string PowerString { get; private set; }

            public Hero(string id, int? power) : this()
            {
                Id = id;
                Name = HeroManager.GetName(id);
                Power = power;
            }

            public Hero(string id, string power) : this()
            {
                Id = id;
                Name = HeroManager.GetName(id);
                if (int.TryParse(power, out var powerInt))
                    Power = powerInt;
                PowerString = power;
            }
        }

        public Hero[] PlayerHeroes { get; private set; } = new Hero[5];
        public bool CanFindOpponent { get; set; }
        public bool HasOpponent { get; set; }
        public int TeamIndex { get; set; } = -1;
        public Hero[] OpponentHeroes { get; private set; } = new Hero[5];

        public bool Victory { get; set; }
        public bool Defeat { get; set; }
    }

    internal class BlitzPage : GamePage
    {
        private FImage _img;

        public BlitzPage()
            : base(GamePageId.Blitz)
        {
        }

        public override bool IsCurrentPage()
        {
            return IsCurrentPageStatic();
        }

        public static bool IsCurrentPageStatic()
        {
            if (!Emulator.IsValid)
            {
                return false;
            }

            // Normal blitz page
            // 1280, 432, 2560, 1440  67, 69, 82
            // 752, 254, 1505, 847  72, 74, 89
            // 180, 1152, 2560, 1440  101, 253, 255
            if (Emulator.GameImage.GetPixel(0.5f, 0.3f).IsCloseTo(Color.FromArgb(72, 74, 89), 10)
                && Emulator.GameImage.GetPixel(0.5f, 744f / 1440).IsCloseTo(Color.FromArgb(101, 252, 254), 3)
                && Emulator.GameImage.GetPixel(0.0703125f, 0.8f).IsCloseTo(Color.FromArgb(101, 253, 255), 3))
                return true;

            // Victory screen
            // 625, 125, 1505, 847  255, 255, 255
            // 600, 132, 1505, 847  9, 127, 177
            // 177, 132, 1505, 847  80, 131, 162
            if (Emulator.GameImage.GetPixel(625f / 1505, 125f / 847).IsCloseTo(Color.White, 3)
                && Emulator.GameImage.GetPixel(600f / 1505, 132f / 847).IsCloseTo(Color.FromArgb(9, 127, 177), 10))
                return true;

            // Defeat screen
            // 645, 125, 1505, 847  255, 255, 255
            // 620, 132, 1505, 847  150, 52, 77
            // 177, 132, 1505, 847  212, 78, 88
            if (Emulator.GameImage.GetPixel(645f / 1505, 125f / 847).IsCloseTo(Color.White, 3)
                && Emulator.GameImage.GetPixel(620f / 1505, 132f / 847).IsCloseTo(Color.FromArgb(150, 52, 77), 10))
                return true;

            return false;
        }

        protected override void OnOpenInternal()
        {
        }

        protected override void OnCloseInternal()
        {
        }

        public void SetImage(FImage image)
        {
            if (_img == null || image.Width != _img!.Width)
            {
                HeroManager.LoadPortraits("gameFiles", 88 * image.Width / 1505, 139 * image.Height / 847);
            }
            _img = image;
        }

        public BlitzData GetData()
        {
            var data = new BlitzData();

            if (Emulator.GameImage.GetPixel(625f / 1505, 125f / 847).IsCloseTo(Color.White, 3)
                && Emulator.GameImage.GetPixel(600f / 1505, 132f / 847).IsCloseTo(Color.FromArgb(9, 127, 177), 10))
            {
                data.Victory = true;
                return data;
            }
            if (Emulator.GameImage.GetPixel(645f / 1505, 125f / 847).IsCloseTo(Color.White, 3)
                && Emulator.GameImage.GetPixel(620f / 1505, 132f / 847).IsCloseTo(Color.FromArgb(150, 52, 77), 10))
            {
                data.Defeat = true;
                return data;
            }

            for (var i = 0; i < 5; i++)
            {
                data.PlayerHeroes[i] = new(GetHeroId(0, i), GetHeroPower(0, i));
            }

            // 1180, 640  0, 223, 251   New Opponent
            // 1180, 640  249, 181, 28  Find Opponent
            Color pixelOpponentButton = Emulator.GameImage.GetPixel(1180f / 1505, 640f / 847);
            if (pixelOpponentButton.IsCloseTo(Color.FromArgb(0, 223, 251), 3) || pixelOpponentButton.IsCloseTo(Color.FromArgb(4, 158, 229), 3))
            {
                data.HasOpponent = true;
                if (Emulator.GameImage.GetPixel(1200f / 1505, 710f / 847).IsCloseTo(Color.FromArgb(0, 197, 243), 3))
                    data.TeamIndex = 0;
                if (Emulator.GameImage.GetPixel(1290f / 1505, 710f / 847).IsCloseTo(Color.FromArgb(0, 197, 243), 3))
                    data.TeamIndex = 1;
                if (Emulator.GameImage.GetPixel(1380f / 1505, 710f / 847).IsCloseTo(Color.FromArgb(0, 197, 243), 3))
                    data.TeamIndex = 2;

                for (var i = 0; i < 5; i++)
                {
                    data.OpponentHeroes[i] = new(GetHeroId(1, i), GetHeroPower(1, i));
                }
            }
            else
                data.CanFindOpponent = pixelOpponentButton.IsCloseTo(Color.FromArgb(249, 181, 28), 3);

            return data;
        }

        private string GetHeroId(int side, int index)
        {
            // side 0 index 0 =  57, 343
            // side 0 index 1 = 138, 175
            // side 0 index 2 = 221, 343
            // side 0 index 3 = , 175
            // side 0 index 4 = , 343
            // side 1 index 0 = 1026, 343
            float posx = (index * 82 + 57f + side * 969) / 1505;
            float posy = (index % 2 == 1 ? 175f : 343f) / 847;
            string[] heroIds = HeroManager.HeroIds;
            string id = string.Empty;
            byte b = 0;
            foreach (var heroId in heroIds)
            {
                FImage portrait = HeroManager.GetPortrait(heroId);
                string debugFilename = null;
                //debugFilename = $"PortraitHero_{side}_{index}";
                byte b2 = _img!.Match(portrait, posx, posy, 0.75f, (byte)30u, 100, FImage.MatchColorMode.Color, debugFilename);
                if (b2 > b)
                {
                    b = b2;
                    id = heroId;
                }
            }
            return HeroManager.GetId(id);
        }

        private string GetHeroPower(int side, int index)
        {
            float posx = (index * 82 + 55f + side * 969) / 1505;
            float posy = (index % 2 == 1 ? 267f : 435f) / 847;
            string text = _img.ReadText(new RectangleF(posx, posy, 0.07f, 45f / 847f), 23f, CharactersImageSet.UltimusMed, 4294964894u, detectionY: 0.5f, dist1: 5, dist2: 15);
            return text;
        }
    }
}
