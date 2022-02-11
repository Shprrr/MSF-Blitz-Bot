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
        public void GetData_SabretoothIsntBlackBoltInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Black Bolt slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("Hela", 96360, data.PlayerHeroes[0]);
            AssertHeroData("BaronMordo", 43141, data.PlayerHeroes[1]);
            AssertHeroData("GhostRider", 58376, data.PlayerHeroes[2]);
            AssertHeroData("ScarletWitch", 23529, data.PlayerHeroes[3]);
            AssertHeroData("DoctorStrange", 54796, data.PlayerHeroes[4]);
            AssertHeroData("Pyro", 93971, data.OpponentHeroes[0]);
            AssertHeroData("Mystique", 26664, data.OpponentHeroes[1]);
            AssertHeroData("Magneto", 58751, data.OpponentHeroes[2]);
            AssertHeroData("Sabretooth", 48074, data.OpponentHeroes[3]);
            AssertHeroData("Juggernaut", 61196, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_SabretoothIsntBlackBoltInOpponent1()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Black Bolt slot 2.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("SamWilson", 105570, data.PlayerHeroes[0]);
            AssertHeroData("MariaHill", 97138, data.PlayerHeroes[1]);
            AssertHeroData("Zemo", 95959, data.PlayerHeroes[2]);
            AssertHeroData("SharonCarter", 109918, data.PlayerHeroes[3]);
            AssertHeroData("Sybil", 115060, data.PlayerHeroes[4]);
            AssertHeroData("Pyro", 124490, data.OpponentHeroes[0]);
            AssertHeroData("Sabretooth", 120911, data.OpponentHeroes[1]);
            AssertHeroData("Toad", 122809, data.OpponentHeroes[2]);
            AssertHeroData("Juggernaut", 113870, data.OpponentHeroes[3]);
            AssertHeroData("Blob", 110000, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_SabretoothIsntBlackBoltInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Black Bolt.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("CullObsidian", 25223, data.PlayerHeroes[0]);
            AssertHeroData("Thanos", 510, data.PlayerHeroes[1]);
            AssertHeroData("CorvusGlaive", 509, data.PlayerHeroes[2]);
            AssertHeroData("ProximaMidnight", 414, data.PlayerHeroes[3]);
            AssertHeroData("EbonyMaw", 45601, data.PlayerHeroes[4]);
            AssertHeroData("BlackPanther", 52174, data.OpponentHeroes[0]);
            AssertHeroData("Deadpool", 775, data.OpponentHeroes[1]);
            AssertHeroData("Hulk", 777, data.OpponentHeroes[2]);
            AssertHeroData("ShieldSupport_Heal", 631, data.OpponentHeroes[3]);
            AssertHeroData("Sabretooth", 31314, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_SabretoothIsntBlackBoltInOpponent2()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Black Bolt slot 3.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("JessicaJones", 352, data.PlayerHeroes[0]);
            AssertHeroData("Mysterio", 15178, data.PlayerHeroes[1]);
            AssertHeroData("Ultron", 168, data.PlayerHeroes[2]);
            AssertHeroData("Shocker", 41036, data.PlayerHeroes[3]);
            AssertHeroData("Vulture", 37965, data.PlayerHeroes[4]);
            AssertHeroData("EmmaFrost", 44864, data.OpponentHeroes[0]);
            AssertHeroData("MrSinister", 46535, data.OpponentHeroes[1]);
            AssertHeroData("Sabretooth", 513, data.OpponentHeroes[2]);
            AssertHeroData("Stryfe", 299, data.OpponentHeroes[3]);
            AssertHeroData("Mystique", 18167, data.OpponentHeroes[4]);
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
        public void GetData_SpiderPunkIsntJessicaJonesInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Spider-Punk en Jessica Jones slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("RavagerSupport_Heal", 418, data.PlayerHeroes[0]);
            AssertHeroData("Yondu", 4308, data.PlayerHeroes[1]);
            AssertHeroData("RavagerDmg_AoE", 881, data.PlayerHeroes[2]);
            AssertHeroData("ShieldDmg_AoE", 376, data.PlayerHeroes[3]);
            AssertHeroData("RavagerTank_Taunt", 382, data.PlayerHeroes[4]);
            AssertHeroData("SymbioteSpiderMan", 5092, data.OpponentHeroes[0]);
            AssertHeroData("SpiderMan", 485, data.OpponentHeroes[1]);
            AssertHeroData("ScarletSpider", 516, data.OpponentHeroes[2]);
            AssertHeroData("SpiderPunk", 1042, data.OpponentHeroes[3]);
            AssertHeroData("UltSpiderMan", 477, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_PhylaVellIsntCrossbonesInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Phyla-Vell en Crossbones slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("ShieldTank_Stun", 460, data.PlayerHeroes[0]);
            AssertHeroData("Sybil", 115060, data.PlayerHeroes[1]);
            AssertHeroData("NickFury", 50756, data.PlayerHeroes[2]);
            AssertHeroData("ShieldDmg_Defense", 5837, data.PlayerHeroes[3]);
            AssertHeroData("ShieldSupport_Stealth", 381, data.PlayerHeroes[4]);
            AssertHeroData("GhostRider", 7612, data.OpponentHeroes[0]);
            AssertHeroData("Hulk", 574, data.OpponentHeroes[1]);
            AssertHeroData("Gamora", 60272, data.OpponentHeroes[2]);
            AssertHeroData("PhylaVell", 494, data.OpponentHeroes[3]);
            AssertHeroData("Nebula", 122000, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_StarLordIsntBlackPantherInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Star-Lord en Black Panther slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("SilverSamurai", 415, data.PlayerHeroes[0]);
            AssertHeroData("Wolverine", 68461, data.PlayerHeroes[1]);
            AssertHeroData("Sabretooth", 54935, data.PlayerHeroes[2]);
            AssertHeroData("LadyDeathstrike", 10664, data.PlayerHeroes[3]);
            AssertHeroData("ElsaBloodstone", 69213, data.PlayerHeroes[4]);
            AssertHeroData("MrSinister", 87219, data.OpponentHeroes[0]);
            AssertHeroData("ShieldSupport_Heal", 676, data.OpponentHeroes[1]);
            AssertHeroData("CaptainMarvel", 67819, data.OpponentHeroes[2]);
            AssertHeroData("StarLord", 81807, data.OpponentHeroes[3]);
            AssertHeroData("LukeCage", 13503, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_SabretoothIsntSpiderManMilesInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Miles slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("Juggernaut", 43546, data.PlayerHeroes[0]);
            AssertHeroData("Magneto", 29001, data.PlayerHeroes[1]);
            AssertHeroData("Toad", 25144, data.PlayerHeroes[2]);
            AssertHeroData("Blob", 28768, data.PlayerHeroes[3]);
            AssertHeroData("Pyro", 22614, data.PlayerHeroes[4]);
            AssertHeroData("LukeCage", 44219, data.OpponentHeroes[0]);
            AssertHeroData("ScarletWitch", 23207, data.OpponentHeroes[1]);
            AssertHeroData("SpiderMan", 29009, data.OpponentHeroes[2]);
            AssertHeroData("Sabretooth", 27682, data.OpponentHeroes[3]);
            AssertHeroData("Wolverine", 24712, data.OpponentHeroes[4]);
        }

        [TestMethod]
        public void GetData_AntManIsntBlackPantherInOpponent3()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Ant-man en Black Panther slot 4.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("SheHulk", 482, data.PlayerHeroes[0]);
            AssertHeroData("Thing", 687, data.PlayerHeroes[1]);
            AssertHeroData("MrFantastic", 476, data.PlayerHeroes[2]);
            AssertHeroData("HumanTorch", 512, data.PlayerHeroes[3]);
            AssertHeroData("InvisibleWoman", 447, data.PlayerHeroes[4]);
            AssertHeroData("MrSinister", 603, data.OpponentHeroes[0]);
            AssertHeroData("SilverSurfer", 614, data.OpponentHeroes[1]);
            AssertHeroData("Groot", 869, data.OpponentHeroes[2]);
            AssertHeroData("AntMan", 562, data.OpponentHeroes[3]);
            AssertHeroData("Thor", 628, data.OpponentHeroes[4]);
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

        [TestMethod]
        public void GetData_SabretoothIsntToadInOpponent4()
        {
            BlitzPage page = new();
            var image = (Bitmap)Image.FromFile(Path.Combine(ImageFolder, "ScreenshotGame Sabretooth en Toad.png"));
            Emulator.GameImage.Initialize(image);
            page.SetImage(Emulator.GameImage);

            var data = page.GetData();

            Assert.IsTrue(data.HasOpponent);
            AssertHeroData("Groot", 21515, data.PlayerHeroes[0]);
            AssertHeroData("RocketRaccoon", 30504, data.PlayerHeroes[1]);
            AssertHeroData("StarLord", 295, data.PlayerHeroes[2]);
            AssertHeroData("Vision", 19145, data.PlayerHeroes[3]);
            AssertHeroData("Minerva", 79698, data.PlayerHeroes[4]);
            AssertHeroData("Daredevil", 23988, data.OpponentHeroes[0]);
            AssertHeroData("Punisher", 32110, data.OpponentHeroes[1]);
            AssertHeroData("Hulk", 423, data.OpponentHeroes[2]);
            AssertHeroData("RocketRaccoon", 21680, data.OpponentHeroes[3]);
            AssertHeroData("Sabretooth", 91085, data.OpponentHeroes[4]);
        }

        private static void AssertHeroData(string expectedId, int expectedPower, BlitzHero heroData)
        {
            Assert.AreEqual(expectedId, heroData.Id);
            Assert.AreEqual(expectedPower, heroData.Power);
        }
    }
}
