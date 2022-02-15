using System.Drawing;

namespace MSFBlitzBot.GamePages
{
    public class BlitzData
    {
        public BlitzHero[] PlayerHeroes { get; private set; } = new BlitzHero[5];
        public bool CanFindOpponent { get; set; }
        public bool HasOpponent { get; set; }
        public int TeamIndex { get; set; } = -1;
        public BlitzHero[] OpponentHeroes { get; private set; } = new BlitzHero[5];

        public bool Victory { get; set; }
        public bool Defeat { get; set; }
    }

    public class BlitzPage : GamePage
    {
        private FImage screenImage;

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
            if (Emulator.GameImage.GetPixel(0.5f, 0.06f).IsCloseTo(Color.FromArgb(5, 32, 57), 10)
                && Emulator.GameImage.GetPixel(0.5f, 744f / 1440).IsCloseTo(Color.FromArgb(95, 249, 252), 10)
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
            if (screenImage == null || image.Width != screenImage!.Width)
            {
                HeroManager.LoadPortraits("gameFiles", 88 * image.Width / 1505, 139 * image.Height / 847);
            }
            screenImage = image;
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
                data.PlayerHeroes[i] = new(GetHeroId(0, i, out var accuracy), GetHeroPower(0, i), accuracy);
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
                    data.OpponentHeroes[i] = new(GetHeroId(1, i, out var accuracy), GetHeroPower(1, i), accuracy);
                }
            }
            else
                data.CanFindOpponent = pixelOpponentButton.IsCloseTo(Color.FromArgb(249, 181, 28), 3);

            return data;
        }

        private string GetHeroId(int side, int index, out float accuracy)
        {
            // side 0 index 0 =  57, 343
            // side 0 index 1 = 138, 175
            // side 0 index 2 = 221, 343
            // side 0 index 3 = , 175
            // side 0 index 4 = , 343
            // side 1 index 0 = 1026, 343
            float posx = (index * 82 + 57f + side * 971) / 1504;
            float posy = (index % 2 == 1 ? 171f : 340f) / 846;
            string[] heroIds = HeroManager.HeroIds;
            string id = string.Empty;
            byte b = 0;
            foreach (var heroId in heroIds)
            {
                FImage portrait = HeroManager.GetPortrait(heroId);
                string debugFilename = null;
                //debugFilename = $"PortraitHero_{side}_{index}";
                byte b2 = screenImage!.Match(portrait, posx, posy, 0.76f, (byte)30u, 100, FImage.MatchColorMode.Color, debugFilename);
                if (b2 > b)
                {
                    b = b2;
                    id = heroId;
                }
            }
            //Debug matching portrait
            //FImage p = HeroManager.GetPortrait(id); string d = $"PortraitHero_{side}_{index}";
            //_img!.Match(p, posx, posy, 0.75f, (byte)30u, 100, FImage.MatchColorMode.Color, d);
            accuracy = b / 255f;
            return HeroManager.GetId(id);
        }

        private string GetHeroPower(int side, int index)
        {
            float posx = (index * 82 + 55f + side * 969) / 1505;
            float posy = (index % 2 == 1 ? 267f : 435f) / 847;
            string text = screenImage.ReadText(new RectangleF(posx, posy, 0.07f, 45f / 847f), 23f, CharactersImageSet.UltimusMed, 4294964894u, detectionY: 0.5f, dist1: 5, dist2: 15);
            return text;
        }
    }
}
