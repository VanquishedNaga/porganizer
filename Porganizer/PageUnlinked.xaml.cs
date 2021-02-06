using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

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
                if (videoFile.isUnlinked())
                {
                    unlinkedFileList.Add(videoFile);
                }
            }

            TextFileNum.Text = unlinkedFileList.Count + " files";
        }
    }
}
