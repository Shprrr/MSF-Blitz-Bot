using System.IO;
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
            }
            OverlayViewModel.StartLoopTask();
            BlitzViewModel.Fights.CollectionChanged += Fights_CollectionChanged;
        }

        private void Fights_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            File.WriteAllText("blitzFights.json", Newtonsoft.Json.JsonConvert.SerializeObject(BlitzViewModel.Fights.ToArray()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
