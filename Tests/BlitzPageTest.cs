using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSFBlitzBot;
using MSFBlitzBot.GamePages;

namespace Tests
{
    [TestClass]
    public class BlitzPageTest
    {
        private const string ImageFolder = "BlitzPageImageTests";

        [ClassInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Supprimer le paramètre inutilisé", Justification = "Signature forcée")]
        public static void ClassInitialize(TestContext context)
        {
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Regular.ttf"));
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Medium.ttf"));
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Bold.ttf"));
            HeroManager.Load("gameFiles");
        }

        [TestMethod]
        public void GetData_StryfeIsntBlackPantherInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Stryfe en Black Panther.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("CullObsidian", 414, data.PlayerHeroes[0]);
            AssertHeroData("Thanos", 382, data.PlayerHeroes[1]);
            AssertHeroData("CorvusGlaive", 476, data.PlayerHeroes[2]);
            AssertHeroData("ProximaMidnight", 414, data.PlayerHeroes[3]);
            AssertHeroData("EbonyMaw", 11047, data.PlayerHeroes[4]);
            AssertHeroData("Mystique", 414, data.OpponentHeroes[0]);
            AssertHeroData("Minerva", 414, data.OpponentHeroes[1]);
            AssertHeroData("Sabretooth", 382, data.OpponentHeroes[2]);
            AssertHeroData("MrSinister", 476, data.OpponentHeroes[3]);
            AssertHeroData("Stryfe", 10671, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_AntManIsntBlackPantherInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Ant-man en Black Panther.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("MultipleMan", 29807, data.PlayerHeroes[0]);
            AssertHeroData("Polaris", 28191, data.PlayerHeroes[1]);
            AssertHeroData("Namor", 482, data.PlayerHeroes[2]);
            AssertHeroData("Shatterstar", 63850, data.PlayerHeroes[3]);
            AssertHeroData("Longshot", 56507, data.PlayerHeroes[4]);
            AssertHeroData("CaptainAmerica", 21999, data.OpponentHeroes[0]);
            AssertHeroData("CaptainMarvel", 107014, data.OpponentHeroes[1]);
            AssertHeroData("Thor", 39787, data.OpponentHeroes[2]);
            AssertHeroData("ScarletWitch", 32910, data.OpponentHeroes[3]);
            AssertHeroData("AntMan", 646, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_StryfeIsntBlackPantherInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Stryfe en Black Panther slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("KreeTank_Counter", 17595, data.PlayerHeroes[0]);
            AssertHeroData("Nobu", 7330, data.PlayerHeroes[1]);
            AssertHeroData("KreeDmg_Speed", 13310, data.PlayerHeroes[2]);
            AssertHeroData("KreeSupport_HoT", 15771, data.PlayerHeroes[3]);
            AssertHeroData("KreeControl_Assist", 13408, data.PlayerHeroes[4]);
            AssertHeroData("Wolverine", 18690, data.OpponentHeroes[0]);
            AssertHeroData("Mystique", 8386, data.OpponentHeroes[1]);
            AssertHeroData("Sabretooth", 15740, data.OpponentHeroes[2]);
            AssertHeroData("Stryfe", 15120, data.OpponentHeroes[3]);
            AssertHeroData("MrSinister", 18465, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_RescueIsntBlackPantherInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Rescue en Black Panther.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("ScientistSupreme", 413, data.PlayerHeroes[0]);
            AssertHeroData("AimDmg_Speed", 382, data.PlayerHeroes[1]);
            AssertHeroData("AimDmg_Offense", 446, data.PlayerHeroes[2]);
            AssertHeroData("Graviton", 414, data.PlayerHeroes[3]);
            AssertHeroData("AimTank_Taunt", 381, data.PlayerHeroes[4]);
            AssertHeroData("Thanos", 528, data.OpponentHeroes[0]);
            AssertHeroData("Carnage", 575, data.OpponentHeroes[1]);
            AssertHeroData("SpiderMan", 370, data.OpponentHeroes[2]);
            AssertHeroData("IronMan", 485, data.OpponentHeroes[3]);
            AssertHeroData("Rescue", 484, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_BlackBoltIsntBlackPantherInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Black Bolt en Black Panther.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("SilverSamurai", 415, data.PlayerHeroes[0]);
            AssertHeroData("HandDmg_Bonus", 381, data.PlayerHeroes[1]);
            AssertHeroData("AimControl_Infect", 446, data.PlayerHeroes[2]);
            AssertHeroData("LadyDeathstrike", 383, data.PlayerHeroes[3]);
            AssertHeroData("HandDmg_Unbuff", 287, data.PlayerHeroes[4]);
            AssertHeroData("CaptainMarvel", 346, data.OpponentHeroes[0]);
            AssertHeroData("InvisibleWoman", 533, data.OpponentHeroes[1]);
            AssertHeroData("Phoenix", 452, data.OpponentHeroes[2]);
            AssertHeroData("Minerva", 491, data.OpponentHeroes[3]);
            AssertHeroData("BlackBolt", 453, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_KestrelIsntX23InOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Kestrel en X-23 slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("HandControl_HealBlock", 414, data.PlayerHeroes[0]);
            AssertHeroData("Cloak", 20115, data.PlayerHeroes[1]);
            AssertHeroData("HandSupport_Heal", 2595, data.PlayerHeroes[2]);
            AssertHeroData("Dagger", 20573, data.PlayerHeroes[3]);
            AssertHeroData("Deathpool", 383, data.PlayerHeroes[4]);
            AssertHeroData("Gamora", 454, data.OpponentHeroes[0]);
            AssertHeroData("CaptainMarvel", 21784, data.OpponentHeroes[1]);
            AssertHeroData("Thor", 20836, data.OpponentHeroes[2]);
            AssertHeroData("Sybil", 529, data.OpponentHeroes[3]);
            AssertHeroData("BaronMordo", 2493, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_ToadIsntWinterSoldierInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Toad en Winter Soldier.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("HydraTank_Taunt", 195, data.PlayerHeroes[0]);
            AssertHeroData("HydraDmg_Single", 476, data.PlayerHeroes[1]);
            AssertHeroData("HydraDmg_Buff", 261, data.PlayerHeroes[2]);
            AssertHeroData("HydraSupport_Heal", 542, data.PlayerHeroes[3]);
            AssertHeroData("RedSkull", 28500, data.PlayerHeroes[4]);
            AssertHeroData("Blob", 28269, data.OpponentHeroes[0]);
            AssertHeroData("Magneto", 476, data.OpponentHeroes[1]);
            AssertHeroData("Juggernaut", 542, data.OpponentHeroes[2]);
            AssertHeroData("Pyro", 195, data.OpponentHeroes[3]);
            AssertHeroData("Toad", 261, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_SabretoothIsntBlackBoltInPlayer2()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Black Bolt player.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsFalse(data.HasOpponent);
            AssertHeroData("SilverSamurai", 415, data.PlayerHeroes[0]);
            AssertHeroData("Wolverine", 68461, data.PlayerHeroes[1]);
            AssertHeroData("Sabretooth", 54935, data.PlayerHeroes[2]);
            AssertHeroData("LadyDeathstrike", 10664, data.PlayerHeroes[3]);
            AssertHeroData("ElsaBloodstone", 69213, data.PlayerHeroes[4]);
        }

        [TestMethod]
        public void GetData_PhylaVellIsntCrossbonesInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Phyla-Vell en Crossbones.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("SheHulk", 482, data.PlayerHeroes[0]);
            AssertHeroData("Thing", 687, data.PlayerHeroes[1]);
            AssertHeroData("MrFantastic", 476, data.PlayerHeroes[2]);
            AssertHeroData("HumanTorch", 512, data.PlayerHeroes[3]);
            AssertHeroData("InvisibleWoman", 447, data.PlayerHeroes[4]);
            AssertHeroData("Deadpool", 883, data.OpponentHeroes[0]);
            AssertHeroData("Deathpool", 612, data.OpponentHeroes[1]);
            AssertHeroData("Gamora", 639, data.OpponentHeroes[2]);
            AssertHeroData("X23", 609, data.OpponentHeroes[3]);
            AssertHeroData("PhylaVell", 569, data.OpponentHeroes[4]);
        }

        private static void AssertHeroData(string expectedId, int expectedPower, BlitzHero heroData)
        {
            Assert.AreEqual(expectedId, heroData.Id);
            Assert.AreEqual(expectedPower, heroData.Power);
        }
    }
}
