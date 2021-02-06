using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class PagePerformers : Page
    {
        public ObservableCollection<Performer> performersList = new ObservableCollection<Performer>();

        public PagePerformers()
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
            this.Frame.Navigate(typeof(FormPerformer));
        }

        private void DeletePerformer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayDeleteDialog();
        }

        private async void DisplayDeleteDialog()
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete performer?",
                Content = "Are you sure you want to permanently delete " + ((Performer)performerListView.SelectedItem).Name + "?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DeletePerformer();
            }
        }

        void DeletePerformer()
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
                this.Frame.Navigate(typeof(FormPerformer), performer);
            }
        }
    }
}
