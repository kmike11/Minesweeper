using System.Collections.Generic;
using System.Windows;

namespace Minesweeper;

public partial class LeaderboardWindow : Window
{
    public LeaderboardWindow(List<PlayerResult> results) 
    {
            InitializeComponent();
            LeaderboardGrid.ItemsSource = results;
    }
}
