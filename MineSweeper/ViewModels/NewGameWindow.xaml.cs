using System.Windows;

namespace Minesweeper
{
    public partial class NewGameWindow : Window
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int Mines { get; private set; }

        public NewGameWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowsInput.Text, out int rows) &&
                int.TryParse(ColumnsInput.Text, out int cols) &&
                int.TryParse(MinesInput.Text, out int mines) &&
                30 >= cols
                && 30 >= rows
                && 30 >= mines)
            {
                Rows = rows;
                Columns = cols;
                Mines = mines;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Számot adj meg, maximum 30-ig!", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}