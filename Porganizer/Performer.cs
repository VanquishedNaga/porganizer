using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;

namespace Porganizer
{
    public class Performer : BindableBase
    {
        public Performer()
        {
            string profilePicPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";
            ProfilePic = new BitmapImage(new Uri(profilePicPlaceholderPath));
        }

        public Performer(string inName, string inEthnicity)
        {
            Name = inName;
            Ethnicity = inEthnicity;
            GetProfilePicAsync();
        }

        async void GetProfilePicAsync()
        {
            string profilePicPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";
            ProfilePic = new BitmapImage(new Uri(profilePicPlaceholderPath));

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["PerformerImageFolder"] is string token)
            {
                StorageItemAccessList futureAccessList = StorageApplicationPermissions.FutureAccessList;
                StorageFolder profilePicFolder = await futureAccessList.GetFolderAsync(token);

                List<string> fileTypeFilter = new List<string>
                {
                    ".jpg"
                };

                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
                {
                    ApplicationSearchFilter = "System.FileName:*\"" + Name + "\"*"
                };

                StorageFileQueryResult queryResult = profilePicFolder.CreateFileQueryWithOptions(queryOptions);

                var files = await queryResult.GetFilesAsync();
                if (files.Count > 0)
                {
                    BitmapImage screen = new BitmapImage();
                    screen.SetSource(await files[0].GetThumbnailAsync(ThumbnailMode.SingleItem));
                    ProfilePic = screen;
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        private DateTime? dateOfBirth;
        public DateTime? DateOfBirth
        {
            get { return this.dateOfBirth; }
            set { this.SetProperty(ref this.dateOfBirth, value); }
        }

        private string ethnicity;
        public string Ethnicity
        {
            get { return this.ethnicity; }
            set { this.SetProperty(ref this.ethnicity, value); }
        }

        private BitmapImage profilePic;
        public BitmapImage ProfilePic
        {
            get { return this.profilePic; }
            set { this.SetProperty(ref this.profilePic, value); }
        }
    }
}
