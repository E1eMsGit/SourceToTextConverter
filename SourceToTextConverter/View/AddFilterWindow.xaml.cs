using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SrcToTextConverter.View
{
    public partial class AddFilterWindow : Window
    {
        public AddFilterWindow()
        {
            InitializeComponent();
        }

        private void CodePagePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
