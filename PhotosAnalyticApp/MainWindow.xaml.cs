using System;
using System.Collections.Generic;
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
            dlg.DefaultExt = ".jpg";
            bool? result = dlg.ShowDialog();
            if (result != true)
            {
                MessageBox.Show("Dupa!" , "Bad Picture format!");
                return;
            }
            Analytic = new AnalyzePhotos(new string[] { dlg.FileName });
            onePictureExifValuesGrid.ItemsSource = Analytic.Result();
        }
    }
}
