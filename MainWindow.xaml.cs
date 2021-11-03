using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace SeaBattle
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        static GameField userField = new GameField();
        static GameField enemyField = new GameField();
        static GameField availableShipField = new GameField();
        static int sessionnumber = 0;
        static bool deleteMode = false;
        static Button[][] userButtons;
        static Button[][] enemyButtons;
        static Button[][] availableShipButton;
        static Button stopDM = new Button();
        const int mapsize = 11;
        int cellsize = 30;
        string namecell = "ABCDEFGHIJ";
        static private string logPath = "log.txt";
        /// <summary>
        /// Method, which write information to the log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="st"></param>
        /// <param name="ex"></param>
        /// <param name="tip"></param>
        static void WriteToLog(string message, StackTrace st = null, Exception ex = null, string tip = "")
        {
            try { if (!File.Exists(logPath)) File.Create(logPath).Close(); } catch { WriteToLog("Can't reach log file.", new StackTrace()); }
            File.AppendAllText(logPath, "[" + DateTime.Now + "] ");
            if (st != null) File.AppendAllText(logPath, st.GetFrame(0).GetMethod().Name + "()");
            File.AppendAllText(logPath, "-> " + message.TrimEnd('.') + ".");
            if (ex != null) File.AppendAllText(logPath, " (" + ex.Message.Replace(Environment.NewLine, " ") + ") ");
            if (tip.Length > 0) File.AppendAllText(logPath, "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText(logPath, Environment.NewLine);
        }
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                userButtons = CreateUIField();
                enemyButtons = CreateUIField();
                availableShipButton = CreateUIField();
                Init();
                File.Create(logPath).Close();
                userField.DisplayField(userField.LocationsActivity(), ref userButtons);
                enemyField.DisplayField(enemyField.LocationsActivity(), ref enemyButtons);
                availableShipField.DisplayField(availableShipField.LocationsActivity(), ref availableShipButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// The method, which create array for the fields
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Class, which save position of one cell of ship
        /// </summary>
        public class Location
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
            const string logPath = "log.txt";
            void WriteToLog(string message, StackTrace st = null, Exception ex = null, string tip = "")
            {
                try { if (!File.Exists(logPath)) File.Create(logPath).Close(); } catch { WriteToLog("Can't reach log file.", new StackTrace()); }
                File.AppendAllText(logPath, "[" + DateTime.Now + "] ");
                if (st != null) File.AppendAllText(logPath, st.GetFrame(0).GetMethod().Name + "()");
                File.AppendAllText(logPath, "-> " + message.TrimEnd('.') + ".");
                if (ex != null) File.AppendAllText(logPath, " (" + ex.Message.Replace(Environment.NewLine, " ") + ") ");
                if (tip.Length > 0) File.AppendAllText(logPath, "[Tip: " + tip.TrimEnd('.') + "]");
                File.AppendAllText(logPath, Environment.NewLine);
            }
            public List<Ship> ships = new List<Ship>();
            public int shipsCount = 0;
            public int fourcellship = 0;
            public int threeCellships = 0;
            public int twoCellShips = 0;
            public int oneCellShips = 0;

            public void SortShipsCoordinate()
            {
                foreach (var s in ships)
                {
                    s.SortLocs();
                }
            }
            public Ship SearchShipG(int row, int cell)
            {
                foreach (var ship in userField.ships)
                {
                    if (ship.SearchShip(row, cell))
                    {
                        return ship;
                    }
                }
                return null;
            }
            public bool DoesExistShip(int row, int cell)
            {
                foreach (var ship in userField.ships)
                {
                    if (ship.SearchShip(row, cell))
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// This method return ship to the map of the available ships
            /// </summary>
            /// <param name="length"></param>
            public void ReturnAvs(int length)
            {
                Ship ship = new Ship();
                if (length == 4)
                {
                    ship.locations.Add(new Location(1, 1));
                    ship.locations.Add(new Location(2, 1));
                    ship.locations.Add(new Location(3, 1));
                    ship.locations.Add(new Location(4, 1));
                    ships.Add(ship);
                    foreach (var loc in ship.locations)
                    {
                        availableShipButton[loc.row][loc.cell].Content = "1";
                        availableShipButton[loc.row][loc.cell].Background = Brushes.Red;
                    }
                }
                else if (length == 3)
                {
                    //if ()
                    //{

                    //}
                }
                else if (length == 2)
                {

                }
                else if (length == 1)
                {

                }

            }
            /// <summary>
            /// Method find coincidence in adress of activated cell and button. According to the type of the ship, program reset location of chosen button, and unificate all buttons of the current ship
            /// </summary>
            /// <param name="row"></param>
            /// <param name="cell"></param>
            /// <param name="buttons"></param>
            //public void ResetLocation(int row, int cell)
            //{
            //    foreach (var l in SearchShipG(row, cell).locations)
            //    {
            //        userButtons[l.row][l.cell].Content = "0";
            //        userButtons[l.row][l.cell].Background = Brushes.LightGray;
            //    }
            //    ReturnAvs(SearchShipG(row, cell).CountCells());
            //    ships.Remove(SearchShipG(row, cell));
            //}
            public void ResetLocation(int row, int cell, Button[][] buttons)
            {
                bool breakOperand = false;
                bool createNewShip = false;
                if (ships.Count > 0)
                {
                    foreach (var ship in ships)
                    {
                        foreach (var location in ship.locations)
                        {
                            if (row == location.row && cell == location.cell)
                            {
                                breakOperand = true;
                                if (ship.CountCells() == 4)
                                {
                                    location.shipCellAlive = false;
                                    int whatShipAdd = 1;//This value is used to check in what order ships would be created.
                                    int countEntireShip = 0;
                                    Ship s1 = new Ship();
                                    Ship s2 = new Ship();
                                    foreach (var loc in ship.locations)
                                    {
                                        //MessageBox.Show(loc.row + " " + loc.cell);
                                        //MessageBox.Show(" State of cell: " + loc.shipCellAlive + " Size: " + s1.locations.Count);
                                        if (loc.shipCellAlive)
                                        {
                                            if (!createNewShip)
                                            {
                                                AddPartShipRes(s1, loc, buttons);
                                            }
                                            else if (createNewShip)
                                            {
                                                AddPartShipRes(s2, loc, buttons);
                                            }
                                            countEntireShip++;
                                        }
                                        else if (!loc.shipCellAlive && countEntireShip < 3)
                                        {
                                            countEntireShip = 0;
                                            createNewShip = true;
                                            ships.Add(s2);
                                            continue;
                                        }
                                    }
                                    ships.Add(s1);
                                    ships.Remove(ship);
                                    break;
                                }
                                else if (ship.CountCells() == 3)
                                {
                                    Ship s3 = new Ship();
                                    Ship s4 = new Ship();
                                    location.shipCellAlive = false;
                                    createNewShip = false;
                                    int countEntireShip = 0;
                                    foreach (var loc in ship.locations)
                                    {
                                        //MessageBox.Show(loc.row + " " + loc.cell);
                                        //MessageBox.Show(" State of cell: " + loc.shipCellAlive + " Size: " + s3.locations.Count);
                                        if (loc.shipCellAlive)
                                        {
                                            if (!createNewShip)
                                            {
                                                AddPartShipRes(s3, loc, buttons);
                                            }
                                            else if (createNewShip)
                                            {
                                                AddPartShipRes(s4, loc, buttons);
                                            }
                                            countEntireShip++;
                                        }
                                        else if (!loc.shipCellAlive && countEntireShip < 2)
                                        {
                                            createNewShip = true;
                                            ships.Add(s4);
                                            continue;
                                        }
                                    }
                                    ships.Remove(ship);
                                    ships.Add(s3);
                                    break;
                                }
                                else if (ship.CountCells() == 2)
                                {
                                    //twoCellShips++;
                                    //oneCellShips--;
                                    ship.locations.Remove(location);
                                    break;
                                }
                                else if (ship.CountCells() == 1)
                                {
                                    //oneCellShips++;
                                    ships.Remove(ship);
                                    break;
                                }
                            }
                        }
                        if (breakOperand)
                        {
                            break;
                        }
                    }
                }
            }
            /// <summary>
            /// Method gets object of class Ship. According to the type of the ship, program unificate all buttons of the current ship
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="typeOfShip"></param>
            /// <param name="userButtons"></param>
            public void ChooseShip(ref Button[][] userButtons, Ship ship, int OCS = 0, int TCS = 0, int THCS = 0)
            {
                if (ship.CountCells() == 4)
                {
                    foreach (var l in ship.locations)
                    {
                        userButtons[l.row][l.cell].Background = Brushes.Red;
                    }
                }
                else if (ship.CountCells() == 3)
                {
                    if (THCS <= 2)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Blue;
                        }
                    }
                    else if (THCS > 2)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Red;
                        }
                    }
                }
                else if (ship.CountCells() == 2)
                {
                    if (TCS > 3)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Blue;
                        }
                    }
                    else if (TCS <= 3)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Purple;
                        }
                    }
                    else if (THCS >= 2)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Red;
                        }
                    }
                }
                else if (ship.CountCells() == 1)
                {
                    if (OCS <= 4)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Black;
                        }
                    }
                    else if (OCS > 4)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Purple;
                        }
                    }
                    else if (TCS >= 3)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Blue;
                        }
                    }
                    else if (THCS >= 2)
                    {
                        foreach (var l in ship.locations)
                        {
                            userButtons[l.row][l.cell].Background = Brushes.Red;
                        }
                    }
                }
            }
            public GameField() { }//Default constructor
            /// <summary>
            /// Method construct horizontal ships. According to the count of cells, program add one type of ship and delete another one
            /// </summary>
            /// <param name="row"></param>
            /// <param name="checkCell"></param>
            /// <param name="currentCell"></param>
            /// <param name="typeOfShip"></param>
            public void AddCellShip(int row, int checkCell, int currentCell)
            {
                bool breakOperand = false;
                foreach (var s in ships)
                {
                    foreach (var l in s.locations)
                    {
                        if (l.row == row && l.cell == checkCell)
                        {
                            if (fourcellship < 1)
                            {
                                if (s.CountCells() == 1 && twoCellShips == 3 && threeCellships == 2 && oneCellShips >= 4)
                                {
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    userButtons[row][currentCell].Content = "1";
                                    break;
                                }
                            }
                            if (fourcellship >= 1 && s.CountCells() == 4) { break; }
                            else if (fourcellship < 1)
                            {
                                if (s.CountCells() == 3 || s.CountCells() == 2)
                                {
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    userButtons[row][currentCell].Content = "1";
                                }
                            }
                            if (threeCellships >= 2 && s.CountCells() == 3) { break; }
                            else if (threeCellships < 2)
                            {
                                if (s.CountCells() == 2 || s.CountCells() == 1)
                                {
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    userButtons[row][currentCell].Content = "1";
                                }
                            }
                            if (twoCellShips >= 3 && s.CountCells() == 2)
                            {
                                break;
                            }
                            else if (twoCellShips < 3 && s.CountCells() == 1)
                            {
                                s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                userButtons[row][currentCell].Content = "1";
                            }
                            if (oneCellShips >= 4 && s.CountCells() == 1)
                            {
                                break;
                            }
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
            /// <summary>
            /// Method construct vertical ships. According to the count of cells, program add one type of ship and delete another one
            /// </summary>
            /// <param name="checkRow"></param>
            /// <param name="cell"></param>
            /// <param name="currentRow"></param>
            /// <param name="typeOfShip"></param>
            public void AddRowShip(int checkRow, int cell, int currentRow)
            {
                bool breakOperand = false;
                foreach (var s in ships)
                {
                    foreach (var l in s.locations)
                    {
                        if (l.row == checkRow && l.cell == cell)
                        {
                            //MessageBox.Show(fourcellship.ToString());
                            //userField.checkAllShip(userField.LocationsActivity(), userButtons);
                            if (fourcellship < 1)
                            {
                                if (s.CountCells() == 1 && twoCellShips == 3 && threeCellships == 2 && oneCellShips >= 4)
                                {
                                    s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                    userButtons[currentRow][cell].Content = "1";
                                    break;
                                }
                            }
                            if (fourcellship >= 1 && s.CountCells() == 4)
                            {
                                break;
                            }
                            else if (fourcellship < 1)
                            {
                                if (s.CountCells() == 3 || s.CountCells() == 2)
                                {
                                    s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                    userButtons[currentRow][cell].Content = "1";
                                }
                            }
                            if (threeCellships >= 2 && s.CountCells() == 3)
                            {
                                breakOperand = true;
                                break;
                            }
                            else if (threeCellships < 2)
                            {
                                if (s.CountCells() == 2 || s.CountCells() == 1)
                                {
                                    s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                    userButtons[currentRow][cell].Content = "1";
                                }
                            }
                            if (twoCellShips >= 3 && s.CountCells() == 2)
                            {
                                breakOperand = true;
                                break;
                            }
                            else if (twoCellShips < 3 && s.CountCells() == 1)
                            {
                                s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                userButtons[currentRow][cell].Content = "1";
                            }
                            breakOperand = true;
                            break;
                        }
                    }
                    if (breakOperand)
                    {
                        //MessageBox.Show(fourcellship.ToString());
                        break;
                    }
                }
            }
            /// <summary>
            /// Method that give count of all ships int the current moment, and give this data to the Method ChooseShip
            /// </summary>
            /// <param name="field"></param>
            /// <param name="userButtons"></param>
            public void checkAllShip(Button[][] buttons)
            {
                int OCS = 0;
                int TCS = 0;
                int THCS = 0;
                int FCS = 0;
                for (int i = 0; i < buttons.Length; i++)
                {
                    for (int j = 0; j < buttons[i].Length; j++)
                    {
                        if (DoesExistShip(i, j))
                        {
                            continue;
                        }
                        else
                        {
                            buttons[i][j].Content = "0";
                            buttons[i][j].Background = Brushes.LightGray;
                        }
                    }
                }
                foreach (var ship in ships)
                {
                    if (ship.CountCells() == 1)
                    {
                        OCS++;
                    }
                    else if (ship.CountCells() == 2)
                    {
                        TCS++;
                    }
                    else if (ship.CountCells() == 3)
                    {
                        THCS++;
                    }
                    else if (ship.CountCells() == 4)
                    {
                        FCS++;
                    }
                    userField.ChooseShip(ref buttons, ship, OCS, TCS, THCS);
                }
                //MessageBox.Show("Four: " + FCS + " Three: " + THCS + " Two: " + TCS + " One: " + OCS);
                fourcellship = FCS;
                threeCellships = THCS;
                twoCellShips = TCS;
                oneCellShips = OCS;
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
            string logPath = "log.txt";
            public Ship()
            {

            }
            public Ship(List<Location> locs)
            {
                locations = locs;
            }
            void WriteToLog(string message, StackTrace st = null, Exception ex = null, string tip = "")
            {
                try { if (!File.Exists(logPath)) File.Create(logPath).Close(); } catch { WriteToLog("Can't reach log file.", new StackTrace()); }
                File.AppendAllText(logPath, "[" + DateTime.Now + "] ");
                if (st != null) File.AppendAllText(logPath, st.GetFrame(0).GetMethod().Name + "()");
                File.AppendAllText(logPath, "-> " + message.TrimEnd('.') + ".");
                if (ex != null) File.AppendAllText(logPath, " (" + ex.Message.Replace(Environment.NewLine, " ") + ") ");
                if (tip.Length > 0) File.AppendAllText(logPath, "[Tip: " + tip.TrimEnd('.') + "]");
                File.AppendAllText(logPath, Environment.NewLine);
            }
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
            public bool SearchShip(int row, int cell)
            {
                foreach (Location l in locations)
                {
                    if (l.row == row && l.cell == cell)
                    {
                        WriteToLog("Searched row: " + l.row + " Searched cell: " + l.cell);
                        return true;
                    }
                }
                return false;
            }
            public void SortLocs()
            {
                int temprow = 0;
                int tempcell = 0;
                for (int i = 0; i < locations.Count; i++)
                {
                    for (int j = i; j < locations.Count; j++)
                    {
                        if (locations[i].row > locations[j].row)
                        {
                            temprow = locations[i].row;
                            locations[i].row = locations[j].row;
                            locations[j].row = temprow;
                        }
                        if (locations[i].cell > locations[j].cell)
                        {
                            tempcell = locations[i].row;
                            locations[i].row = locations[j].row;
                            locations[j].row = tempcell;
                        }
                    }
                }
            }
        }
        public void Init()
        {
            stopDM.Width = 150;
            stopDM.Height = 32;
            stopDM.Content = "Stop the deleting mode";
            stopDM.Visibility = Visibility.Hidden;
            stopDM.Click += StopDelete;
            ButtonMap.Children.Add(stopDM);
            CreateMap();
            ShowAvailableShip();
        }
        /// <summary>
        /// This method delete all ships, and return map to the default view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearField(object sender, RoutedEventArgs e)
        {
            userField.ships.Clear();
            for (int i = 0; i < userButtons.Length; i++)
            {
                for (int j = 0; j < userButtons[i].Length; j++)
                {
                    userButtons[i][j].Background = Brushes.LightGray;
                    userButtons[i][j].Content = "0";
                }
            }
        }
        /// <summary>
        /// This method deactivate deleting mode, and return color of the ships to the normal view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopDelete(object sender, RoutedEventArgs e)
        {
            deleteMode = false;
            userField.checkAllShip(userButtons);
            stopDM.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// This method activate deleting mode, and brush color of all ships to another
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteMode(object sender, RoutedEventArgs e)
        {
            deleteMode = true;
            stopDM.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Creating array of buttons (Need to connect this array with panels (UI))
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        public Button[][] CreateUIField(int rows = mapsize - 1, int cells = mapsize - 1)
        {
            Button[][] buttons = new Button[rows][];
            for (int i = 0; i < rows; i++)
                buttons[i] = new Button[cells];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cells; j++)
                    buttons[i][j] = new Button();
            return buttons;
        }
        /// <summary>
        /// The method, which creates the maps
        /// </summary>
        public void CreateMap()
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
                        userButtons[i - 1][j - 1].Click += AddUserShip;
                        Map.Children.Add(userButtons[i - 1][j - 1]);
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
                    }
                }
            }
        }

        public void ShowAvailableShip()
        {
            for (int i = 0; i < availableShipButton.Length; i++)
            {
                availableShipField.ships.Add(new Ship());
            }
            availableShipField.ships[0].locations.Add(new Location(1, 1));
            availableShipField.ships[0].locations.Add(new Location(2, 1));
            availableShipField.ships[0].locations.Add(new Location(3, 1));
            availableShipField.ships[0].locations.Add(new Location(4, 1));
            availableShipField.ships[1].locations.Add(new Location(2, 3));
            availableShipField.ships[1].locations.Add(new Location(3, 3));
            availableShipField.ships[1].locations.Add(new Location(4, 3));
            availableShipField.ships[2].locations.Add(new Location(2, 5));
            availableShipField.ships[2].locations.Add(new Location(3, 5));
            availableShipField.ships[2].locations.Add(new Location(4, 5));
            availableShipField.ships[3].locations.Add(new Location(6, 1));
            availableShipField.ships[3].locations.Add(new Location(7, 1));
            availableShipField.ships[4].locations.Add(new Location(6, 3));
            availableShipField.ships[4].locations.Add(new Location(7, 3));
            availableShipField.ships[5].locations.Add(new Location(6, 5));
            availableShipField.ships[5].locations.Add(new Location(7, 5));
            availableShipField.ships[6].locations.Add(new Location(1, 7));
            availableShipField.ships[7].locations.Add(new Location(3, 7));
            availableShipField.ships[8].locations.Add(new Location(5, 7));
            availableShipField.ships[9].locations.Add(new Location(7, 7));
            availableShipButton[1][1].Content = "1";
            availableShipButton[2][1].Content = "1";
            availableShipButton[3][1].Content = "1";
            availableShipButton[4][1].Content = "1";
            availableShipButton[2][3].Content = "1";
            availableShipButton[3][3].Content = "1";
            availableShipButton[4][3].Content = "1";
            availableShipButton[2][5].Content = "1";
            availableShipButton[3][5].Content = "1";
            availableShipButton[4][5].Content = "1";
            availableShipButton[6][1].Content = "1";
            availableShipButton[7][1].Content = "1";
            availableShipButton[6][3].Content = "1";
            availableShipButton[7][3].Content = "1";
            availableShipButton[6][5].Content = "1";
            availableShipButton[7][5].Content = "1";
            availableShipButton[1][7].Content = "1";
            availableShipButton[3][7].Content = "1";
            availableShipButton[5][7].Content = "1";
            availableShipButton[7][7].Content = "1";
            availableShipField.checkAllShip(availableShipButton);
        }
        public void RemoveAvS(Ship ship)
        {
            foreach (var loc in ship.locations)
            {
                availableShipButton[loc.row][loc.cell].Content = "0";
                availableShipButton[loc.row][loc.cell].Background = Brushes.LightGray;
            }
            availableShipField.ships.Remove(ship);
        }

        public void RegulateAvS()
        {
            userField.checkAllShip(userButtons);
            //availableShipField.fourcellship = userField.fourcellship;
            //availableShipField.threeCellships = userField.threeCellships;
            //availableShipField.twoCellShips = userField.twoCellShips;
            //availableShipField.oneCellShips = userField.oneCellShips;
            if (deleteMode)
            {

            }
            foreach (var ship in availableShipField.ships)
            {
                if (ship.CountCells() == 4)
                {
                    if (userField.fourcellship == 1)
                    {
                        RemoveAvS(ship);
                        break;
                    }
                }
                else if (ship.CountCells() == 3)
                {
                    if (userField.threeCellships > 0 && userField.threeCellships <= 2)
                    {
                        RemoveAvS(ship);
                        break;
                    }
                }
                else if (ship.CountCells() == 2)
                {
                    if (userField.twoCellShips > 0 && userField.twoCellShips <= 3)
                    {
                        RemoveAvS(ship);
                        break;
                    }
                }
                else if (ship.CountCells() == 1)
                {
                    if (userField.oneCellShips > 0 && userField.oneCellShips <= 4)
                    {
                        RemoveAvS(ship);
                        break;
                    }
                }
            }
            availableShipField.checkAllShip(availableShipButton);
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
        /// <summary>
        /// Method check activated cells in all directions in a distance of one cell
        /// </summary>
        /// <param name="l"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static bool CheckNearbyShip(Location l, int[][] fs, bool mode = false)
        {
            for (int i = l.row; i < fs.Length; i++)
            {
                if (l.row == 9)
                {
                    break;
                }
                if (fs[i][l.cell] == 1)//Checking activated cells in the next row
                {
                    //MessageBox.Show("Hello suka, you catch gay panic 2V.1");
                    return false;
                }
                if (i - l.row == 1)//Program check only the next row.  If we check with bigger division, it will conflict with other ships
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
            }
            for (int i = l.row; i >= 0; i--)
            {
                if (l.row == 0)
                {
                    break;
                }
                if (fs[i][l.cell] == 1)//Checking activated cells in the previous row
                {
                    //MessageBox.Show("Hello suka, you catch gay panic 2V.2");
                    return false;
                }
                if (l.row - i == 1)//Program check only the previous row.  If we check with bigger division, it will conflict with other ships
                {
                    //MessageBox.Show("Hello suka, you catch gay panic");
                    break;
                }
            }
            for (int j = l.cell; j < fs[l.row].Length; j++)
            {
                if (l.cell == 9)
                {
                    break;
                }
                if (fs[l.row][j] == 1)//Checking activated button(cell) in the next cell
                {
                    //MessageBox.Show("Hello suka, you catch gay panic 2V.3");
                    return false;
                }
                if (j - l.cell == 1)//Program check only the next cell.  If we check with bigger division, it will conflict with other ships
                {
                    //MessageBox.Show("Hello suka, you catch gay panic #1");
                    break;
                }
            }
            for (int j = l.cell; j >= 0; j--)
            {
                if (l.cell == 0)
                {
                    break;
                }
                if (fs[l.row][j] == 1)//Checking activated button(cell) in the previous cell
                {
                    //MessageBox.Show("Hello suka, you catch gay panic 2V.4");
                    return false;
                }
                if (l.cell - j == 1)//Program check only the previous cell.  If we check with bigger division, it will conflict with other ships
                {
                    //MessageBox.Show("Hello suka, you catch gay panic 2V.4");
                    break;
                }
                //MessageBox.Show(fs[l.row][j].ToString());
            }
            return true;
        }
        /// <summary>
        /// Method check activated cells in four existing directions, so four diagonals in a distance of one cell
        /// </summary>
        /// <param name="l"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public bool CheckDiagonalCell(Location l, int[][] fs)
        {
            for (int i = l.row; i < fs.Length; i++)
            {
                if (i - l.row == 2)//Cell will be checked only at a distance of one cell, and then loop will be stopped
                {
                    break;
                }
                for (int j = l.cell; j < fs[i].Length; j++)
                {
                    if (l.row == 9 || l.cell == 9)
                    {
                        break;
                    }
                    if (fs[l.row + 1][l.cell + 1] == 1)//Checking activated button(cell) in the diagonal (lower right)
                    {
                        //MessageBox.Show("Hello suka, you catch gay panic DV.1");
                        return false;
                    }
                }
            }
            for (int i = l.row; i < fs.Length; i++)
            {
                if (i - l.row == 2)//Cell will be checked only at a distance of one cell, and then loop will be stopped
                {
                    break;
                }
                for (int j = l.cell; j > 0; j--)
                {
                    if (l.row == 9 || l.cell == 0)
                    {
                        break;
                    }
                    if (fs[l.row + 1][l.cell - 1] == 1)//Checking activated button(cell) in the diagonal (lower left)
                    {
                        //MessageBox.Show("Hello suka, you catch gay panic DV.2");
                        return false;
                    }
                }
            }
            for (int i = l.row; i > 0; i--)
            {
                if (l.row - i == 2)//Cell will be checked only at a distance of one cell, and then loop will be stopped
                {
                    break;
                }
                for (int j = l.cell; j > 0; j--)
                {
                    if (l.row == 0 || l.cell == 0)
                    {
                        break;
                    }
                    if (fs[l.row - 1][l.cell - 1] == 1)
                    {
                        //MessageBox.Show("Hello suka, you catch gay panic DV.3");//Checking activated button(cell) in the diagonal (upper left)
                        return false;
                    }
                }
            }
            for (int i = l.row; i > 0; i--)
            {
                if (l.row - i == 2)//Cell will be checked only at a distance of one cell, and then loop will be stopped
                {
                    break;
                }
                for (int j = 0; j < fs[i].Length; j++)
                {
                    if (l.row == 0 || l.cell == 9)
                    {
                        break;
                    }
                    if (fs[l.row - 1][l.cell + 1] == 1)//Checking activated button(cell) in the diagonal (upper right)
                    {
                        //MessageBox.Show("Hello suka, you catch gay panic DV.4");
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// This method check activated cells nearby, and add them to the current ship
        /// </summary>
        /// <param name="l"></param>
        /// <param name="fs"></param>
        /// <param name="typeOfShip"></param>
        /// <returns></returns>
        public static bool CheckPartition(Location l, int[][] fs)
        {
            Ship temps = new Ship();
            for (int i = l.row; i < fs.Length; i++)
            {
                if (l.row == 9)
                {
                    break;
                }
                if (fs[i][l.cell] == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic V.1");
                    userField.AddRowShip(i, l.cell, l.row);//Add cell to the existing vertical ship
                    for (int k = l.row; k >= 0; k--)
                    {
                        if (fs[k][l.cell] == 1)
                        {
                            foreach (var s in userField.ships)
                            {
                                if (s.SearchShip(k, l.cell))
                                {
                                    temps = s;
                                    foreach (var loc in s.locations)
                                    {
                                        WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                        userField.AddRowShip(l.row, l.cell, loc.row);
                                    }
                                    break;
                                }
                            }
                            userField.ships.Remove(temps);
                        }
                        if (l.row - k == 1)
                        {
                            break;
                        }
                    }
                    return true;
                }
                if (i - l.row == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic #1");
                    break;
                }
            }
            for (int i = l.row; i >= 0; i--)
            {
                if (l.row == 0)
                {
                    break;
                }
                if (fs[i][l.cell] == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic V.2");
                    userField.AddRowShip(i, l.cell, l.row);//Add cell to the existing vertical ship
                    for (int k = l.row; k < fs.Length; k++)
                    {
                        if (fs[k][l.cell] == 1)
                        {
                            foreach (var s in userField.ships)
                            {
                                if (s.SearchShip(k, l.cell))
                                {
                                    temps = s;
                                    foreach (var loc in s.locations)
                                    {
                                        WriteToLog("K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                        userField.AddRowShip(l.row, l.cell, loc.row);
                                    }
                                    break;
                                }
                            }
                            userField.ships.Remove(temps);
                        }
                        if (k - l.row == 1)
                        {
                            break;
                        }
                    }
                    return true;
                }
                if (l.row - i == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic #2");
                    break;
                }
            }
            for (int j = l.cell; j < fs[l.row].Length; j++)
            {
                if (l.cell == 9)
                {
                    break;
                }
                if (fs[l.row][j] == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic V.3");
                    userField.AddCellShip(l.row, j, l.cell);//Add cell to the existing horizontal ship
                    for (int k = l.cell; k >= 0; k--)
                    {
                        if (fs[l.row][k] == 1)
                        {
                            foreach (var s in userField.ships)
                            {
                                if (s.SearchShip(l.row, k))
                                {
                                    temps = s;
                                    foreach (var loc in s.locations)
                                    {
                                        WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                        userField.AddCellShip(l.row, l.cell, loc.cell);
                                    }
                                    break;
                                }
                            }
                            userField.ships.Remove(temps);
                        }
                        if (l.cell - k == 1)
                        {
                            break;
                        }
                    }
                    return true;
                }
                if (j - l.cell == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic #3");
                    break;
                }
            }
            for (int j = l.cell; j >= 0; j--)
            {
                if (l.cell == 0)
                {
                    break;
                }
                if (fs[l.row][j] == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic V.4");
                    userField.AddCellShip(l.row, j, l.cell);//Add cell to the existing horizontal ship
                    for (int k = l.cell; k < fs.Length; k++)
                    {
                        if (fs[l.row][k] == 1)
                        {
                            foreach (var s in userField.ships)
                            {
                                if (s.SearchShip(l.row, k))
                                {
                                    temps = s;
                                    foreach (var loc in s.locations)
                                    {
                                        WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                        userField.AddCellShip(l.row, l.cell, loc.cell);
                                    }
                                    break;
                                }
                            }
                            userField.ships.Remove(temps);
                        }
                        if (k - l.cell == 1)
                        {
                            break;
                        }
                    }
                    return true;
                }
                if (l.cell - j == 1)
                {
                    //MessageBox.Show("Hello suka, you catch gay panic #4");
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// This method add new ship while reseting the another one
        /// </summary>
        public static void AddShipRES(List<Ship> ships, Location loc, Ship s, Button[][] buttons)
        {
            Location l = new Location();
            l.SetLocation(loc.row, loc.cell);
            s.locations.Add(l);
            ships.Add(s);
        }
        /// <summary>
        ///  This method add cells to the existing ships, which have been created in the method AddShipRES
        /// </summary>
        public static void AddPartShipRes(Ship s, Location loc, Button[][] buttons)
        {
            s.locations.Add(loc);
        }
        /// <summary>
        /// This method is used for checking what button has pressed, then we check if button has been activated or no. If button is not activated, program will count active ships.
        /// If all ships are not picked, it will be created object, which consist of adress of button. Then program will check cells nearby. If activated cells are not espied, new ship will be created as a object. If not, program
        /// will check cells, and add this addres to current ship
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddUserShip(object sender, RoutedEventArgs e)
        {
            try
            {
                Button pressedButton = sender as Button;
                bool breakOperand = false;
                for (int i = 0; i < userButtons.Length; i++)
                {
                    for (int j = 0; j < userButtons[i].Length; j++)
                    {
                        if (pressedButton == userButtons[i][j])
                        {
                            breakOperand = true;
                            //MessageBox.Show(userField.GetTwoCellShip().ToString());
                            if (pressedButton.Background != Brushes.LightGray)
                            {
                                if (deleteMode)
                                {
                                    userField.ResetLocation(i, j, userButtons);//If button has been activated, the method will reset location, and delete adress from ship's list
                                }
                                else
                                    MessageBox.Show("Activate deleting mode using the button: Delete ship ");
                                break;
                            }
                            else if (pressedButton.Background == Brushes.LightGray && deleteMode)
                            {
                                MessageBox.Show("Choose only activated cell to remove the ships please.");
                                break;
                            }
                            else if (userField.ships.Count >= 0 && userField.ships.Count <= 10)
                            {
                                Location l = new Location();
                                l.SetLocation(i, j);
                                if (CheckNearbyShip(l, userField.LocationsActivity()) && CheckDiagonalCell(l, userField.LocationsActivity()) && userField.ships.Count < 10)//Check cells nearby
                                {
                                    Ship s = new Ship();
                                    Location l1 = new Location();
                                    l1.SetLocation(i, j);
                                    s.locations.Add(l1);
                                    userField.ships.Add(s);
                                    userButtons[i][j].Content = "1";
                                    break;
                                }
                                else if (CheckDiagonalCell(l, userField.LocationsActivity()) && CheckPartition(l, userField.LocationsActivity()))//Check cells to add them to the current ship
                                    break;
                                else MessageBox.Show("You can't place ship there");
                            }
                            else MessageBox.Show("You have used all ships, Let's start"); //If all ships have been picked
                        }
                    }
                    if (breakOperand)
                    {
                        userField.checkAllShip(userButtons);
                        userField.SortShipsCoordinate();
                        //availableShipField.checkAllShip(availableShipButton);
                        //RegulateAvS();
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
