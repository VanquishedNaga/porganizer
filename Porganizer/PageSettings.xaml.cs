using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class PageSettings : Page
    {
        StorageItemAccessList futureAccessList = StorageApplicationPermissions.FutureAccessList;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public PageSettings()
        {
            this.InitializeComponent();
            LoadPerformerImageFolderAsync();
        }

        private async void LoadPerformerImageFolderAsync()
        {
            if (localSettings.Values["PerformerImageFolder"] is string token)
            {
                var tempFolder = await futureAccessList.GetItemAsync(token);
                PerformerImageFolderName.Text = tempFolder.Path;
            }
        }

        private async void ChooseFolderButton_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder selectedFolder = await folderPicker.PickSingleFolderAsync();
            if (selectedFolder != null)
            {
                // Save folder in MRU.
                string futureAccessToken = futureAccessList.Add(selectedFolder);
                localSettings.Values["PerformerImageFolder"] = futureAccessToken;
                PerformerImageFolderName.Text = selectedFolder.Path;
            }
        }
    }
}
