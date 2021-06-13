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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeaBattle
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int mapsize = 11;
        int cellsize = 30;
        string namecell = "ABCDEFGHIJ";
        int[,] map = new int[mapsize, mapsize];
        int[,] enemymap = new int[mapsize, mapsize];
        int[,] shipmap = new int[mapsize, mapsize];
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }
        public static int[][] CreateArray(int rows, int cells)
        {
            int[][] array = new int[rows][];
            for (int i = 0; i < rows; i++)
                array[i] = new int[cells];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cells; j++)
                    array[i][j] = 0;
            return array;
        }

        public class Location
        {
            public int row = -1;
            public int cell = -1;
            public bool shipCellAlive = false;
            public void SetLocation(int row, int cell)
            {
                this.row = row;
                this.cell = cell;
            }
            public void ResetLocation()
            {
                row = -1;
                cell = -1;
            }
        }
        public class GameField
        {
            List<Ship> ships = new List<Ship>();
            public int[][] FieldState()
            {
                int[][] fs = CreateArray(10, 10);
                foreach (Ship s in ships)
                {
                    foreach  (Location sl in s.location)
                    {
                        
                    }
                }
            }
        }

        public class Ship
        {
            public List<Location> location = new List<Location>();//Add info about shipcell
            public void CountCells()
            {
                int cellCounter = 0;
                foreach (Location l in location)
                {
                    if (l != default)
                    {
                        cellCounter++;
                    }
                }
            }
        }
        public void Init()
        {
            try
            {
                CreateMap();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
        public void RefreshField()
        {

        }
        public void CreateMap()
        {
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    map[i, j] = 0;
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j-1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                    }
                    button.Width = cellsize;
                    button.Height = cellsize;
                    Map.Children.Add(button);
                }
            }
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    enemymap[i, j] = 0;
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                    }
                    button.Width = cellsize;
                    button.Height = cellsize;
                    EnemyMap.Children.Add(button);
                }
            }
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    shipmap[i, j] = 0;
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                    }
                    button.Width = cellsize;
                    button.Height = cellsize;
                    Shipmap.Children.Add(button);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            EnemyMap.Visibility = Visibility.Visible;
            Shipmap.Visibility = Visibility.Hidden;
        }
    }
}
