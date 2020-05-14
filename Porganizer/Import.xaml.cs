using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Import : Page
    {
        List<string> fileTypeFilter = new List<string>
        {
            ".mp4",
            ".wmv",
            ".mkv",
            ".avi"
        };

        public ObservableCollection<VideoFile> thumbFileList = new ObservableCollection<VideoFile>();
        List<string> paths = new List<string>();

        public Import()
        {
            this.InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    foreach (StorageFile file in items)
                    {
                        // If it is video
                        if (fileTypeFilter.Contains(file.FileType))
                        {
                            if (!paths.Contains(file.Path))
                            {
                                thumbFileList.Add(new VideoFile(file));
                                paths.Add(file.Path);
                            }
                        }
                    }
                }
            }
        }

        // Display log in debug window.
        void AddLog(string log)
        {
            DebugOutput.Text = log + '\n' + DebugOutput.Text;
        }

        private void ClearList(object sender, TappedRoutedEventArgs e)
        {
            thumbFileList.Clear();
            paths.Clear();
        }
    }
}
