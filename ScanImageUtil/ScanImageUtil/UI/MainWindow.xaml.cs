using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ScanImageUtil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Regex numberRegex = new Regex("[^0-9]+");

        public MainWindow()
        {
            InitializeComponent();
            targetFormat.ItemsSource = new string[] { ".png", ".jpeg", ".jpg" };
            targetFormat.SelectedItem = ".jpg";
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                chosedFiles.ItemsSource = openFileDialog.FileNames;              
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CompressNeedChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
                qualityPanel.Visibility = Visibility.Visible;
            else
                qualityPanel.Visibility = Visibility.Hidden;
        }

        private void QualityPercentageChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (sender as TextBox);
            var isCorrectNumber = !numberRegex.IsMatch(txtBox.Text);
           
            if (!isCorrectNumber || Int32.Parse(txtBox.Text) > 100 || Int32.Parse(txtBox.Text) < 0)
            {            
                MessageBox.Show("Quality percentage should be only a number(0-100)", "Wrong input!", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBox.Text = "50"; // default value
            }
        }
    }
}
