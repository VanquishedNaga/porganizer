using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Porganizer
{
    public sealed partial class SeriesForm : Page
    {
        private Series temp = null;
        public SeriesForm()
        {
            this.InitializeComponent();
            SaveButton.IsEnabled = IsFormFilled();

            this.Loaded += SeriesForm_Loaded;
        }

        private void SeriesForm_Loaded(object sender, RoutedEventArgs e)
        {
            Name.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Series series)
            {
                Name.Text = series.Name;
                temp = series;
            }

            base.OnNavigatedTo(e);
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Adding new series.
            if (temp == null)
            {
                // Save series details to dababase.
                DataAccess.AddSeries(Name.Text);
            }
            else
            {
                DataAccess.UpdateSeries(temp.Name, Name.Text);
            }

            // Return to previous page.
            this.Frame.GoBack();
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsFormFilled();
        }

        bool IsFormFilled()
        {
            bool ret = false;

            if (!String.IsNullOrEmpty(Name.Text))
            {
                ret = true;
            }

            return ret;
        }
    }
}
