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
using ScanImageUtil.UI.Dialogs;
using ScanImageUtil.Back.Models;
using System.Threading.Tasks;
using System.Threading;

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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Regex numberRegex;
        private const string defaultQualityPercentage = "50";
        private const string defaultResizePercentage = "75";
        private List<FileStatusLine> fileStatusLines;
        private IList<ExcelRowDataModel> googleSheetData;
        private GoogleSheetsDbReader googleSheetReader;
        private ProgressBarWindow pbw;
        private readonly ScanRecognizer ocr;
        private string googleSheetId;



        private string ExcelFilePath { get; set; }

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
                if (!string.IsNullOrEmpty(ExcelFilePath))
                {
                    forwardButton.Visibility = Visibility.Visible;
                }
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
            try
            {
                if (googleSheetReader != null)
                {
                    googleSheetData = googleSheetReader.ReadAllData(worker);
                    ocr.Run(worker, fileStatusLines, 80);
                    foreach (var line in fileStatusLines)
                    {
                        if (line.Status != RenamingStatus.Failed && googleSheetData.Where(item => item.SerialNumber == line.SerialNumber).Count() <= 0)
                            line.Status = RenamingStatus.Warned;
                    }
                }
                else
                {
                    ocr.Run(worker, fileStatusLines);
                }                
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                worker.ReportProgress(100);
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (worker.CancellationPending)
                {
                    mainGrid.Visibility = Visibility.Hidden;
                    renamedFilesView.ItemsSource = new List<FileStatusLine>();
                    return;
                }

                if (fileStatusLines.Count > 0)
                {
                    mainGrid.Visibility = Visibility.Visible;
                    renamedFilesView.ItemsSource = fileStatusLines;
                    forwardButton.Visibility = Visibility.Collapsed;
                    chosedFilesView.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void SaveImagesAndExcelProcess(object sender, DoWorkEventArgs e)
        {
            try
            {
                var worker = sender as BackgroundWorker;
                var arguments = e.Argument as object[];
                var lines = arguments[0] as List<FileStatusLine>;
                var formatter = new ImageTransformer(lines, arguments[1].ToString());
                if (worker.CancellationPending)
                {
                    return;
                }
                formatter.Run(worker, (bool)arguments[2], (bool)arguments[3], arguments[4].ToString(),
                    Int32.Parse(arguments[5].ToString()), Int32.Parse(arguments[6].ToString()));
                if (worker.CancellationPending)
                {
                    return;
                }
                SaveDataToExcel(worker, arguments[7] as IList<ExcelRowDataModel>, lines, arguments[8].ToString());               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDataToExcel(BackgroundWorker worker, IList<ExcelRowDataModel> googleData, List<FileStatusLine> lines, string excelPath, double allProgress = 25)
        {
            if (googleData != null && googleData.Count > 0)
            {
                using (var excelWriter = new ExcelScanWriter(excelPath))
                {
                    excelWriter.WriteScanDataToExcel(lines, googleData, worker, allProgress);
                }
            }
            else
            {
                using (var excelWriter = new ExcelScanWriter(excelPath))
                {
                    excelWriter.WriteScanDataToExcel(lines, worker, allProgress);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {           
            if (fileStatusLines.Count <= 0)
            {
                MessageBox.Show("You should choose scans before saving", "No file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(ExcelFilePath))
            {
                MessageBox.Show("You should choose excel file before saving", "No excel file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(savingFolderRun.Text))
            {
                MessageBox.Show("You should specify saving folder", "Saving directory isn't specified", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                List<FileStatusLine> linesCopy = new List<FileStatusLine>(fileStatusLines); ;
                var arguments = new object[9];
                arguments[0] = linesCopy;
                arguments[1] = savingFolderRun.Text;
                arguments[2] = isResizeNeededCheckBx.IsChecked.Value;
                arguments[3] = isCompressNeededCheckBx.IsChecked.Value;
                arguments[4] = targetFormat.SelectedItem.ToString();
                arguments[5] = Int32.Parse(resizeTxtBx.Text);
                arguments[6] = Int32.Parse(qualityTxtBx.Text);
                arguments[7] = googleSheetData;
                arguments[8] = ExcelFilePath;
                // Using background worker to asynchronously run work method.
                using (var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                })
                {
                    pbw = new ProgressBarWindow(worker);                                     
                    worker.DoWork += SaveImagesAndExcelProcess;
                    worker.ProgressChanged += Worker_ProgressChanged;
                    worker.RunWorkerAsync(arguments);
                    pbw.ShowDialog();
                }
                ResetWindowState();
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
            chosedFilesView.Visibility = Visibility.Visible;
            isCompressNeededCheckBx.IsChecked = false;
            isResizeNeededCheckBx.IsChecked = false;
            resizeTxtBx.Text = "75";
            qualityTxtBx.Text = "50";
            targetFormat.SelectedItem = ImageFormats.Jpg.Value;
            fileStatusLines = new List<FileStatusLine>();
            chosedFilesView.ItemsSource = new List<string>();
            renamedFilesView.ItemsSource = new List<FileStatusLine>();
            //googleSheetId = "";
            //ExcelFilePath = "";
            //excelFileRun.Text = "";
            //googleSheetIdRun.Text = "";
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
            if (string.IsNullOrEmpty(ExcelFilePath))
            {
                MessageBox.Show("You should choose excel file", "No excel file was chosen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Using background worker to asynchronously run work method.
                using (var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                })
                {
                    pbw = new ProgressBarWindow(worker);                                              
                    worker.DoWork += OcrProcess;
                    worker.ProgressChanged += Worker_ProgressChanged;
                    worker.RunWorkerAsync();
                    pbw.ShowDialog();
                }
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
                savingFolderRun.Text = openFileDialog.SelectedPath;
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
            if (Helper.CheckFileNameRequirements(txtBox.Text, false))
            {
                if(googleSheetData != null && googleSheetData.Where(item => item.SerialNumber == txtBox.Text.Split('_')[0]).Count() <= 0)
                    fileStatusLines.Where(item => item.NewFileName == txtBox.Text).FirstOrDefault().Status = RenamingStatus.Warned;
                else
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
                ExcelFilePath = openFileDialog.FileNames[0];
                excelFileRun.Text = ExcelFilePath;
                if (fileStatusLines.Count > 0)
                {
                    forwardButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void ResetWindowState_Click(object sender, RoutedEventArgs e)
        {
            ResetWindowState();
        }

        private void EnterGoogleSheetId_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialog("Google sheet id");
            var res = inputDialog.ShowDialog();
            if(!res.Value || string.IsNullOrEmpty(inputDialog.Input))           
                return;
            try
            {                            
                googleSheetReader = new GoogleSheetsDbReader(inputDialog.Input);
                googleSheetReader.Check();
                googleSheetId = inputDialog.Input;
                googleSheetIdRun.Text = googleSheetId;
            }
            catch(Google.GoogleApiException ex)
            {
                googleSheetReader = null;
                var message = "";
                foreach (var error in ex.Error.Errors)
                    message += error.Message + Environment.NewLine;
                MessageBox.Show(message, "Google sheet error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
