using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Porganizer
{
    public static class DataAccess
    {
        // Create the database tables if they don't already exist.
        public static void InitializeDatabase()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                String tableCommand =
                    "CREATE TABLE IF NOT EXISTS SERIES (" +
                    "   SeriesId        INTEGER         PRIMARY KEY, " +
                    "   Name            NVARCHAR(50)    NOT NULL        UNIQUE" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS FILE (" +
                    "   FileId          INTEGER         PRIMARY KEY, " +
                    "   Path            NVARCHAR(2048)  NOT NULL        UNIQUE," +
                    "   ImportDate      DATE            NOT NULL," +
                    "   Size            INTEGER         NOT NULL," +
                    "   Ranking         INTEGER," +
                    "   ReleaseDate     DATE," +
                    "   SeriesId        INTEGER," +
                    "   CONSTRAINT SeriFK FOREIGN KEY (SeriesId)" +
                    "       REFERENCES SERIES (SeriesId)" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFORMER (" +
                    "   PerformerId     INTEGER         PRIMARY KEY, " +
                    "   Name            NVARCHAR(50)    NOT NULL        UNIQUE," +
                    "   DateOfBirth     DATE," +
                    "   Ranking         INTEGER," +
                    "   Ethnicity       NVARCHAR(25)    NOT NULL" +
                    "       CHECK (Ethnicity IN ('JAPANESE', 'AMERICAN'))" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFOMANCE (" +
                    "   FileId          INTEGER," +
                    "   PerformerId     INTEGER," +
                    "   CONSTRAINT FileFK FOREIGN KEY (FileId)" +
                    "       REFERENCES FILE (FileId)," +
                    "   CONSTRAINT PerfFK FOREIGN KEY (PerformerId)" +
                    "       REFERENCES PERFORMER (PerformerId)," +
                    "   UNIQUE (FileId, PerformerId)" +
                    ");";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();

                db.Close();
            }
        }

        public static void AddFile(string path, int size)
        {
            if (!IsFileInDatabase(path))
            {
                using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText =
                        "INSERT INTO FILE (Path, ImportDate, Size)" +
                        "   VALUES (@Path, CURRENT_DATE, @Size);";
                    insertCommand.Parameters.AddWithValue("@Path", path);
                    insertCommand.Parameters.AddWithValue("@Size", size);

                    insertCommand.ExecuteReader();

                    db.Close();
                }
            }
        }

        public static void RemoveFileFromDatabase(string path)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM FILE WHERE Path=\"" + path + "\"", db);
                deleteCommand.ExecuteReader();
            }
        }

        public static bool IsFileInDatabase(string path)
        {
            bool isInDatabase = false;

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                using (SqliteCommand selectCommand = new SqliteCommand("SELECT EXISTS(SELECT 1 FROM FILE WHERE Path=\"" + path + "\")", db))
                {
                    // Investigate why this does not work.
                    //selectCommand.Parameters.AddWithValue("@Path", path);

                    using (SqliteDataReader query = selectCommand.ExecuteReader())
                    {
                        query.Read();
                        isInDatabase = query.GetBoolean(0);
                    }
                }
            }

            return isInDatabase;
        }

        public static List<VideoFile> GetVideoList()
        {
            List<VideoFile> entries = new List<VideoFile>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT FileId, Path from FILE", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    VideoFile temp = new VideoFile(query.GetString(1));
                    temp.FileId = query.GetInt16(0);
                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }

        public static void AddPerformer(string name, DateTimeOffset? date, string ethnicity)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                Object tempDate = DBNull.Value;

                if (date.HasValue)
                {
                    tempDate = date.Value.Date;
                }

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText =
                    "INSERT INTO PERFORMER (Name, DateOfBirth, Ethnicity)" +
                    "   VALUES (@Name, @Date, @Ethnicity);";
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Date", tempDate);
                insertCommand.Parameters.AddWithValue("@Ethnicity", ethnicity.ToUpper());

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        public static void UpdatePerformer(string oldName, string newName, DateTimeOffset? date, string ethnicity)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;

                Object tempDate = DBNull.Value;

                if (date.HasValue)
                {
                    tempDate = date.Value.Date;
                }

                // Use parameterized query to prevent SQL injection attacks
                updateCommand.CommandText =
                    "UPDATE PERFORMER " +
                    "   SET Name = @NewName, " +
                    "       DateOfBirth = @Date, " +
                    "       Ethnicity = @Ethnicity" +
                    "   WHERE Name = @OldName;";
                updateCommand.Parameters.AddWithValue("@NewName", newName);
                updateCommand.Parameters.AddWithValue("@Date", tempDate);
                updateCommand.Parameters.AddWithValue("@Ethnicity", ethnicity.ToUpper());
                updateCommand.Parameters.AddWithValue("@OldName", oldName);

                updateCommand.ExecuteReader();

                db.Close();
            }
        }

        public static void DeletePerformer(string name)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                deleteCommand.CommandText =
                    "DELETE FROM PERFORMER " +
                    "   WHERE Name = @Name;";
                deleteCommand.Parameters.AddWithValue("@Name", name);

                deleteCommand.ExecuteReader();

                db.Close();
            }
        }

        public static ObservableCollection<Performer> GetPerformerList()
        {
            ObservableCollection <Performer> entries = new ObservableCollection<Performer>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Name, DateOfBirth, Ethnicity from PERFORMER", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    Performer temp = new Performer(query.GetString(0), textInfo.ToTitleCase(query.GetString(2).ToLower()));

                    if (!query.IsDBNull(1))
                    {
                        temp.DateOfBirth = query.GetDateTime(1);
                    }
                    else
                    {
                        temp.DateOfBirth = null;
                    }

                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }
    }

    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VideoFile : BindableBase
    {
        private static List<string> supportedVideoTypes = new List<string>
        {
            ".avi",
            ".mkv",
            ".mov",
            ".mp4",
            ".wmv",
        };

        public static bool IsSupportedVideoType(StorageFile file)
        {
            bool isVideo = false;

            if (supportedVideoTypes.Contains(file.FileType))
            {
                isVideo = true;
            }

            return isVideo;
        }

        public VideoFile()
        {
        }

        public VideoFile(StorageFile inputFile)
        {
            File = inputFile;

            GetThumbnail();
            FindScreens();
            FindGif();
        }

        public VideoFile(string filePath)
        {
            // Can't make constructor async, so need this function.
            InitFromFilePath(filePath);
        }

        private async void InitFromFilePath(string filePath)
        {
            StorageFile temp = null;
            try
            {
                temp = await StorageFile.GetFileFromPathAsync(filePath);
            }
            catch(Exception ex)
            {
            }

            if (temp != null)
            {
                File = temp;
                FileName = File.Name;

                GetThumbnail();
                FindScreens();
                FindGif();
            }
            else
            {
                string ListThumbnailPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";
                BitmapImage image = new BitmapImage(new Uri(ListThumbnailPlaceholderPath));
                FileName = Path.GetFileNameWithoutExtension(filePath);
                Thumbnail = image;
                DisplayedImage = image;
            }
        }

        private async void GetThumbnail()
        {
            BitmapImage image = new BitmapImage();

            var temp = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
            if (temp != null)
            {
                await image.SetSourceAsync(temp);
                Thumbnail = image;
                DisplayedImage = image;
            }
        }

        private async void FindScreens()
        {
            List<string> fileTypeFilter = new List<string>
            {
                ".jpg"
            };

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
            {
                ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(File.Name) + "\"*"
            };

            StorageFolder folder = await File.GetParentAsync();
            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await queryResult.GetFilesAsync();
            if (files.Count > 0)
            {
                Screen = files[0];
            }
        }

        private async void FindGif()
        {
            List<string> fileTypeFilter = new List<string>
            {
                ".gif"
            };

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
            {
                ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(File.Name) + "\"*"
            };

            StorageFolder folder = await File.GetParentAsync();
            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await queryResult.GetFilesAsync();
            if (files.Count > 0)
            {
                IRandomAccessStream fileStream = await files[0].OpenAsync(FileAccessMode.Read);
                BitmapImage image = new BitmapImage();
                image.SetSource(fileStream);
                Gif = image;
            }
        }

        private int fileId;
        public int FileId
        {
            get { return this.fileId; }
            set { this.SetProperty(ref this.fileId, value); }
        }

        private StorageFile file;
        public StorageFile File
        {
            get { return this.file; }
            set { this.SetProperty(ref this.file, value); }
        }

        private string fileName;
        public string FileName
        {
            get { return this.fileName; }
            set { this.SetProperty(ref this.fileName, value); }
        }

        private StorageFile screen;
        public StorageFile Screen
        {
            get { return this.screen; }
            set { this.SetProperty(ref this.screen, value); }
        }

        private BitmapImage thumbnail;
        public BitmapImage Thumbnail
        {
            get { return this.thumbnail; }
            set { this.SetProperty(ref this.thumbnail, value); }
        }

        private BitmapImage gif;
        public BitmapImage Gif
        {
            get { return this.gif; }
            set { this.SetProperty(ref this.gif, value); }
        }

        // Could either display a thumbnail or a GIF.
        private BitmapImage displayedImage;
        public BitmapImage DisplayedImage
        {
            get { return this.displayedImage; }
            set { this.SetProperty(ref this.displayedImage, value); }
        }

        private BitmapImage screenImage;
        public BitmapImage ScreenImage
        {
            get { return this.screenImage; }
            set { this.SetProperty(ref this.screenImage, value); }
        }
    }

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
