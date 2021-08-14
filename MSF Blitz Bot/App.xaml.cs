using System.IO;
using System.Windows;

namespace MSFBlitzBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Regular.ttf"));
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Medium.ttf"));
            FontManager.Load(Path.Combine("Fonts", "Ultimus-Bold.ttf"));
            Emulator.Initialize();
            if (!Emulator.IsValid)
            {
                MessageBox.Show("You need to start your Emulator before MANTIS.\nSee MANTIS as a parasite, it can't survive without a host.", "OOPS");
                Current.Shutdown();
                return;
            }
        }
    }
}
