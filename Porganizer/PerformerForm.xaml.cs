using DataAccessLibrary;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class PerformerForm : Page
    {
        public PerformerForm()
        {
            this.InitializeComponent();
            SaveButton.IsEnabled = IsFormFilled();
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Save performer details to dababase.
            DataAccess.AddPerformer(Name.Text, BirthDatePicker.SelectedDate, Ethnicity.SelectedItem.ToString());

            // Return to previous page.
            this.Frame.GoBack();
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsFormFilled();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = IsFormFilled();
        }

        bool IsFormFilled()
        {
            bool ret = false;

            if (!String.IsNullOrEmpty(Name.Text) && Ethnicity.SelectedItem != null)
            {
                ret = true;
            }

            return ret;
        }
    }
}
