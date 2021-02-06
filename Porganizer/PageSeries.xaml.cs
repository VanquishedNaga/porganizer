using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class PageSeries : Page
    {
        public ObservableCollection<Series> seriesList = new ObservableCollection<Series>();

        public PageSeries()
        {
            this.InitializeComponent();

            // Read the list of seriess from database.
            LoadSeriessFromDatabase();

            AssessButtons();
        }

        void AssessButtons()
        {
            if (seriesListView.SelectedItem == null)
            {
                DeleteSeriesButton.IsEnabled = false;
                EditSeriesButton.IsEnabled = false;
            }
            else
            {
                DeleteSeriesButton.IsEnabled = true;
                EditSeriesButton.IsEnabled = true;
            }
        }

        private void SeriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssessButtons();
        }

        private void LoadSeriessFromDatabase()
        {
            seriesList = DataAccess.GetSeriesList();
        }

        private void AddSeries_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FormSeries));
        }

        private void DeleteSeries_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayDeleteDialog();
        }

        private async void DisplayDeleteDialog()
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete series?",
                Content = "Are you sure you want to permanently delete " + ((Series)seriesListView.SelectedItem).Name + "?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DeleteSeries();
            }
        }

        void DeleteSeries()
        {
            if (seriesListView.SelectedItem is Series series)
            {
                DataAccess.DeleteSeries(series.Name);

                // Refresh the list.
                LoadSeriessFromDatabase();
                seriesListView.ItemsSource = seriesList;
            }
        }

        private void EditSeries_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (seriesListView.SelectedItem is Series series)
            {
                this.Frame.Navigate(typeof(FormSeries), series);
            }
        }
    }
}
