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
    /// Відслідковувати розмір корабля в класі GameField та коректно відображати його на полі (Добавляти координати кораблів в методі CheckPartition)!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public partial class MainWindow : Window
    {
        GameField userField = new GameField();
        GameField enemyField = new GameField();
        GameField availableShipField = new GameField();
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
                userButtons = CreateUIField();
                enemyButtons = CreateUIField();
                availableShipButton = CreateUIField();
                Init();
                //MessageBox.Show(field.ships[0].locations[0].cell + " " + field.ships[0].locations[0].row);
                userField.DisplayField(userField.LocationsActivity(), ref userButtons);
                enemyField.DisplayField(enemyField.LocationsActivity(), ref enemyButtons);
                availableShipField.DisplayField(availableShipField.LocationsActivity(), ref availableShipButton);
                ShowAvailableShip();
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
            public Location()
            {

            }
            public Location(int row, int cell)
            {
                this.row = row;
                this.cell = cell;
                shipCellAlive = true;
            }
            public void SetLocation(int row, int cell)
            {
                this.row = row;
                this.cell = cell;
                shipCellAlive = true;
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
            int fourcellship = 1;
            int threeCellships = 2;
            int twoCellShips = 3;
            int oneCellShips = 4;
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

            public void AddCellShip(int row, int checkCell, int currentCell, ref int typeOfShip)
            {
                bool breakOperand = false;
                foreach (var s in ships)
                {
                    foreach (var l in s.locations)
                    {
                        MessageBox.Show(l.row.ToString() + " " + l.cell.ToString());
                        if (l.row == row && l.cell == checkCell)
                        {
                            if (s.CountCells() == 3 && fourcellship == 1)
                            {
                                typeOfShip = 4;
                                fourcellship--;
                                threeCellships++;
                            }
                            else if (s.CountCells() == 2 && threeCellships > 0 && threeCellships <= 2)
                            {
                                typeOfShip = 3;
                                threeCellships--;
                                twoCellShips++;
                            }
                            else if (s.CountCells() == 1 && twoCellShips > 0 && twoCellShips <= 3)
                            {
                                typeOfShip = 2;
                                twoCellShips--;
                                oneCellShips++;
                            }
                            else if (s.CountCells() == 0 && oneCellShips > 0 && oneCellShips <= 4)
                            {
                                typeOfShip = 1;
                                oneCellShips--;
                            }
                            else
                            {
                                breakOperand = true;
                                break;
                            }
                            s.locations.Add(new Location(row, currentCell));
                            MessageBox.Show($"Added cell with index: {row} and {currentCell} and State of cell {l.shipCellAlive} ");
                            breakOperand = true;
                            break;
                        }
                    }
                    if (breakOperand)
                    {
                        break;
                    }
                }
            }
            public void AddRowShip(int checkRow, int cell, int currentRow, ref int typeOfShip)
            {
                bool breakOperand = false;
                foreach (var s in ships)
                {
                    foreach (var l in s.locations)
                    {
                        if (l.row == checkRow && l.cell == cell)
                        {
                            if (s.CountCells() == 3 && fourcellship == 1)
                            {
                                typeOfShip = 4;
                                fourcellship--;
                                threeCellships++;
                            }
                            else if (s.CountCells() == 2 && threeCellships > 0 && threeCellships <= 2)
                            {
                                typeOfShip = 3;
                                threeCellships--;
                                twoCellShips++;
                            }
                            else if (s.CountCells() == 1 && twoCellShips > 0 && twoCellShips <= 3)
                            {
                                typeOfShip = 2;
                                twoCellShips--;
                                oneCellShips++;
                            }
                            else if (s.CountCells() == 0 && oneCellShips > 0 && oneCellShips <= 4)
                            {
                                typeOfShip = 1;
                                oneCellShips--;
                            }
                            else
                            {
                                breakOperand = true;
                                break;
                            }
                            s.locations.Add(new Location(currentRow, cell));
                            MessageBox.Show($"Added cell with index: {currentRow} and {l.cell} ");
                            breakOperand = true;
                            break;
                        }
                    }
                }
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
                            //else
                            //{
                            //    fs[sl.row][sl.cell] = 2;
                            //}
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
            public int CountCells()
            {
                int cellCounter = 0;
                foreach (Location l in locations)
                {
                    if (l != default)
                    {
                        cellCounter++;
                    }
                }
                return cellCounter;
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
                        //userButtons[i - 1][j - 1].Tag = i.ToString() + j.ToString();
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

        public void ShowAvailableShip()
        {
            availableShipButton[1][1].Background = Brushes.Red;
            availableShipButton[2][1].Background = Brushes.Red;
            availableShipButton[3][1].Background = Brushes.Red;
            availableShipButton[4][1].Background = Brushes.Red;
            availableShipButton[2][3].Background = Brushes.Blue;
            availableShipButton[3][3].Background = Brushes.Blue;
            availableShipButton[4][3].Background = Brushes.Blue;
            availableShipButton[2][5].Background = Brushes.Blue;
            availableShipButton[3][5].Background = Brushes.Blue;
            availableShipButton[4][5].Background = Brushes.Blue;
            availableShipButton[6][1].Background = Brushes.Purple;
            availableShipButton[7][1].Background = Brushes.Purple;
            availableShipButton[6][3].Background = Brushes.Purple;
            availableShipButton[7][3].Background = Brushes.Purple;
            availableShipButton[6][5].Background = Brushes.Purple;
            availableShipButton[7][5].Background = Brushes.Purple;
            availableShipButton[1][7].Background = Brushes.Black;
            availableShipButton[3][7].Background = Brushes.Black;
            availableShipButton[5][7].Background = Brushes.Black;
            availableShipButton[7][7].Background = Brushes.Black;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            sessionnumber++;
            EnemyMap.Visibility = Visibility.Visible;
            Shipmap.Visibility = Visibility.Hidden;
        }
        public Location GetButtonIndex(Button button)
        {
            for (int i = 0; i < userButtons.Length; i++)
            {
                for (int j = 0; j < userButtons[i].Length; j++)
                {
                    if (button == userButtons[i][j])
                    {
                        return new Location(i, j);
                    }
                }
            }
            return new Location();
        }
        public bool CheckPartition(Location l, int[][] fs, ref int typeOfShip)
        {
            for (int i = l.row; i < fs.Length; i++)
            {
                if (l.row == 9)
                {
                    break;
                }
                if (i - l.row == 3)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
                if (fs[i][l.cell] == 1)
                {
                    userField.AddRowShip(i, l.cell, l.row, ref typeOfShip);
                    return true;
                }
            }
            for (int i = l.row; i >= 0; i--)
            {
                if (l.row == 0)
                {
                    break;
                }
                if (i - l.row == 3)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
                if (fs[i][l.cell] == 1)
                {
                    userField.AddRowShip(i, l.cell, l.row, ref typeOfShip);
                    return true;
                }
            }
            for (int j = l.cell; j < fs[l.row].Length; j++)
            {
                if (l.cell == 9)
                {
                    break;
                }
                if (j - l.cell == 3)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
                if (fs[l.row][j] == 1)
                {
                    userField.AddCellShip(l.row, j, l.cell, ref typeOfShip);
                    return true;
                }
            }
            for (int j = l.cell; j >= 0; j--)
            {
                if (l.cell == 0)
                {
                    break;
                }
                if (j - l.cell == 3)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
                MessageBox.Show(fs[l.row][j].ToString());
                if (fs[l.row][j] == 1)
                {
                    userField.AddCellShip(l.row, j, l.cell, ref typeOfShip);
                    return true;
                }
            }
            return false;
        }

        public void AddUserShip(object sender, RoutedEventArgs e)//This method is used for checking what button has pressed, and then color of this button will changed
                                                                 //to red, so it will cell of current ship
        {
            try
            {
                int typeOfShip = 0;
                Button pressedButton = sender as Button;
                for (int i = 0; i < userButtons.Length; i++)
                {
                    for (int j = 0; j < userButtons[i].Length; j++)
                    {
                        if (pressedButton == userButtons[i][j])
                        {
                            Location l = new Location();
                            l.SetLocation(i, j);
                            if (CheckPartition(l, userField.LocationsActivity(), ref typeOfShip))
                            {
                                if (typeOfShip == 4)
                                {
                                    pressedButton.Background = Brushes.Red;
                                    pressedButton.Content = "1";
                                }
                                else if (typeOfShip == 3)
                                {
                                    pressedButton.Background = Brushes.Blue;
                                    pressedButton.Content = "1";
                                }
                                else if (typeOfShip == 2)
                                {
                                    pressedButton.Background = Brushes.Purple;
                                    pressedButton.Content = "1";
                                }
                                else if (typeOfShip == 1)
                                {
                                    pressedButton.Background = Brushes.Black;
                                    pressedButton.Content = "1";
                                }
                                break;
                            }
                            else
                            {
                                Ship s = new Ship();
                                Location l1 = new Location();
                                l1.SetLocation(i, j);
                                s.locations.Add(l1);
                                userField.ships.Add(s);
                                pressedButton.Background = Brushes.Black;
                                pressedButton.Content = "1";
                                break;
                            }
                        }
                    }
                    if (pressedButton.Content.ToString() == "1")
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }
    }
}
