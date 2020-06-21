using DataAccessLibrary;
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
        }

        private void LoadPerformersFromDatabase()
        {
            performersList = DataAccess.GetPerformerList();
        }

        private void AddPerformer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PerformerForm));
        }
    }
}
