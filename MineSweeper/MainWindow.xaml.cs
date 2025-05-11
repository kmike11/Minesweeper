using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper;

    public partial class MainWindow : Window
    {
        private bool[,] isMine;
        private Button[,] buttons;
        private Stopwatch stopwatch;
        private DispatcherTimer timer;
        private bool isGameOver = false;
        private int totalMines;
        private int flagsPlaced;
        private List<PlayerResult> leaderboard = new List<PlayerResult>();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartNewGame_Click(object sender, RoutedEventArgs e)
        {
            var newGameWindow = new NewGameWindow();
            if (newGameWindow.ShowDialog() == true)
            {
                int rows = newGameWindow.Rows;
                int cols = newGameWindow.Columns;
                int mines = newGameWindow.Mines;

                StartMenu.Visibility = Visibility.Collapsed;
                GameArea.Visibility = Visibility.Visible;
                TimerBorder.Visibility = Visibility.Visible;
                TimeDisplay.Text = "Idő: 0";

                StartGame(rows, cols, mines);
            }
        }

        private void Leaderboard_Click(object sender, RoutedEventArgs e)
        {
            if (leaderboard.Count == 0)
            {
                MessageBox.Show("Még nincs rekord a leaderboardon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string message = "🏆 Leaderboard:\n\n";
            int rank = 1;
            foreach (var entry in leaderboard)
            {
                message += $"{rank}. {entry.Name} - {entry.Time} mp\n";
                rank++;
            }
            var leaderboardWindow = new LeaderboardWindow(leaderboard);
            leaderboardWindow.ShowDialog();
        }

        private void CellButton_LeftClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ValueTuple<int, int> pos)
            {
                int row = pos.Item1;
                int col = pos.Item2;

                if (isMine[row, col])
                {
                    button.Content = "💣";

                    RevealAllCells();

                    stopwatch.Stop();
                    timer.Stop();

                    MessageBox.Show("Game Over!", "Vesztettél", MessageBoxButton.OK, MessageBoxImage.Warning);

                    BackToMenu();
                }
                else
                {
                    RevealCell(row, col);
                }
            }
        }

        private void RevealCell(int row, int col)
        {
            if (row < 0 || row >= isMine.GetLength(0) ||
                col < 0 || col >= isMine.GetLength(1) ||
                buttons[row, col].IsEnabled == false)
                return;

            int adjacentMines = CountAdjacentMines(row, col);
            buttons[row, col].Content = adjacentMines > 0 ? adjacentMines.ToString() : "";
            buttons[row, col].IsEnabled = false;
            buttons[row, col].Background = Brushes.White;

            if (adjacentMines == 0)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr != 0 || dc != 0)
                            RevealCell(row + dr, col + dc);
                    }
                }
            }

            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            for (int r = 0; r < isMine.GetLength(0); r++)
            {
                for (int c = 0; c < isMine.GetLength(1); c++)
                {
                    if (!isMine[r, c] && buttons[r, c].IsEnabled)
                    {
                        return;
                    }
                }
            }
            EndGame();
        }

        private void EndGame()
        {
            if (isGameOver) return;
            isGameOver = true;

            stopwatch.Stop();
            timer.Stop();

            // Felfedjük az összes aknát
            for (int r = 0; r < isMine.GetLength(0); r++)
            {
                for (int c = 0; c < isMine.GetLength(1); c++)
                {
                    if (isMine[r, c])
                    {
                        buttons[r, c].Content = "💣";
                        buttons[r, c].IsEnabled = false;
                    }
                }
            }

            // Név bekérése
            string playerName = Microsoft.VisualBasic.Interaction.InputBox(
                "Gratulálok, nyertél! 🎉\nÍrd be a neved:", 
                "Név bekérése", 
                "Játékos");

            if (!string.IsNullOrWhiteSpace(playerName))
            {
                SaveScore(playerName, stopwatch.Elapsed.Seconds);
            }

            // Menübe visszadobás
            GameArea.Visibility = Visibility.Collapsed;
            StartMenu.Visibility = Visibility.Visible;

            //Játékmező törlése
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();
        }

        private void SaveScore(string name, int time)
        {
            leaderboard.Add(new PlayerResult { Name = name, Time = time });
            leaderboard = leaderboard.OrderBy(e => e.Time).Take(10).ToList(); // csak a 10 legjobb
        }
        
        private int CountAdjacentMines(int row, int col)
        {
            int count = 0;
            int[] dr = { -1, 0, 1 };
            int[] dc = { -1, 0, 1 };

            foreach (var rDiff in dr)
            {
                foreach (var cDiff in dc)
                {
                    if (rDiff == 0 && cDiff == 0) continue;

                    int newRow = row + rDiff;
                    int newCol = col + cDiff;

                    if (newRow >= 0 && newRow < isMine.GetLength(0) &&
                        newCol >= 0 && newCol < isMine.GetLength(1))
                    {
                        if (isMine[newRow, newCol])
                            count++;
                    }
                }
            }

            return count;
        }

        private void CellButton_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Content as string == "🚩")
                {
                    button.Content = null;
                    flagsPlaced--;
                }
                else
                {
                    button.Content = "🚩";
                    flagsPlaced++;
                }
                UpdateMineCounterDisplay();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeDisplay.Text = $"Idő: {stopwatch.Elapsed.Seconds}";
        }

        private void StartGame(int rows, int cols, int mines)
        {
            isGameOver = false;
            
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            stopwatch = new Stopwatch();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            stopwatch.Start();

            totalMines = mines;
            flagsPlaced = 0;
            UpdateMineCounterDisplay();
            
            isMine = new bool[rows, cols];
            buttons = new Button[rows, cols];

            for (int r = 0; r < rows; r++)
                GameGrid.RowDefinitions.Add(new RowDefinition());

            for (int c = 0; c < cols; c++)
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Random rand = new Random();
            int minesPlaced = 0;
            while (minesPlaced < mines)
            {
                int r = rand.Next(rows);
                int c = rand.Next(cols);
                if (!isMine[r, c])
                {
                    isMine[r, c] = true;
                    minesPlaced++;
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Button cellButton = new Button
                    {
                        Margin = new Thickness(1),
                        Background = Brushes.LightGray,
                        Tag = (r, c)
                    };
                    cellButton.Click += CellButton_LeftClick;
                    cellButton.MouseRightButtonUp += CellButton_RightClick;

                    Grid.SetRow(cellButton, r);
                    Grid.SetColumn(cellButton, c);
                    GameGrid.Children.Add(cellButton);

                    buttons[r, c] = cellButton;
                }
            }
        }
        
        private void BackToMenu()
        {
            GameArea.Visibility = Visibility.Collapsed;
            StartMenu.Visibility = Visibility.Visible;
            TimerBorder.Visibility = Visibility.Collapsed;

            stopwatch?.Stop();
            timer?.Stop();
        }
        
        private void RevealAllCells()
        {
            for (int r = 0; r < isMine.GetLength(0); r++)
            {
                for (int c = 0; c < isMine.GetLength(1); c++)
                {
                    if (buttons[r, c].IsEnabled)
                    {
                        if (isMine[r, c])
                        {
                            buttons[r, c].Content = "💣";
                            buttons[r, c].Background = Brushes.LightCoral;
                        }
                        else
                        {
                            int adjacent = CountAdjacentMines(r, c);
                            buttons[r, c].Content = adjacent > 0 ? adjacent.ToString() : "";
                            buttons[r, c].Background = Brushes.White;
                        }

                        buttons[r, c].IsEnabled = false;
                    }
                }
            }
        }
        
        private void UpdateMineCounterDisplay()
        {
            MineCounterDisplay.Text = $"Aknák: {totalMines} | Megjelölve: {flagsPlaced}";
        }
    }
