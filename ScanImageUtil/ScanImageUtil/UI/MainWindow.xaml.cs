using Microsoft.Win32;
using ScanImageUtil.Back;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using System.Diagnostics;
using System.ComponentModel;
using ScanImageUtil.UI;

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
        private List<FileStatusLine> fileStatusLines;
        private ProgressBarWindow pbw;
        private readonly ScanRecognizer ocr;

        private void ChooseScans_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.tiff)|*.png;*.jpeg;*.jpg;*.tiff",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                fileStatusLines = new List<FileStatusLine>();
                foreach (var file in openFileDialog.FileNames)
                {
                    fileStatusLines.Add(new FileStatusLine("", file));
                }
                chosedFilesView.ItemsSource = fileStatusLines.Select(statusLine => statusLine.SourceFilePath);
                if (!string.IsNullOrEmpty(excelSourceTxtBlock.Text))
                    forwardButton.Visibility = Visibility.Visible;
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Notifying the progress bar window of the current progress.
            pbw.UpdateProgress(e.ProgressPercentage);
        }

        private void OcrProcess(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var excelPath = "";
            Dispatcher.Invoke(() =>
            {
                pbw = new ProgressBarWindow(worker);
                excelPath = excelSourceTxtBlock.Text;
            });

            Dispatcher.InvokeAsync(() =>
            {
                // Disabling parent window controls while the work is being done.              
                // Launch the progress bar window using Show()                      
                pbw.ShowDialog();
            });

            ocr.Run(worker, fileStatusLines, excelPath);
            Dispatcher.Invoke(() =>
            {
                if (worker.CancellationPending)
                {
                    mainGrid.Visibility = Visibility.Hidden;
                    excelChoosingPanel.Visibility = Visibility.Visible;
                    renamedFilesView.ItemsSource = new List<FileStatusLine>();
                    return;
                }

                if (fileStatusLines.Count > 0)
                {
                    mainGrid.Visibility = Visibility.Visible;
                    excelChoosingPanel.Visibility = Visibility.Collapsed;
                    renamedFilesView.ItemsSource = fileStatusLines;
                }
            });
        }

        private void TransformImageProcess(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            Dispatcher.Invoke(() =>
            {
                pbw = new ProgressBarWindow(worker);
            });

            Dispatcher.InvokeAsync(() =>
            {
                // Disabling parent window controls while the work is being done.              
                // Launch the progress bar window using Show()      
                if (pbw != null && pbw.IsActive)
                    pbw.ShowDialog();
            });

            Dispatcher.Invoke(() =>
            {
                var formatter = new ImageTransformer(fileStatusLines, savingFolderTxtBlock.Text);
                try
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }

                    formatter.Run(worker, isResizeNeededCheckBx.IsChecked.Value, isCompressNeededCheckBx.IsChecked.Value, targetFormat.SelectedItem.ToString(),
                        Int32.Parse(resizeTxtBx.Text), Int32.Parse(qualityTxtBx.Text));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                ResetWindowState();
            });
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (fileStatusLines.Count <= 0)
            {
                MessageBox.Show("You should choose scans before saving", "No file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(excelSourceTxtBlock.Text))
            {
                MessageBox.Show("You should choose excel file before saving", "No excel file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(savingFolderTxtBlock.Text))
            {
                MessageBox.Show("You should specify saving folder", "Saving directory isn't specified", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Using background worker to asynchronously run work method.
                var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += TransformImageProcess;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            excelSourceTxtBlock.Text = "";
            targetFormat.SelectedItem = ImageFormats.Jpg.Value;
            fileStatusLines = new List<FileStatusLine>();
            chosedFilesView.ItemsSource = new List<string>();
            renamedFilesView.ItemsSource = new List<FileStatusLine>();
            excelChoosingPanel.Visibility = Visibility.Visible;
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
            if (fileStatusLines.Count <= 0)
            {
                MessageBox.Show("You should choose scans", "No images were chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(excelSourceTxtBlock.Text))
            {
                MessageBox.Show("You should choose excel file", "No excel file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Using background worker to asynchronously run work method.
                var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += OcrProcess;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
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
            fileStatusLines = new List<FileStatusLine>();
            ocr = new ScanRecognizer();
        }

        private void FileNameManualFix(object sender, RoutedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (Helper.CheckFileNameRequirements(txtBox.Text))
            {
                fileStatusLines.Where(item => item.NewFileName == txtBox.Text).FirstOrDefault().Status = RenamingStatus.OK;
            }
            else
            {
                fileStatusLines.Where(item => item.NewFileName == txtBox.Text).FirstOrDefault().Status = RenamingStatus.Failed;
            }
        }

        private void ChooseExcelSource_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel file (*.xlsx;*.xls;*.xlsm;)|*.xlsx;*.xls;*.xlsm;",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                excelSourceTxtBlock.Text = openFileDialog.FileNames[0];
                if (fileStatusLines.Count > 0)
                    forwardButton.Visibility = Visibility.Visible;
            }
        }
    }
}
