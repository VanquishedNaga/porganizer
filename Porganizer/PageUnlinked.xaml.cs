using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Porganizer
{
    public sealed partial class PageUnlinked : Page
    {
        ObservableCollection<VideoFile> unlinkedFileList = new ObservableCollection<VideoFile>();

        public PageUnlinked()
        {
            this.InitializeComponent();
            LoadUnlinkedFiles();
        }

        void DeleteAllUnlinkedFiles(object sender, RoutedEventArgs e)
        {
            foreach (VideoFile videoFile in unlinkedFileList)
            {
                DataAccess.DeleteFile(videoFile.FileId);
            }

            App.databaseVideoFiles = DataAccess.GetVideoList();
            LoadUnlinkedFiles();
        }

        void LoadUnlinkedFiles()
        {
            unlinkedFileList.Clear();

            foreach (VideoFile videoFile in App.databaseVideoFiles)
            {
                if (videoFile.IsUnlinked())
                {
                    unlinkedFileList.Add(videoFile);
                }
            }

            TextFileNum.Text = unlinkedFileList.Count + " files";
        }
    }
}
