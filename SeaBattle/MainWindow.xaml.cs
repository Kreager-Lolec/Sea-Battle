using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

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
        static AvailableShipField availableShipField = new AvailableShipField();
        static int sessionnumber = 0;
        static bool startGame = false;
        static bool deleteMode = false;
        static Button stopDM = new Button();
        const int cellsize = 30;
        const int mapsize = 11;
        string namecell = "ABCDEFGHIJ";
        static private string logPath = "log.txt";
        static public Brush fourCellColor = Brushes.Green;
        static public Brush threeCellColor = Brushes.Blue;
        static public Brush twoCellColor = Brushes.Purple;
        static public Brush oneCellColor = Brushes.DarkOrange;
        static public Brush defaultCellColor = Brushes.LightGray;
        static public Brush usedCellColor = Brushes.DarkGray;
        static public Brush brokenCellColor = Brushes.Red;
        static public bool displayContent = true;
        DispatcherTimer dt = new DispatcherTimer();
        int seconds = 0;
        int minutes = 0;
        string defaultValueTimer = "00:00";
        Label lTimer = new Label();
        Label fTimer = new Label();
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
                userField.buttons = CreateUIField();
                enemyField.buttons = CreateUIField();
                availableShipField.buttons = CreateUIField();
                Init();
                File.Create(logPath).Close();
                userField.DisplayField(userField.LocationsActivity(), userField.buttons);
                enemyField.DisplayField(enemyField.LocationsActivity(), enemyField.buttons);
                availableShipField.DisplayField(availableShipField.LocationsActivity(), availableShipField.buttons);
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
            public Button[][] buttons;
            public List<Ship> ships = new List<Ship>();
            public int shipsCount = 0;
            public int fourcellship = 0;
            public int threeCellships = 0;
            public int twoCellShips = 0;
            public int oneCellShips = 0;
            public GameField() { }//Default constructor
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
            public List<Ship> GetShips()
            {
                return ships;
            }
            public void SortShipsCoordinate()
            {
                foreach (var s in ships)
                {
                    s.SortLocs();
                }
            }

            public int GetShipSize(int row, int cell)
            {
                foreach (var ship in ships)
                {
                    if (ship.DoesShipExist(row, cell))
                    {
                        return ship.locations.Count;
                    }
                }
                return 10;
            }
            public bool DoesExistShip(int row, int cell)
            {
                foreach (var ship in ships)
                {
                    if (ship.DoesShipExist(row, cell))
                    {
                        return true;
                    }
                }
                return false;
            }
            public Ship GetShip(int row, int cell)
            {
                foreach (var ship in ships)
                {
                    if (ship.DoesShipExist(row, cell))
                    {
                        return ship;
                    }
                }
                return null;
            }
            public bool DoesExistAllships()
            {
                if (ships.Count > 10)
                {
                    return true;
                }
                return false;
            }
            public bool DoesNotExistAllTypesOfShips()
            {
                if (fourcellship >= 1 && threeCellships >= 2 && twoCellShips >= 3 && oneCellShips >= 4)
                {
                    return false;
                }
                return true;
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
            //        userButtons[l.row][l.cell].Background = defaultCellColor;
            //    }
            //    ReturnAvs(SearchShipG(row, cell).CountCells());
            //    ships.Remove(SearchShipG(row, cell));
            //}
            public void ResetLocation(int row, int cell)
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
                                    if (s2.CountCells() == 1 && oneCellShips >= 4)
                                    {
                                        s2.brokenShip = true;
                                    }
                                    else if (s2.CountCells() == 2 && twoCellShips >= 3)
                                    {
                                        s2.brokenShip = true;
                                    }
                                    if (s1.CountCells() == 1 && oneCellShips >= 4)
                                    {
                                        s1.brokenShip = true;
                                    }
                                    else if (s1.CountCells() == 2 && twoCellShips >= 3)
                                    {
                                        s1.brokenShip = true;
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
                                            if (oneCellShips >= 2)
                                            {
                                                s4.brokenShip = true;
                                            }
                                            ships.Add(s4);
                                            continue;
                                        }
                                    }
                                    ships.Remove(ship);
                                    if (oneCellShips >= 2)
                                    {
                                        s3.brokenShip = true;
                                    }
                                    ships.Add(s3);
                                    break;
                                }
                                else if (ship.CountCells() == 2)
                                {
                                    ship.locations.Remove(location);
                                    if (oneCellShips >= 4)
                                    {
                                        ship.brokenShip = true;
                                    }
                                    break;
                                }
                                else if (ship.CountCells() == 1)
                                {
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
            public void ChooseShip(Ship ship, int OCS = 0, int TCS = 0, int THCS = 0)
            {
                if (!ship.brokenShip)
                {
                    if (ship.CountCells() == 4)
                    {
                        foreach (var l in ship.locations)
                        {
                            buttons[l.row][l.cell].Background = fourCellColor;
                        }
                    }
                    else if (ship.CountCells() == 3)
                    {
                        if (THCS <= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (THCS > 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = fourCellColor;
                            }
                        }
                    }
                    else if (ship.CountCells() == 2)
                    {
                        if (TCS > 3 && THCS < 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (TCS <= 3)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = twoCellColor;
                            }
                        }
                        else if (THCS >= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = fourCellColor;
                            }
                        }
                    }
                    else if (ship.CountCells() == 1)
                    {
                        if (OCS <= 4)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = oneCellColor;
                            }
                        }
                        else if (OCS > 4 && TCS < 3)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = twoCellColor;
                            }
                        }
                        else if (TCS >= 3 && THCS < 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (THCS >= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                buttons[l.row][l.cell].Background = fourCellColor;
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Method construct horizontal ships. According to the count of cells, program add one type of ship and delete another one
            /// </summary>
            /// <param name="row"></param>
            /// <param name="checkCell"></param>
            /// <param name="currentCell"></param>
            /// <param name="typeOfShip"></param>
            /// /// <summary>
            /// This method is used for checking what button has pressed, then we check if button has been activated or no. If button is not activated, program will count active ships.
            /// If all ships are not picked, it will be created object, which consist of adress of button. Then program will check cells nearby. If activated cells are not espied, new ship will be created as a object. If not, program
            /// will check cells, and add this addres to current ship
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void AddShip(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (sessionnumber == 0)
                    {
                        Button pressedButton = sender as Button;
                        bool breakOperand = false;
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            for (int j = 0; j < buttons[i].Length; j++)
                            {
                                if (pressedButton == buttons[i][j])
                                {
                                    breakOperand = true;
                                    //MessageBox.Show(userField.GetTwoCellShip().ToString());
                                    if (pressedButton.Background != defaultCellColor)
                                    {
                                        if (deleteMode)
                                        {
                                            ResetLocation(i, j);//If button has been activated, the method will reset location, and delete adress from ship's list
                                        }
                                        else
                                            MessageBox.Show("Activate deleting mode using the button: Delete ship.");
                                        break;
                                    }
                                    else if (pressedButton.Background == defaultCellColor && deleteMode)
                                    {
                                        MessageBox.Show("Choose only activated cell to remove the ships please.");
                                        break;
                                    }
                                    else if (DoesNotExistAllTypesOfShips())
                                    {
                                        Location l = new Location();
                                        l.SetLocation(i, j);
                                        if (CheckNearbyShip(l, LocationsActivity()) && CheckDiagonalCell(l, LocationsActivity()))//Check cells nearby
                                        {
                                            Ship s = new Ship();
                                            Location l1 = new Location();
                                            l1.SetLocation(i, j);
                                            s.locations.Add(l1);
                                            ships.Add(s);
                                            if (DoesExistAllships())
                                            {
                                                s.brokenShip = true;
                                            }
                                            buttons[i][j].Content = "1";
                                            break;
                                        }
                                        else if (CheckDiagonalCell(l, LocationsActivity()))//Check cells to add them to the current ship
                                        {
                                            bool succesufulAdding = CheckPartition(l);
                                            if (succesufulAdding)
                                            {
                                                break;
                                            }
                                            else if (!succesufulAdding)
                                            {
                                                MessageBox.Show("You can't connect the two ships into the one.");
                                            }
                                            break;
                                        }
                                        else MessageBox.Show("You can't place ship there.");
                                    }
                                    else
                                    {
                                        if (sessionnumber == 0)
                                        {
                                            MessageBox.Show("You have used all ships, Let's start.");
                                        }
                                        else if (sessionnumber == 1)
                                        {

                                        }
                                    }
                                    //If all ships have been picked
                                }
                            }
                            if (breakOperand)
                            {
                                checkAllShip();
                                SortShipsCoordinate();
                                availableShipField.countActiveCell();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw;
                }
            }
            /// <summary>
            /// This method check activated cells nearby, and add them to the current ship
            /// </summary>
            /// <param name="l"></param>
            /// <param name="fs"></param>
            /// <param name="typeOfShip"></param>
            /// <returns></returns>
            public bool CheckPartition(Location l)
            {
                int[][] fs = LocationsActivity();
                Ship temps = new Ship();
                bool addPart = false;
                for (int i = l.row; i < fs.Length; i++)
                {
                    if (l.row == 9)
                    {
                        break;
                    }
                    if (fs[i][l.cell] == 1)
                    {
                        for (int k = l.row; k >= 0; k--)
                        {
                            if (fs[k][l.cell] == 1)
                            {
                                foreach (var s in ships)
                                {
                                    if (s.DoesShipExist(k, l.cell))
                                    {

                                        if (GetShipSize(k, l.cell) < 2 && GetShipSize(i, l.cell) < 3 && fourcellship < 1 || GetShipSize(k, l.cell) < 2 &&
                                            GetShipSize(i, l.cell) < 2 && threeCellships < 2)
                                        {
                                            addPart = true;
                                            temps = s;
                                            foreach (var loc in s.locations)
                                            {
                                                WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                                AddRowShip(i, l.cell, loc.row);
                                                ships.Remove(temps);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (k != l.row && fs[k][l.cell] != 1 || k == 0)
                            {
                                addPart = true;
                            }
                            if (addPart)
                            {
                                AddRowShip(i, l.cell, l.row);//Add cell to the existing vertical ship
                                return true;
                            }
                            if (l.row - k == 1)
                            {
                                break;
                            }
                        }
                    }
                    if (i - l.row == 1)
                    {
                        break;
                    }
                    //MessageBox.Show("Hello suka, you catch gay panic #1");
                }
                for (int i = l.row; i >= 0; i--)
                {
                    if (l.row == 0)
                    {
                        break;
                    }
                    if (fs[i][l.cell] == 1)
                    {
                        for (int k = l.row; k < fs.Length; k++)
                        {
                            if (fs[k][l.cell] == 1)
                            {
                                foreach (var s in ships)
                                {
                                    if (s.DoesShipExist(k, l.cell))
                                    {
                                        if (GetShipSize(k, l.cell) < 2 && GetShipSize(i, l.cell) < 3 && fourcellship < 1 || GetShipSize(k, l.cell) < 2 &&
                                            GetShipSize(i, l.cell) < 2 && threeCellships < 2)
                                        {
                                            addPart = true;
                                            temps = s;
                                            foreach (var loc in s.locations)
                                            {
                                                WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                                AddRowShip(i, l.cell, loc.row);
                                                ships.Remove(temps);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (k != l.row && fs[k][l.cell] != 1 || k == 9)
                            {
                                addPart = true;
                            }
                            if (addPart)
                            {
                                AddRowShip(i, l.cell, l.row);//Add cell to the existing vertical ship
                                return true;
                            }
                            if (k - l.row == 1)
                            {
                                break;
                            }
                        }
                        //MessageBox.Show("Hello suka, you catch gay panic V.2");
                    }
                    if (l.row - i == 1)
                    {
                        break;
                        //MessageBox.Show("Hello suka, you catch gay panic #2");
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
                        for (int k = l.cell; k >= 0; k--)
                        {
                            if (fs[l.row][k] == 1)
                            {
                                foreach (var s in userField.ships)
                                {
                                    if (s.DoesShipExist(l.row, k))
                                    {
                                        if (GetShipSize(l.row, k) < 2 && GetShipSize(l.row, j) < 3 && fourcellship < 1 ||
                                            GetShipSize(l.row, k) < 2 && GetShipSize(l.row, j) < 2 && threeCellships < 2)
                                        {
                                            addPart = true;
                                            temps = s;
                                            foreach (var loc in s.locations)
                                            {
                                                WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                                AddCellShip(l.row, j, loc.cell);
                                                ships.Remove(temps);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (l.cell != k && fs[l.row][k] != 1 || k == 9)
                            {
                                addPart = true;
                            }
                            if (addPart)
                            {
                                AddCellShip(l.row, j, l.cell);//Add cell to the existing horizontal ship
                                return true;
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
                        for (int k = l.cell; k < fs.Length; k++)
                        {
                            if (fs[l.row][k] == 1)
                            {
                                foreach (var s in userField.ships)
                                {
                                    if (s.DoesShipExist(l.row, k))
                                    {
                                        if (GetShipSize(l.row, k) < 2 && GetShipSize(l.row, j) < 3 && fourcellship < 1 ||
                                            GetShipSize(l.row, k) < 2 && GetShipSize(l.row, j) < 2 && threeCellships < 2)
                                        {
                                            addPart = true;
                                            temps = s;
                                            foreach (var loc in s.locations)
                                            {
                                                WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
                                                AddCellShip(l.row, j, loc.cell);
                                                ships.Remove(temps);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (l.cell != k && fs[l.row][k] != 1 || k == 9)
                            {
                                addPart = true;
                            }
                            if (addPart)
                            {
                                AddCellShip(l.row, j, l.cell);//Add cell to the existing horizontal ship
                                return true;
                            }
                            if (k - l.cell == 1)
                            {
                                break;
                            }
                        }
                    }
                    if (l.cell - j == 1)
                    {
                        //MessageBox.Show("Hello suka, you catch gay panic #4");
                        break;
                    }
                }
                return false;
            }
            public void AddCellShip(int row, int checkCell, int currentCell)
            {
                bool breakOperand = false;
                foreach (var s in ships)
                {
                    foreach (var l in s.locations)
                    {
                        if (l.row == row && l.cell == checkCell && !s.brokenShip || s.brokenShip && DoesNotExistAllTypesOfShips())
                        {
                            if (s.brokenShip && DoesNotExistAllTypesOfShips())
                            {
                                s.brokenShip = false;
                            }
                            if (fourcellship < 1)
                            {
                                if (s.CountCells() == 1 && twoCellShips == 3 && threeCellships == 2 && oneCellShips >= 1)
                                {
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    buttons[row][currentCell].Content = "1";
                                    break;
                                }
                            }
                            if (fourcellship >= 1 && s.CountCells() == 4)
                            {
                                break;
                            }
                            else if (fourcellship < 1)
                            {
                                if (s.CountCells() == 3 || s.CountCells() == 2 && threeCellships < 3)
                                {
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    buttons[row][currentCell].Content = "1";
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
                                    s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                    buttons[row][currentCell].Content = "1";
                                }
                            }
                            if (twoCellShips >= 3 && s.CountCells() == 2)
                            {
                                breakOperand = true;
                                break;
                            }
                            else if (twoCellShips < 3 && s.CountCells() == 1)
                            {
                                s.locations.Add(new Location(row, currentCell));//Add cell to the existing ship
                                buttons[row][currentCell].Content = "1";
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
                        if (l.row == checkRow && l.cell == cell && !s.brokenShip || s.brokenShip && DoesNotExistAllTypesOfShips())
                        {
                            if (s.brokenShip && DoesNotExistAllTypesOfShips())
                            {
                                s.brokenShip = false;
                            }
                            //MessageBox.Show(fourcellship.ToString());
                            //userField.checkAllShip(userField.LocationsActivity(), userButtons);
                            if (fourcellship < 1)
                            {
                                if (s.CountCells() == 1 && twoCellShips == 3 && threeCellships == 2 && oneCellShips >= 1)
                                {
                                    s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                    buttons[currentRow][cell].Content = "1";
                                    break;
                                }
                            }
                            if (fourcellship >= 1 && s.CountCells() == 4)
                            {
                                break;
                            }
                            else if (fourcellship < 1)
                            {
                                if (s.CountCells() == 3 || s.CountCells() == 2 && threeCellships < 3)
                                {
                                    s.locations.Add(new Location(currentRow, cell));//Add cell to the existing ship
                                    buttons[currentRow][cell].Content = "1";
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
                                    buttons[currentRow][cell].Content = "1";
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
                                buttons[currentRow][cell].Content = "1";
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
            public void checkAllShip()
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
                            buttons[i][j].Background = defaultCellColor;
                        }
                    }
                }
                foreach (var ship in ships)
                {
                    if (!ship.brokenShip)
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
                        ChooseShip(ship, OCS, TCS, THCS);
                    }
                }
                fourcellship = FCS;
                threeCellships = THCS;
                twoCellShips = TCS;
                oneCellShips = OCS;
                foreach (var s in ships)
                {
                    if (s.brokenShip)
                    {
                        foreach (var l in s.locations)
                        {
                            buttons[l.row][l.cell].Background = brokenCellColor;
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
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                return fs;
            }
            public void DisplayField(int[][] fs, Button[][] buttons)//According to state of cells of ships, method fills up array with numbers 1 or 2
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

        public class AvailableShipField : GameField
        {
            public void ShowAvailableShip()
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    ships.Add(new Ship());
                }
                ships[0].locations.Add(new Location(1, 1));
                ships[0].locations.Add(new Location(2, 1));
                ships[0].locations.Add(new Location(3, 1));
                ships[0].locations.Add(new Location(4, 1));
                ships[1].locations.Add(new Location(2, 3));
                ships[1].locations.Add(new Location(3, 3));
                ships[1].locations.Add(new Location(4, 3));
                ships[2].locations.Add(new Location(2, 5));
                ships[2].locations.Add(new Location(3, 5));
                ships[2].locations.Add(new Location(4, 5));
                ships[3].locations.Add(new Location(6, 1));
                ships[3].locations.Add(new Location(7, 1));
                ships[4].locations.Add(new Location(6, 3));
                ships[4].locations.Add(new Location(7, 3));
                ships[5].locations.Add(new Location(6, 5));
                ships[5].locations.Add(new Location(7, 5));
                ships[6].locations.Add(new Location(1, 7));
                ships[7].locations.Add(new Location(3, 7));
                ships[8].locations.Add(new Location(5, 7));
                ships[9].locations.Add(new Location(7, 7));
                buttons[1][1].Content = "1";
                buttons[2][1].Content = "1";
                buttons[3][1].Content = "1";
                buttons[4][1].Content = "1";
                buttons[2][3].Content = "1";
                buttons[3][3].Content = "1";
                buttons[4][3].Content = "1";
                buttons[2][5].Content = "1";
                buttons[3][5].Content = "1";
                buttons[4][5].Content = "1";
                buttons[6][1].Content = "1";
                buttons[7][1].Content = "1";
                buttons[6][3].Content = "1";
                buttons[7][3].Content = "1";
                buttons[6][5].Content = "1";
                buttons[7][5].Content = "1";
                buttons[1][7].Content = "1";
                buttons[3][7].Content = "1";
                buttons[5][7].Content = "1";
                buttons[7][7].Content = "1";
                checkAllShip();
            }
            public void RemoveAvS(Ship ship)
            {
                foreach (var loc in ship.locations)
                {
                    buttons[loc.row][loc.cell].Content = "0";
                    buttons[loc.row][loc.cell].Background = defaultCellColor;
                }
                ships.Remove(ship);
            }
            public void countActiveCell()
            {
                int fourCellAvail = 0;
                int threeCellAvail = 0;
                int twoCellAvail = 0;
                int oneCellAvail = 0;
                int brokenOneCellShips = 0;
                int brokenTwoCellShips = 0;
                List<Ship> checkList = new List<Ship>();
                Ship tempShip = new Ship();
                int[][] fs = userField.LocationsActivity();
                Button[][] tempbuttons = userField.buttons;
                for (int i = 0; i < fs.Length; i++)
                {
                    for (int j = 0; j < fs[i].Length; j++)
                    {
                        if (!checkList.Contains(userField.GetShip(i, j)))
                        {
                            if (tempbuttons[i][j].Background == fourCellColor)
                            {
                                fourCellAvail += userField.GetShip(i, j).CountCells();
                                checkList.Add(userField.GetShip(i, j));
                            }
                            else if (tempbuttons[i][j].Background == threeCellColor)
                            {
                                threeCellAvail += userField.GetShip(i, j).CountCells();
                                checkList.Add(userField.GetShip(i, j));
                            }
                            else if (tempbuttons[i][j].Background == twoCellColor)
                            {
                                twoCellAvail += userField.GetShip(i, j).CountCells();
                                checkList.Add(userField.GetShip(i, j));
                            }
                            else if (tempbuttons[i][j].Background == oneCellColor)
                            {
                                oneCellAvail += userField.GetShip(i, j).CountCells();
                                checkList.Add(userField.GetShip(i, j));
                            }
                        }
                    }
                }
                regulateAVS(fourCellAvail, threeCellAvail, twoCellAvail, oneCellAvail, brokenOneCellShips, brokenTwoCellShips);
            }
            public void regulateAVS(int FCA, int THCA, int TCA, int OCA, int BOCS, int BTCS)
            {
                foreach (var s in ships)
                {
                    if (s.CountCells() == 4)
                    {
                        foreach (var loc in s.locations)
                        {
                            if (FCA > 0)
                            {
                                buttons[loc.row][loc.cell].Background = usedCellColor;
                                buttons[loc.row][loc.cell].Content = "0";
                                FCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = fourCellColor;
                                buttons[loc.row][loc.cell].Content = "1";
                            }
                        }
                    }
                    else if (s.CountCells() == 3)
                    {
                        foreach (var loc in s.locations)
                        {
                            if (THCA > 0)
                            {
                                buttons[loc.row][loc.cell].Background = usedCellColor;
                                buttons[loc.row][loc.cell].Content = "0";
                                THCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = threeCellColor;
                                buttons[loc.row][loc.cell].Content = "1";
                            }
                        }
                    }
                    else if (s.CountCells() == 2)
                    {
                        foreach (var loc in s.locations)
                        {
                            if (TCA > 0)
                            {
                                buttons[loc.row][loc.cell].Background = usedCellColor;
                                buttons[loc.row][loc.cell].Content = "0";
                                TCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = twoCellColor;
                                buttons[loc.row][loc.cell].Content = "1";
                            }
                        }
                    }
                    else if (s.CountCells() == 1)
                    {
                        foreach (var loc in s.locations)
                        {
                            if (OCA > 0)
                            {
                                buttons[loc.row][loc.cell].Background = usedCellColor;
                                buttons[loc.row][loc.cell].Content = "0";
                                OCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = oneCellColor;
                                buttons[loc.row][loc.cell].Content = "1";
                            }
                        }
                    }
                }
            }
        }

        public class Ship//Class, which determine size of ship
        {
            public List<Location> locations = new List<Location>();//Add info about shipcell
            string logPath = "log.txt";
            public bool brokenShip = false;
            public bool cellChecked = false;
            public Ship() { }
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
            public bool DoesShipExist(int row, int cell)
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
        public void CreateStopDeleteButton()
        {
            stopDM.Content = "Stop the deleting mode";
            stopDM.Visibility = Visibility.Hidden;
            stopDM.Click += StopDelete;
            FunctionalMap.Children.Add(stopDM);
            Grid.SetRow(stopDM, 1);
            Grid.SetColumn(stopDM, 3);
            Grid.SetColumnSpan(stopDM, 2);
        }
        public void InitializeLabelColor()
        {
            Label UserLabel = new Label();
            UserMap.RowDefinitions.Add(new RowDefinition());
            UserMap.Children.Add(UserLabel);
            UserLabel.Background = Brushes.Bisque;
            UserLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            UserLabel.Content = "User Map";
            UserLabel.FontSize = 20;
            Grid.SetRow(UserLabel, 0);
            Grid.SetColumnSpan(UserLabel, 11);
        }
        public void InitializeTimerBlock()
        {
            lTimer.Content = "Timer: ";
            UserMap.Children.Add(lTimer);
            lTimer.VerticalContentAlignment = VerticalAlignment.Center;
            lTimer.HorizontalContentAlignment = HorizontalAlignment.Right;
            lTimer.FontSize = 20;
            lTimer.Background = Brushes.Bisque;
            Grid.SetRow(lTimer, 0);
            Grid.SetColumn(lTimer, 11);
            Grid.SetColumnSpan(lTimer, 2);
            //EnemyMap.Children.Add(fTimer);
            Shipmap.Children.Add(fTimer);
            fTimer.VerticalContentAlignment = VerticalAlignment.Center;
            fTimer.HorizontalContentAlignment = HorizontalAlignment.Left;
            fTimer.Background = Brushes.Bisque;
            fTimer.Content = defaultValueTimer;
            fTimer.FontSize = 20;
            Grid.SetRow(fTimer, 0);
            Grid.SetColumn(fTimer, 0);
            Grid.SetColumnSpan(fTimer, 2);
        }
        public void ReinitializeTimerBlock()
        {
            Shipmap.Children.Remove(fTimer);
            EnemyMap.Children.Add(fTimer);
        }
        public void Init()
        {
            CreateMap();
            CreateStopDeleteButton();
            InitializeTimerBlock();
            //InitializeLabelColor();
            availableShipField.ShowAvailableShip();
        }
        /// <summary>
        /// This method deactivate deleting mode, and return color of the ships to the normal view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopDelete(object sender, RoutedEventArgs e)
        {
            deleteMode = false;
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
                RowDefinition gridRow1 = new RowDefinition();
                ColumnDefinition gridColumn1 = new ColumnDefinition();
                UserMap.RowDefinitions.Add(gridRow1);
                UserMap.ColumnDefinitions.Add(gridColumn1);
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        UserMap.Children.Add(button);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                            Grid.SetColumn(button, j);
                            Grid.SetRow(button, i + 1);
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                            Grid.SetRow(button, i + 1);
                        }
                        else if (i == 0 && j == 0)
                        {
                            Grid.SetRow(button, i + 1);
                            Grid.SetColumn(button, j);
                        }
                    }
                    else
                    {
                        UserMap.Children.Add(userField.buttons[i - 1][j - 1]);
                        Grid.SetRow(userField.buttons[i - 1][j - 1], i + 1);
                        Grid.SetColumn(userField.buttons[i - 1][j - 1], j);
                        userField.buttons[i - 1][j - 1].Background = defaultCellColor;
                        userField.buttons[i - 1][j - 1].Click += userField.AddShip;
                    }
                }
            }
            for (int i = 0; i < mapsize; i++)
            {
                RowDefinition gridRow1 = new RowDefinition();
                ColumnDefinition gridColumn1 = new ColumnDefinition();
                EnemyMap.RowDefinitions.Add(gridRow1);
                EnemyMap.ColumnDefinitions.Add(gridColumn1);
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        EnemyMap.Children.Add(button);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                            Grid.SetColumn(button, j + 2);
                            Grid.SetRow(button, i + 1);
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                            Grid.SetRow(button, i + 1);
                            Grid.SetColumn(button, j + 2);
                        }
                        else if (i == 0 && j == 0)
                        {
                            Grid.SetRow(button, i + 1);
                            Grid.SetColumn(button, j + 2);
                        }
                        button.Background = new SolidColorBrush(Colors.Gray);

                    }
                    else
                    {
                        EnemyMap.Children.Add(enemyField.buttons[i - 1][j - 1]);
                        Grid.SetRow(enemyField.buttons[i - 1][j - 1], i + 1);
                        Grid.SetColumn(enemyField.buttons[i - 1][j - 1], j + 2);
                        enemyField.buttons[i - 1][j - 1].Background = defaultCellColor;
                        enemyField.buttons[i - 1][j - 1].Click += enemyField.AddShip;
                    }
                }
            }
            for (int i = 0; i < mapsize; i++)
            {

                RowDefinition gridRow1 = new RowDefinition();
                ColumnDefinition gridColumn1 = new ColumnDefinition();
                Shipmap.RowDefinitions.Add(gridRow1);
                Shipmap.ColumnDefinitions.Add(gridColumn1);
                for (int j = 0; j < mapsize; j++)
                {
                    Button button = new Button();
                    if (i == 0 || j == 0)
                    {
                        button.Background = new SolidColorBrush(Colors.Gray);
                        Shipmap.Children.Add(button);
                        if (i == 0 && j > 0)
                        {
                            button.Content = namecell[j - 1].ToString();
                            Grid.SetColumn(button, j + 2);
                            Grid.SetRow(button, i + 1);
                        }
                        else if (j == 0 && i > 0)
                        {
                            button.Content = i.ToString();
                            Grid.SetRow(button, i + 1);
                            Grid.SetColumn(button, j + 2);
                        }
                        else if (i == 0 && j == 0)
                        {
                            Grid.SetRow(button, i + 1);
                            Grid.SetColumn(button, j + 2);
                        }
                        button.Background = new SolidColorBrush(Colors.Gray);

                    }
                    else
                    {
                        Shipmap.Children.Add(availableShipField.buttons[i - 1][j - 1]);
                        Grid.SetRow(availableShipField.buttons[i - 1][j - 1], i + 1);
                        Grid.SetColumn(availableShipField.buttons[i - 1][j - 1], j + 2);
                        availableShipField.buttons[i - 1][j - 1].Background = defaultCellColor;
                        availableShipField.buttons[i - 1][j - 1].Click += availableShipField.AddShip;
                    }
                }
            }
        }
        private void Start(object sender, RoutedEventArgs e)
        {
            if (sessionnumber == 0)
            {
                EnemyMap.Visibility = Visibility.Visible;
                Shipmap.Visibility = Visibility.Hidden;
                if (CheckCorrectStart())
                {
                    sessionnumber++;
                    ReinitializeTimerBlock();
                    
                    //EnemyMap.Visibility = Visibility.Visible;
                    //Shipmap.Visibility = Visibility.Hidden;
                }
            }
        }
        public void ResetTimer(object sender, RoutedEventArgs e)
        {
            if (seconds >= 5)
            {
                dt.Stop();
                fTimer.Content = defaultValueTimer;
                seconds = 0;
                minutes = 0;
                dt.Start();
            }
        }
        public bool CheckCorrectStart()
        {
            string message = "";
            foreach (var s in userField.ships)
            {
                if (s.brokenShip)
                {
                    message += "The ship with addresses: " + "\n";
                    foreach (var loc in s.locations)
                    {
                        message += "Row: " + loc.row + " Cell: " + loc.cell + "\n";
                    }
                }
            }
            if (message != "")
            {
                MessageBox.Show("You can't start game because of:" +
                " You have placed or removed the cells of the ships incorectlly" +
                "\nThe List of the ships: " + "\n" + message + "\nRemove these ships please!");
                return false;
            }
            if (userField.DoesNotExistAllTypesOfShips() || enemyField.DoesNotExistAllTypesOfShips())
            {
                MessageBox.Show("You can't start game because of:" +
                        " You haven't placed all ships. Please check available ships on the right field.");
                return false;
            }
            return true;
        }
        public Location GetButtonIndex(Button button)
        {
            for (int i = 0; i < userField.buttons.Length; i++)
            {
                for (int j = 0; j < userField.buttons[i].Length; j++)
                {
                    if (button == userField.buttons[i][j])
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
        static public bool CheckDiagonalCell(Location l, int[][] fs)
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
        /// This method delete all ships, and return map to the default view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearField(object sender, RoutedEventArgs e)
        {
            userField.ships.Clear();
            for (int i = 0; i < userField.buttons.Length; i++)
            {
                for (int j = 0; j < userField.buttons[i].Length; j++)
                {
                    userField.buttons[i][j].Background = defaultCellColor;
                    userField.buttons[i][j].Content = "0";
                }
            }
            userField.checkAllShip();
            availableShipField.ships.Clear();
            availableShipField.ShowAvailableShip();
            availableShipField.checkAllShip();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Tick;
            dt.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            string displaySeconds = "0";
            seconds++;
            if (seconds == 60)
            {
                minutes++;
                seconds = 0;
            }
            if (seconds <= 9)
            {
                displaySeconds += seconds.ToString();
            }
            else if (seconds >= 10 && seconds <= 60)
            {
                displaySeconds = seconds.ToString();
            }
            fTimer.Content = "0" + minutes.ToString() + ":" + displaySeconds;
        }
    }
}
