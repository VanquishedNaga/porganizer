using System;
using System.Collections.Generic;
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

namespace Porganizer
{
    public sealed partial class FormEditFile : Page
    {
        VideoFile videoFile;
        List<Performer> performerList = new List<Performer>();
        List<Series> seriesList = new List<Series>();

        public FormEditFile()
        {
            this.InitializeComponent();
            performerList = DataAccess.GetPerformerList().ToList();
            seriesList = DataAccess.GetSeriesList().ToList();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VideoFile file)
            {
                videoFile = file;
                FileName.Text = file.FileName;
            }

            base.OnNavigatedTo(e);
        }

        private void SaveChanges(object sender, TappedRoutedEventArgs e)
        {
            if (SeriesComboBox.SelectedItem is Series series)
            {
                DataAccess.UpdateFile(videoFile.FileId, series.SeriesId);
                App.databaseVideoFiles = DataAccess.GetVideoList();
                this.Frame.GoBack();
            }
        }
    }
}
