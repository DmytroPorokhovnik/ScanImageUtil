using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using ScanImageUtil.Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace ScanImageUtil
{
    class ImageFormats
    {
        private ImageFormats(string value) { Value = value; }

        public string Value { get; set; }

        public static ImageFormats Jpg { get { return new ImageFormats(".jpg"); } }
        public static ImageFormats Jpeg { get { return new ImageFormats(".jpeg"); } }
        public static ImageFormats Png { get { return new ImageFormats(".png"); } }
        public static ImageFormats Tiff { get { return new ImageFormats(".tiff"); } }
        public static ImageFormats Bmp { get { return new ImageFormats(".bmp"); } }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Regex numberRegex;
        private const string defaultQualityPercentage = "50";
        private const string defaultResizePercentage = "75";
        private List<string> chosedFilesList;
        private List<RenameFileStatusLine> renamedFilesStatusLines;

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.tiff)|*.png;*.jpeg;*.jpg;*.tiff",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                chosedFilesList = openFileDialog.FileNames.ToList();
                chosedFilesView.ItemsSource = chosedFilesList;
                forwardButton.Visibility = Visibility.Visible;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var formatter = new ImageTransformer(chosedFilesList);
            ResetWindowState();
            //formatter.Run(isCompressNeeded, isResizeNeeded);
            //formatter.Convert Save Compress.....         
        }

        private void CompressNeedChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
                qualityPanel.Visibility = Visibility.Visible;
            else
                qualityPanel.Visibility = Visibility.Hidden;
        }

        private void ResetWindowState()
        {
            mainGrid.Visibility = Visibility.Hidden;            
            qualityPanel.Visibility = Visibility.Hidden;          
            resizePanel.Visibility = Visibility.Hidden;
            forwardButton.Visibility = Visibility.Hidden;
            isCompressNeededCheckBx.IsChecked = false;
            isResizeNeededCheckBx.IsChecked = false;            
            resizeTxtBx.Text = "75";
            qualityTxtBx.Text = "50";
            targetFormat.SelectedItem = ImageFormats.Jpg.Value;           
            chosedFilesList = new List<string>();
            chosedFilesView.ItemsSource = chosedFilesList;
            renamedFilesStatusLines = new List<RenameFileStatusLine>();
            renamedFilesView.ItemsSource = renamedFilesStatusLines;
        }

        private void ResizeNeedChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
                resizePanel.Visibility = Visibility.Visible;
            else
                resizePanel.Visibility = Visibility.Hidden;
        }

        private void QualityPercentageChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (sender as TextBox);
            if (string.IsNullOrEmpty(txtBox.Text))
                return;
            var isCorrectNumber = !numberRegex.IsMatch(txtBox.Text);
           
            if (!isCorrectNumber || Int32.Parse(txtBox.Text) > 100 || Int32.Parse(txtBox.Text) < 0)
            {            
                MessageBox.Show("Quality percentage should be only a number(0-100)", "Wrong input!", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBox.Text = defaultQualityPercentage;
            }
        }

        private void ResizePercentageChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (sender as TextBox);
            if (string.IsNullOrEmpty(txtBox.Text))
                return;
            var isCorrectNumber = !numberRegex.IsMatch(txtBox.Text);

            if (!isCorrectNumber || Int32.Parse(txtBox.Text) > 100 || Int32.Parse(txtBox.Text) < 0)
            {
                MessageBox.Show("Resize percentage should be only a number(0-100)", "Wrong input!", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBox.Text = defaultResizePercentage;
            }
        }

        private void Resize_LostFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (sender as TextBox);
            if (string.IsNullOrEmpty(txtBox.Text))
                txtBox.Text = defaultResizePercentage;
        }

        private void Quality_LostFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (sender as TextBox);
            if (string.IsNullOrEmpty(txtBox.Text))
                txtBox.Text = defaultQualityPercentage;
        }

        private void ForwardClick(object sender, RoutedEventArgs e)
        {
            //.......
            mainGrid.Visibility = Visibility.Visible;
            renamedFilesStatusLines = new List<RenameFileStatusLine>();
            foreach (var filePath in chosedFilesList) //temprorary
            {
                renamedFilesStatusLines.Add(new RenameFileStatusLine(System.IO.Path.GetFileNameWithoutExtension(filePath),
                    filePath, RenamingStatus.OK));
            }
            renamedFilesView.ItemsSource = renamedFilesStatusLines;            
        }

        private void ChooseSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                savingFolderTxtBlock.Text = openFileDialog.SelectedPath;                
            }
        }

        private void OpenScan_Click(object sender, RoutedEventArgs e)
        {
            var filePath = (sender as Button).Tag.ToString();
            Process.Start(filePath);
        }

        public MainWindow()
        {
            numberRegex = new Regex("[^0-9]+");
            InitializeComponent();
            targetFormat.ItemsSource = new string[] { ImageFormats.Png.Value, ImageFormats.Jpeg.Value,
                ImageFormats.Jpg.Value, ImageFormats.Tiff.Value};
            targetFormat.SelectedItem = ".jpg";
            chosedFilesList = new List<string>();
            renamedFilesStatusLines = new List<RenameFileStatusLine>();
        }
    }
}
