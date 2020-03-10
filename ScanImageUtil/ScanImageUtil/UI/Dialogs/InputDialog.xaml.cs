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
using System.Windows.Shapes;

namespace ScanImageUtil.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string Input { get; set; }
        public InputDialog(string title)
        {
            InitializeComponent();
            this.Title = title;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputTxtBx.Text))
            {
                MessageBox.Show("You should enter relevant input", "Wrong or empty input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Input = inputTxtBx.Text;
            this.Close();
        }
    }
}
