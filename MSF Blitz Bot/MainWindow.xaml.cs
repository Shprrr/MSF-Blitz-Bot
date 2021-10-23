using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace MSFBlitzBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Overlay overlay;

        public static string EmulatorDebug => $"{Emulator.IsValid} {Emulator.GetGameScreenArea()}";
        public static string CurrentPage => Engine.Singleton.CurrentPageId.ToString();

        public MainWindow()
        {
            InitializeComponent();
            HeroManager.Load("gameFiles");
            if (Emulator.IsValid)
            {
                overlay = new Overlay();
                if (Emulator.Id == Emulator.EmulatorId.NemuPlayer)
                {
                    overlay.Topmost = true;
                }
                else
                {
                    new WindowInteropHelper(overlay).Owner = Emulator.AppHandle;
                }
                overlay.Show();
                BlitzViewModel.LoadFights("blitzFights.json");
                // Need to convert json to sqlite.
                if (BlitzViewModel.Fights.Count > 0)
                {
                    Task.Run(() => SQLEngine.SaveFightsAsync(BlitzViewModel.Fights)).GetAwaiter().GetResult();
                    File.Delete("blitzFights.json");
                }
                else
                    BlitzViewModel.LoadFightsAsync().ConfigureAwait(false);
            }
            OverlayViewModel.StartLoopTask();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Emulator.IsValid)
            {
                overlay.Close();
                overlay = null;
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtEmulatorDebug.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            txtCurrentPage.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }
    }
}
