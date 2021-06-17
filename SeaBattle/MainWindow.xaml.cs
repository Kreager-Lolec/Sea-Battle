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
        int sessionnumber = 0;
        Button[][] userButtons;
        Button[][] enemyButtons;
        Button[][] availableShipButton;
        const int mapsize = 11;
        int cellsize = 30;
        string namecell = "ABCDEFGHIJ";
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                GameField userField = new GameField();
                GameField enemyField = new GameField();
                GameField availableShipField = new GameField();
                userButtons = CreateUIField();
                enemyButtons = CreateUIField();
                availableShipButton = CreateUIField();
                Init();
                //MessageBox.Show(field.ships[0].locations[0].cell + " " + field.ships[0].locations[0].row);
                userField.DisplayField(userField.LocationsActivity(), ref userButtons);
                enemyField.DisplayField(enemyField.LocationsActivity(), ref enemyButtons);
                availableShipField.DisplayField(availableShipField.LocationsActivity(), ref availableShipButton);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        public class Location//Class, which save position of one cell of ship
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
        public class GameField//Class, which save information about state of field
        {
            public List<Ship> ships = new List<Ship>();
            public GameField()//Default contructor
            {
                Ship s = new Ship();
                s.locations = new List<Location>();
                Location l = new Location();
                l.row = 0;
                l.cell = 0;
                s.locations.Add(l);
                ships.Add(s);
            }
            public int[][] LocationsActivity()//Method, in which we change state of cell of ship (If cell of ship is alive or no)
            {
                int[][] fs = CreateArray(10, 10);
                foreach (Ship s in ships)
                {
                    try
                    {
                        foreach (Location sl in s.locations)
                        {
                            if (sl.shipCellAlive)
                            {
                                fs[sl.row][sl.cell] = 1;
                            }
                            else
                            {
                                fs[sl.row][sl.cell] = 2;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                return fs;
            }
            public void DisplayField(int[][] fs, ref Button[][] buttons)//According to state of cells of ships, method fills up array with numbers 1 or 2
            {
                for (int i = 0; i < fs.Length; i++)
                {
                    //MessageBox.Show(fs[i][0].ToString());
                    for (int j = 0; j < fs[i].Length; j++)
                    {
                        buttons[i][j].Content = fs[i][j].ToString();
                    }
                }
            }
        }

        public class Ship//Class, which determine size of ship
        {
            public List<Location> locations = new List<Location>();//Add info about shipcell
            public void CountCells()
            {
                int cellCounter = 0;
                foreach (Location l in locations)
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
            CreateMap();
        }
        public void RefreshField()
        {

        }
        public Button[][] CreateUIField(int rows = mapsize - 1, int cells = mapsize - 1)//Creating array of buttons (Need to connect this array with panels (UI))
        {
            Button[][] buttons = new Button[rows][];
            for (int i = 0; i < rows; i++)
                buttons[i] = new Button[cells];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cells; j++)
                    buttons[i][j] = new Button();
            return buttons;
        }
        public void CreateMap()//Method, which creates maps
        {
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Width = cellsize;
                        button.Height = cellsize;
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                        Map.Children.Add(button);
                    }
                    else
                    {
                        userButtons[i - 1][j - 1].Width = cellsize;
                        userButtons[i - 1][j - 1].Height = cellsize;
                        userButtons[i - 1][j - 1].Background = Brushes.LightGray;
                        userButtons[i - 1][j - 1].Tag = i.ToString() + j.ToString();
                        userButtons[i - 1][j - 1].Click += AddUserShip;
                        Map.Children.Add(userButtons[i - 1][j - 1]);
                        //userButtons[i - 1][j - 1] = button;
                        //button.Background = Brushes.LightGray;
                    }
                }
            }
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Width = cellsize;
                        button.Height = cellsize;
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                        EnemyMap.Children.Add(button);
                    }
                    else
                    {
                        enemyButtons[i - 1][j - 1].Width = cellsize;
                        enemyButtons[i - 1][j - 1].Height = cellsize;
                        enemyButtons[i - 1][j - 1].Background = Brushes.LightGray;
                        EnemyMap.Children.Add(enemyButtons[i - 1][j - 1]);
                        //enemyButtons[i - 1][j - 1] = button;
                        //enemyButtons[i - 1][j - 1].Click += AddShip;
                    }
                }
            }
            for (int i = 0; i < mapsize; i++)
            {
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Width = cellsize;
                        button.Height = cellsize;
                        button.Background = new SolidColorBrush(Colors.Gray);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                        }
                        Shipmap.Children.Add(button);
                    }
                    else
                    {
                        availableShipButton[i - 1][j - 1] = button;
                        availableShipButton[i - 1][j - 1].Width = cellsize;
                        availableShipButton[i - 1][j - 1].Height = cellsize;
                        availableShipButton[i - 1][j - 1].Background = Brushes.LightGray;
                        Shipmap.Children.Add(availableShipButton[i - 1][j - 1]);
                        //button.Background = Brushes.LightGray;
                        ////availableShipButton[i - 1][j - 1].Click += AddShip;
                    }
                }
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            sessionnumber++;
            EnemyMap.Visibility = Visibility.Visible;
            Shipmap.Visibility = Visibility.Hidden;
        }

        public void AddUserShip(object sender, RoutedEventArgs e)//This method is used for checking what button has pressed, and then color of this button will changed
                                                                 //to red, so it will cell of current ship
        {
            Button pressedButton = sender as Button;
            for (int i = 0; i < userButtons.Length; i++)
            {
                for (int j = 0; j < userButtons[i].Length; j++)
                {
                    if (pressedButton.Tag == userButtons[i][j].Tag)
                    {
                        pressedButton = userButtons[i][j];
                        break;
                    }
                }
            }
            if (sessionnumber == 0)
            {
                if (pressedButton.Background == Brushes.LightGray)
                {
                    pressedButton.Background = Brushes.Red;
                }
                else
                {
                    pressedButton.Background = Brushes.LightGray;
                }
            }
        }
    }
}
