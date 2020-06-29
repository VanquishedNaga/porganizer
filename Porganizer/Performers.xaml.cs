using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class Performers : Page
    {
        public ObservableCollection<Performer> performersList = new ObservableCollection<Performer>();

        public Performers()
        {
            this.InitializeComponent();

            // Read the list of performers from database.
            LoadPerformersFromDatabase();

            AssessButtons();
        }

        void AssessButtons()
        {
            if (performerListView.SelectedItem == null)
            {
                DeletePerformerButton.IsEnabled = false;
                EditPerformerButton.IsEnabled = false;
            }
            else
            {
                DeletePerformerButton.IsEnabled = true;
                EditPerformerButton.IsEnabled = true;
            }
        }

        private void PerformerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssessButtons();
        }

        private void LoadPerformersFromDatabase()
        {
            performersList = DataAccess.GetPerformerList();
        }

        private void AddPerformer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PerformerForm));
        }

        private void DeletePerformer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (performerListView.SelectedItem is Performer performer)
            {
                DataAccess.DeletePerformer(performer.Name);

                // Refresh the list.
                LoadPerformersFromDatabase();
                performerListView.ItemsSource = performersList;
            }
        }

        private void EditPerformer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (performerListView.SelectedItem is Performer performer)
            {
                this.Frame.Navigate(typeof(PerformerForm), performer);
            }
        }
    }
}
