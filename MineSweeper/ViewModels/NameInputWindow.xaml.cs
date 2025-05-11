using System.Windows;

namespace Minesweeper.Views
{
    public partial class NameInputWindow : Window
    {
        public string PlayerName { get; private set; }
        public NameInputWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            PlayerName = string.IsNullOrWhiteSpace(NameBox.Text) ? "Player" : NameBox.Text;
            DialogResult = true;
            Close();
        }
    }
}