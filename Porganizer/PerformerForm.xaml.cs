using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Porganizer
{
    public sealed partial class PerformerForm : Page
    {
        private Performer temp = null;
        public PerformerForm()
        {
            this.InitializeComponent();
            SaveButton.IsEnabled = IsFormFilled();

            this.Loaded += PerformerForm_Loaded;
        }

        private void PerformerForm_Loaded(object sender, RoutedEventArgs e)
        {
            Name.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Performer performer)
            {
                Name.Text = performer.Name;
                BirthDatePicker.SelectedDate = performer.DateOfBirth;
                temp = performer;
            }

            base.OnNavigatedTo(e);
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Adding new performer.
            if (temp == null)
            {
                // Save performer details to dababase.
                DataAccess.AddPerformer(Name.Text, BirthDatePicker.SelectedDate, Ethnicity.SelectedItem.ToString());
            }
            else
            {
                DataAccess.UpdatePerformer(temp.Name, Name.Text, BirthDatePicker.SelectedDate, Ethnicity.SelectedItem.ToString());
            }

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

        private void Ethnicity_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (temp != null)
            {
                switch (temp.Ethnicity)
                {
                    case "American":
                        {
                            Ethnicity.SelectedIndex = 0;
                            break;
                        }
                    case "Japanese":
                        {
                            Ethnicity.SelectedIndex = 1;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}
