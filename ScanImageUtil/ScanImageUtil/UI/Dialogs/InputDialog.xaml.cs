using System.Windows;

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
            DialogResult = true;
            this.Close();
        }        
    }
}
