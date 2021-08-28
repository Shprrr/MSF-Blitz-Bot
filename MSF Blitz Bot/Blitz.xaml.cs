using System.Windows;
using System.Windows.Controls;

namespace MSFBlitzBot
{
    /// <summary>
    /// Logique d'interaction pour Blitz.xaml
    /// </summary>
    public partial class Blitz : UserControl
    {
        public Blitz()
        {
            InitializeComponent();
        }

        private void ButtonBattle_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as BlitzViewModel;
            model.CombatState = model.CombatState == "Lobby" ? "Battle" : "Lobby";
        }

        private void ButtonRetrain_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as BlitzViewModel;
            model.Retrain();
        }

        private void ToggleButtonAutoBestTarget_Click(object sender, RoutedEventArgs e)
        {
            btnAutoHighestTotal.IsChecked = false;
            btnAutoTrainWorthy.IsChecked = false;

            var model = DataContext as BlitzViewModel;
            model.CurrentAutoState = btnAutoBestTarget.IsChecked.GetValueOrDefault() ? BlitzViewModel.AutoState.BestTarget : BlitzViewModel.AutoState.None;
            model.DoAutoBlitz();
        }

        private void ToggleButtonAutoHighestTotal_Click(object sender, RoutedEventArgs e)
        {
            btnAutoBestTarget.IsChecked = false;
            btnAutoTrainWorthy.IsChecked = false;

            var model = DataContext as BlitzViewModel;
            model.CurrentAutoState = btnAutoHighestTotal.IsChecked.GetValueOrDefault() ? BlitzViewModel.AutoState.HighestTotal : BlitzViewModel.AutoState.None;
            model.DoAutoBlitz();
        }

        private void ToggleButtonAutoTrainWorthy_Click(object sender, RoutedEventArgs e)
        {
            btnAutoBestTarget.IsChecked = false;
            btnAutoHighestTotal.IsChecked = false;

            var model = DataContext as BlitzViewModel;
            model.CurrentAutoState = btnAutoTrainWorthy.IsChecked.GetValueOrDefault() ? BlitzViewModel.AutoState.TrainWorthy : BlitzViewModel.AutoState.None;
            model.DoAutoBlitz();
        }
    }
}
