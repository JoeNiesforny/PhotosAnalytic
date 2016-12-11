using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotosAnalytic
{
    public partial class MainWindow : Window
    {
        AnalyzePhotos Analytic;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void loadImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.DefaultExt = ".jpg";
            dlg.ValidateNames = false;
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = true;
            dlg.FileName = "Folder Selection";
            bool? result = dlg.ShowDialog();
            if (result != true)
            {
                MessageBox.Show("No picture where choose!" , "Bad Picture format!");
                return;
            }
            Analytic = new AnalyzePhotos(GetAllFilesFromRecursivly(dlg.FileName.Replace("Folder Selection","")));
            onePictureExifValuesGrid.ItemsSource = Analytic.Result();
        }

        string[] GetAllFilesFromRecursivly(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                string[] newFiles = GetAllFilesFromRecursivly(subdirectory);
                string[] temp = fileEntries;
                fileEntries = new string[temp.Count() + newFiles.Count()];
                for (int i = 0; i < temp.Count() + newFiles.Count(); i++)
                    if (i < temp.Count())
                        fileEntries[i] = temp[i];
                    else
                        fileEntries[i] = newFiles[i - temp.Count()];
            }
            return fileEntries;
        }

    }
}
