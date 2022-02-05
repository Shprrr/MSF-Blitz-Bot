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
