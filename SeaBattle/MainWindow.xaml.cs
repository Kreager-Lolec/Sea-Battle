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
        static UserField userField = new UserField();
        static EnemyField enemyField = new EnemyField();
        static AvailableShipField availableShipField = new AvailableShipField();
        static int sessionnumber = 0;
        static public bool deleteMode = false;
        static Button stopDM = new Button();
        const int mapsize = 11;
        string namecell = "ABCDEFGHIJ";
        static private string logPath = "log.txt";
        static public Brush fourCellColor = Brushes.Green;
        static public Brush threeCellColor = Brushes.Blue;
        static public Brush twoCellColor = Brushes.Purple;
        static public Brush oneCellColor = Brushes.DarkOrange;
        static public Brush defaultCellColor = Brushes.LightGray;
        static public Brush usedCellColor = Brushes.DarkGray;
        static public Brush destroyedCellColor = Brushes.Red;
        static public Brush deactivateCellColor = Brushes.DarkGray;
        static public Brush brokenCellColor = Brushes.Red;
        static public bool displayContent = true;
        static public bool debug = false;
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
                if (debug)
                {
                    userField.DisplayField(userField.LocationsActivity(), userField.buttons);
                    enemyField.DisplayField(enemyField.LocationsActivity(), enemyField.buttons);
                    availableShipField.DisplayField(availableShipField.LocationsActivity(), availableShipField.buttons);
                }
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
            public int shipCellState = 0;

            public Location()
            {

            }
            public Location(int row, int cell)
            {
                this.row = row;
                this.cell = cell;
                shipCellState = 1;
            }
            public void SetLocation(int row, int cell)
            {
                this.row = row;
                this.cell = cell;
                shipCellState = 1;
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
            public int queueWay = 0;//0 - queue of the user, 1 - queue of the bot
            public int[][] fs = CreateArray(10, 10);
            public Random rnd = new Random();
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
            public bool OverFlowedCountOfShips()
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
            public Button GetButtonIndex(Button button)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    for (int j = 0; j < buttons[i].Length; j++)
                    {
                        if (button == buttons[i][j])
                        {
                            return buttons[i][j];
                        }
                    }
                }
                return null;
            }
            public void DeactivateCellAndShip(int i, int j)
            {
                int destroyedCell = 0;
                GetShip(i, j).getCell(i, j).shipCellState = 0;
                foreach (var loc in GetShip(i, j).locations)
                {
                    if (loc.shipCellState == 0)
                    {
                        destroyedCell++;
                    }
                }
                if (destroyedCell == GetShip(i, j).locations.Count)
                {
                    GetShip(i, j).destroyedShip = true;
                }
            }
            public bool IfExistsDestroyedNotFullShip()
            {
                int destroyedCell = 0;
                int shipsCount = 0;
                foreach (var s in ships)
                {
                    destroyedCell = 0;
                    foreach (var loc in s.locations)
                    {
                        if (loc.shipCellState == 0)
                        {
                            destroyedCell++;
                            //MessageBox.Show("Destoyedcell: " + destroyedCell.ToString());
                        }
                    }
                    if (destroyedCell < s.locations.Count && destroyedCell > 0)
                    {
                        return true;
                    }
                    else if (destroyedCell == 0 || destroyedCell == s.CountCells())
                    {
                        shipsCount++;
                        //MessageBox.Show("ShipCount: " + shipsCount);
                    }
                }
                if (shipsCount == ships.Count)
                {
                    return false;
                }
                return true;
            }

            public Ship NotFullGetDestroyedShip()
            {
                int destroyedCell = 0;
                int shipsCount = 0;
                foreach (var s in ships)
                {
                    destroyedCell = 0;
                    foreach (var loc in s.locations)
                    {
                        if (loc.shipCellState == 0)
                        {
                            destroyedCell++;
                            //MessageBox.Show("Destoyedcell: " + destroyedCell.ToString());
                        }
                    }
                    if (destroyedCell < s.locations.Count && destroyedCell > 0)
                    {
                        return s;
                    }
                    else if (destroyedCell == 0 || destroyedCell == s.CountCells())
                    {
                        shipsCount++;
                        //MessageBox.Show("ShipCount: " + shipsCount);
                    }
                }
                if (shipsCount == ships.Count)
                {
                    return null;
                }
                return null;
            }
            public bool IfDestroyedOneCelloFShip(Ship s)
            {
                int destroyedCell = 0;
                foreach (var loc in s.locations)
                {
                    if (loc.shipCellState == 0)
                    {
                        destroyedCell++;
                    }
                }
                if (destroyedCell == 1 && s.locations.Count > 1)
                {
                    return true;
                }
                return false;
            }
            public void DeactivateExactCell(Ship s, ref int i, ref int j)
            {
                foreach (var loc in s.locations)
                {
                    if (loc.shipCellState == 1)
                    {
                        i = loc.row;
                        j = loc.cell;
                        break;
                    }
                }
            }
            public bool IfFTCellsShipAlive(Ship s)
            {
                int destroyedCell = 0;
                foreach (var loc in s.locations)
                {
                    if (loc.shipCellState == 0)
                    {
                        destroyedCell++;
                    }
                }
                if (destroyedCell >= 2 && s.locations.Count > 2)
                {
                    return true;
                }
                return false;
            }

            public void CheckDestroyedShipsArea()
            {
                foreach (var s in ships)
                {
                    if (s.destroyedShip)
                    {
                        foreach (var l in s.locations)
                        {
                            FillDestroyedShipArea(l);
                        }
                    }
                }
            }
            public void FillDestroyedShipArea(Location l)
            {
                for (int i = l.row; i < LocationsActivity().Length;)
                {
                    if (i == 9)
                    {
                        break;
                    }
                    else if (buttons[i + 1][l.cell].Background == destroyedCellColor)
                    {
                        break;
                    }
                    else
                    {
                        buttons[i + 1][l.cell].Background = deactivateCellColor;
                        break;
                    }
                }
                for (int i = l.row; i >= 0;)
                {
                    if (i == 0)
                    {
                        break;
                    }
                    else if (buttons[i - 1][l.cell].Background == destroyedCellColor)
                    {
                        break;
                    }
                    else
                    {
                        buttons[i - 1][l.cell].Background = deactivateCellColor;
                        break;
                    }
                }
                for (int j = l.cell; j < LocationsActivity()[l.row].Length;)
                {
                    if (j == 9)
                    {
                        break;
                    }
                    else if (buttons[l.row][j + 1].Background == destroyedCellColor)
                    {
                        break;
                    }
                    else
                    {
                        buttons[l.row][j + 1].Background = deactivateCellColor;
                        break;
                    }
                }
                for (int j = l.cell; j >= 0;)
                {
                    if (j == 0)
                    {
                        break;
                    }
                    else if (buttons[l.row][j - 1].Background == destroyedCellColor)
                    {
                        break;
                    }
                    else
                    {
                        buttons[l.row][j - 1].Background = deactivateCellColor;
                        break;
                    }
                }
                for (int i = l.row; i < LocationsActivity().Length; i++)
                {
                    for (int j = l.cell; j < LocationsActivity()[i].Length;)
                    {
                        if (l.row == 9 || l.cell == 9)
                        {
                            break;
                        }
                        else
                        {
                            buttons[l.row + 1][l.cell + 1].Background = deactivateCellColor;
                            break;
                        }
                    }
                }
                for (int i = l.row; i < LocationsActivity().Length; i++)
                {
                    for (int j = l.cell; j > 0;)
                    {
                        if (l.row == 9 || l.cell == 0)
                        {
                            break;
                        }
                        else
                        {
                            buttons[l.row + 1][l.cell - 1].Background = deactivateCellColor;
                            break;
                        }
                    }
                }
                for (int i = l.row; i > 0; i--)
                {
                    for (int j = l.cell; j > 0;)
                    {
                        if (l.row == 0 || l.cell == 0)
                        {
                            break;
                        }
                        else
                        {
                            buttons[l.row - 1][l.cell - 1].Background = deactivateCellColor;
                            break;
                        }
                    }
                }
                for (int i = l.row; i > 0; i--)
                {
                    for (int j = 0; j < LocationsActivity()[i].Length;)
                    {
                        if (l.row == 0 || l.cell == 9)
                        {
                            break;
                        }
                        else
                        {
                            buttons[l.row - 1][l.cell + 1].Background = deactivateCellColor;
                            break;
                        }
                    }
                }
            }
            public void resetField()
            {
                ships.Clear();
                fourcellship = 0;
                threeCellships = 0;
                twoCellShips = 0;
                oneCellShips = 0;
                checkAllShip();
                for (int i = 0; i < LocationsActivity().Length; i++)
                {
                    for (int j = 0; j < LocationsActivity()[i].Length; j++)
                    {
                        LocationsActivity()[i][j] = 0;
                    }
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
                                    location.shipCellState = 0;
                                    int countEntireShip = 0;
                                    Ship s1 = new Ship();
                                    Ship s2 = new Ship();
                                    foreach (var loc in ship.locations)
                                    {
                                        //MessageBox.Show(loc.row + " " + loc.cell);
                                        //MessageBox.Show(" State of cell: " + loc.shipCellAlive + " Size: " + s1.locations.Count);
                                        if (loc.shipCellState == 1)
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
                                        else if (loc.shipCellState == 0 && countEntireShip < 3)
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
                                    location.shipCellState = 0;
                                    createNewShip = false;
                                    int countEntireShip = 0;
                                    foreach (var loc in ship.locations)
                                    {
                                        //MessageBox.Show(loc.row + " " + loc.cell);
                                        //MessageBox.Show(" State of cell: " + loc.shipCellAlive + " Size: " + s3.locations.Count);
                                        if (loc.shipCellState == 1)
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
                                        else if (loc.shipCellState == 0 && countEntireShip < 2)
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
                            if (debug)
                            {
                                buttons[l.row][l.cell].Content = "1";
                            }
                            buttons[l.row][l.cell].Background = fourCellColor;
                        }
                    }
                    else if (ship.CountCells() == 3)
                    {
                        if (THCS <= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (THCS > 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
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
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (TCS <= 3)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = twoCellColor;
                            }
                        }
                        else if (THCS >= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
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
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = oneCellColor;
                            }
                        }
                        else if (OCS > 4 && TCS < 3)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = twoCellColor;
                            }
                        }
                        else if (TCS >= 3 && THCS < 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
                                buttons[l.row][l.cell].Background = threeCellColor;
                            }
                        }
                        else if (THCS >= 2)
                        {
                            foreach (var l in ship.locations)
                            {
                                if (debug)
                                {
                                    buttons[l.row][l.cell].Content = "1";
                                }
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
                        LocationsActivity();
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
                                            if (OverFlowedCountOfShips())
                                            {
                                                s.brokenShip = true;
                                            }
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
                                                //WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
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
                                                //WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
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
                                foreach (var s in ships)
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
                                                //WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
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
                                foreach (var s in ships)
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
                                                //WriteToLog("Size: " + s.locations.Count + " K: " + k + " Loc.row: " + loc.row + " Loc.cell: " + loc.cell, new StackTrace());
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
                            if (debug)
                            {
                                buttons[i][j].Content = "0";
                            }
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
                for (int i = 0; i < mapsize - 1; i++)
                {
                    for (int j = 0; j < mapsize - 1; j++)
                    {
                        fs[i][j] = 0;
                    }
                }
                foreach (Ship s in ships)
                {
                    try
                    {
                        foreach (Location sl in s.locations)
                        {
                            if (sl.shipCellState == 0)//Destroyed cell
                            {
                                fs[sl.row][sl.cell] = 2;
                            }
                            else if (sl.shipCellState == 1)//Alive cell
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
        public class UserField : GameField
        {
            public void PickUserShip()
            {
                try
                {
                    int i = -1;
                    int j = -1;
                    bool oneMoreAttack = true;
                    if (queueWay == 1)
                    {
                        if (sessionnumber == 1)
                        {
                            while (oneMoreAttack)
                            {
                                i = -1;
                                j = -1;
                                if (IfExistsDestroyedNotFullShip())
                                {
                                    //MessageBox.Show("Lol0");
                                    if (IfDestroyedOneCelloFShip(NotFullGetDestroyedShip()))
                                    {
                                        //MessageBox.Show("Lol1");
                                        foreach (var loc in NotFullGetDestroyedShip().locations)
                                        {
                                            if (loc.shipCellState == 0)
                                            {
                                                randomRoute(loc, ref i, ref j);
                                                break;
                                            }
                                        }
                                    }
                                    else if (IfFTCellsShipAlive(NotFullGetDestroyedShip()))
                                    {
                                        //MessageBox.Show("Lol2");
                                        foreach (var loc in NotFullGetDestroyedShip().locations)
                                        {
                                            DeactivateExactCell(NotFullGetDestroyedShip(), ref i, ref j);
                                            break;
                                        }
                                    }
                                }
                                else if (i == -1 && j == -1)
                                {
                                    randomCell(ref i, ref j);
                                }
                                if (GetShip(i, j) == null)
                                {
                                    oneMoreAttack = false;
                                    LocationsActivity()[i][j] = 2;
                                    buttons[i][j].Background = deactivateCellColor;
                                }
                                else
                                {
                                    DeactivateCellAndShip(i, j);
                                    buttons[i][j].Background = destroyedCellColor;
                                    CheckDestroyedShipsArea();
                                }
                            }
                        }
                    }
                    enemyField.queueWay = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " PickUserShip");
                    throw;
                }
            }
            private void randomCell(ref int i, ref int j)
            {
                do
                {
                    i = rnd.Next(0, 10);
                    j = rnd.Next(0, 10);
                } while (LocationsActivity()[i][j] == 2);
            }
            private void randomRoute(Location l, ref int pickedRow, ref int pickedCell)
            {
                try
                {
                    bool continueRandom = false;
                    int route = -1;
                    do
                    {
                        route = rnd.Next(1, 5);
                        if (route == 1)
                        {
                            for (int i = l.row; i < LocationsActivity().Length; i++)
                            {

                                if (i == 9)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[i + 1][l.cell] == 2)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[i + 1][l.cell] == 1 || LocationsActivity()[i + 1][l.cell] == 0)
                                {
                                    pickedRow = i + 1;
                                    pickedCell = l.cell;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 2)
                        {
                            for (int i = l.row; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[i - 1][l.cell] == 2)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[i - 1][l.cell] == 1 || LocationsActivity()[i - 1][l.cell] == 0)
                                {
                                    pickedRow = i - 1;
                                    pickedCell = l.cell;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 3)
                        {
                            for (int j = l.cell; j < LocationsActivity()[l.row].Length; j++)
                            {
                                if (j == 9)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[l.row][j + 1] == 2)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[l.row][j + 1] == 1 || LocationsActivity()[l.row][j + 1] == 0)
                                {
                                    pickedRow = l.row;
                                    pickedCell = j + 1;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 4)
                        {
                            for (int j = l.cell; j >= 0; j--)
                            {
                                if (j == 0)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[l.row][j - 1] == 2)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[l.row][j - 1] == 1 || LocationsActivity()[l.row][j - 1] == 0)
                                {
                                    pickedRow = l.row;
                                    pickedCell = j - 1;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                    } while (continueRandom);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " RandomRoute");
                    throw;
                }
            }
        }
        public class EnemyField : GameField
        {
            private void CheckEntireShips(Ship s)
            {

            }
            private void AddNewship(int i, int j)
            {
                Location l = new Location(i, j);
                Ship ship = new Ship();
                ship.locations.Add(l);
                ships.Add(ship);
            }
            private Location GetCell(Ship s)
            {
                return s.locations[0];
            }

            public void PickShips()
            {
                try
                {
                    int i = -1;
                    int j = -1;
                    int route = -1;
                    int countiteration = 0;
                    WriteToLog("New Pick ", new StackTrace());
                    WriteToLog("----------------------------------------------------------------------------------------------------", new StackTrace());
                    do
                    {
                        do
                        {
                            randomCell(ref i, ref j);
                            countiteration++;
                            if (countiteration > 300)
                            {
                                enemyField.resetField();
                                enemyField.PickShips();
                                break;
                            }
                        } while (!CheckNearbyShip(new Location(i, j), LocationsActivity()) || !CheckDiagonalCell(new Location(i, j), LocationsActivity()));
                        if (ships.Count < 10)
                        {
                            AddNewship(i, j);
                            WriteToLog("Add new Ship: " + " i: " + i.ToString() + " j: " + j.ToString(), new StackTrace());
                            WriteToLog("ShipCount " + ships.Count, new StackTrace());
                            checkAllShip();
                            if (ships.Count >= 10)
                            {
                                break;
                            }
                        }
                        if (i != -1 && j != -1)
                        {
                            if (fourcellship < 1 || twoCellShips < 3 || threeCellships < 2)
                            {
                                WriteToLog("Add cell to the current Ship: ", new StackTrace());
                                int defaultRow = i;
                                int defaultCell = j;
                                randomRoute(GetCell(GetShip(defaultRow, defaultCell)), ref i, ref j, ref route);
                                if (i == -1 && j == -1)
                                {
                                    continue;
                                }
                                //WriteToLog(defaultRow.ToString() + " " + defaultCell.ToString(), new StackTrace());
                                ConstructShip(GetShip(defaultRow, defaultCell), i, j, route);
                                if (GetShip(defaultRow, defaultCell).destroyedShip)
                                {
                                    ships.Remove(GetShip(defaultRow, defaultCell));
                                    continue;
                                }
                            }
                        }
                    } while (DoesNotExistAllTypesOfShips());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            private void ConstructShip(Ship s, int defaultRow, int defaultCell, int route)
            {
                try
                {
                    int i = defaultRow;
                    int j = defaultCell;
                    bool succesufulOpetation = false;
                    Location lGeneral = new Location();
                    lGeneral.row = defaultRow;
                    lGeneral.cell = defaultCell;
                    if (CheckDiagonalCell(lGeneral, LocationsActivity()))
                    {
                        //WriteToLog(i.ToString() + " " + j.ToString() + " General", new StackTrace());
                        succesufulOpetation = CheckPartition(lGeneral);
                    }
                    if (!succesufulOpetation)
                    {
                        s.destroyedShip = true;
                    }
                    succesufulOpetation = false;
                    if (fourcellship == 0 && !s.destroyedShip)
                    {
                        do
                        {
                            Location l = new Location();
                            l = SetRoute(ref i, ref j, route);
                            if (l != null && CheckDiagonalCell(l, LocationsActivity()))
                            {
                                succesufulOpetation = CheckPartition(l);
                            }
                            if (succesufulOpetation)
                            {
                                succesufulOpetation = false;
                                WriteToLog(s.CountCells() + " Size Fourcell", new StackTrace());
                            }
                            else
                            {
                                s.destroyedShip = true;
                            }
                        } while (s.CountCells() < 4 && !s.destroyedShip);
                    }
                    else if (threeCellships >= 0 && threeCellships < 2 && !s.destroyedShip)
                    {
                        do
                        {
                            Location l = new Location();
                            l = SetRoute(ref i, ref j, route);
                            if (l != null && CheckDiagonalCell(l, LocationsActivity()))
                            {
                                //WriteToLog(i.ToString() + " " + j.ToString() + " ThreeCell", new StackTrace());
                                succesufulOpetation = CheckPartition(l);
                            }
                            if (succesufulOpetation)
                            {
                                succesufulOpetation = false;
                                WriteToLog(s.CountCells() + " Size Threecell", new StackTrace());
                            }
                            else
                            {
                                s.destroyedShip = true;
                            }
                        } while (s.CountCells() < 3 && !s.destroyedShip); ;
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog(ex.ToString(), new StackTrace());
                    throw;
                }
            }
            private Location SetRoute(ref int defaultRow, ref int defaultCell, int route)
            {
                try
                {
                    Location l = new Location();
                    if (route == 1 && defaultRow != 9)
                    {
                        l.row = ++defaultRow;
                        l.cell = defaultCell;
                    }
                    else if (route == 2 && defaultRow != 0)
                    {
                        l.row = --defaultRow;
                        l.cell = defaultCell;
                    }
                    else if (route == 3 && defaultCell != 9)
                    {
                        l.row = defaultRow;
                        l.cell = ++defaultCell;
                    }
                    else if (route == 4 && defaultCell != 0)
                    {
                        l.row = defaultRow;
                        l.cell = --defaultCell;
                    }
                    WriteToLog(l.row.ToString() + " " + l.cell.ToString(), new StackTrace());
                    if (l.row == -1 && l.cell == -1)
                    {
                        l = null;
                    }
                    return l;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            private void randomCell(ref int i, ref int j)
            {
                do
                {
                    i = rnd.Next(0, 10);
                    j = rnd.Next(0, 10);
                    WriteToLog("i: " + i.ToString() + " j: " + j.ToString() + " Locactivity: " + LocationsActivity()[i][j], new StackTrace());
                } while (LocationsActivity()[i][j] == 1);
            }

            private void randomRoute(Location l, ref int pickedRow, ref int pickedCell, ref int route)
            {
                try
                {
                    bool usedFirstRoute = false;
                    bool usedSecondRoute = false;
                    bool usedThirdRoute = false;
                    bool usedForthroute = false;
                    bool continueRandom = false;
                    do
                    {
                        if (usedFirstRoute && usedSecondRoute && usedThirdRoute && usedForthroute)
                        {
                            pickedRow = -1;
                            pickedCell = -1;
                            break;
                        }
                        route = rnd.Next(1, 5);
                        if (route == 1)
                        {
                            for (int i = l.row; i < LocationsActivity().Length; i++)
                            {
                                usedFirstRoute = true;
                                if (i == 9)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (LocationsActivity()[i + 1][l.cell] == 1)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (i - l.row == 1)
                                {
                                    pickedRow = i;
                                    pickedCell = l.cell;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 2)
                        {
                            usedSecondRoute = true;
                            for (int i = l.row; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                else if (LocationsActivity()[i - 1][l.cell] == 1)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (l.row - i == 1)
                                {
                                    pickedRow = i;
                                    pickedCell = l.cell;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 3)
                        {
                            usedThirdRoute = true;
                            for (int j = l.cell; j < LocationsActivity()[l.row].Length; j++)
                            {
                                if (j == 9)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (LocationsActivity()[l.row][j + 1] == 1)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (j - l.cell == 1)
                                {
                                    pickedRow = l.row;
                                    pickedCell = j;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                        if (route == 4)
                        {
                            usedForthroute = true;
                            for (int j = l.cell; j >= 0; j--)
                            {
                                if (j == 0)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (LocationsActivity()[l.row][j - 1] == 1)
                                {
                                    continueRandom = true;
                                    break;
                                }
                                if (l.cell - j == 1)
                                {
                                    pickedRow = l.row;
                                    pickedCell = j;
                                    continueRandom = false;
                                    break;
                                }
                            }
                        }
                    } while (continueRandom);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " RandomRoute");
                    throw;
                }
            }
            public void PickEnemyShip(object sender, RoutedEventArgs e)
            {
                try
                {
                    bool breakOperand = false;
                    bool oneMoreAttack = true;
                    if (queueWay == 0)
                    {
                        Button pressedButton = sender as Button;
                        if (sessionnumber == 1)
                        {
                            for (int i = 0; i < buttons.Length; i++)
                            {
                                for (int j = 0; j < buttons[i].Length; j++)
                                {
                                    if (pressedButton == buttons[i][j])
                                    {
                                        if (GetShip(i, j) == null)
                                        {
                                            oneMoreAttack = false;
                                            buttons[i][j].Background = deactivateCellColor;
                                        }
                                        else
                                        {
                                            DeactivateCellAndShip(i, j);
                                            buttons[i][j].Background = destroyedCellColor;
                                            CheckDestroyedShipsArea();
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
                        if (!oneMoreAttack)
                        {
                            userField.queueWay = 1;
                            userField.PickUserShip();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
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
                checkAllShip();
            }
            //public void RemoveAvS(Ship ship)
            //{
            //    foreach (var loc in ship.locations)
            //    {
            //        buttons[loc.row][loc.cell].Content = "0";
            //        buttons[loc.row][loc.cell].Background = defaultCellColor;
            //    }
            //    ships.Remove(ship);
            //}
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
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "0";
                                }
                                FCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = fourCellColor;
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "1";
                                }
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
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "0";
                                }
                                THCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = threeCellColor;
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "1";
                                }
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
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "0";
                                }
                                TCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = twoCellColor;
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "1";
                                }
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
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "0";
                                }
                                OCA--;
                            }
                            else
                            {
                                buttons[loc.row][loc.cell].Background = oneCellColor;
                                if (debug)
                                {
                                    buttons[loc.row][loc.cell].Content = "1";
                                }
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
            public bool destroyedShip = false;
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

            public Location getCell(int i, int j)
            {
                foreach (var l in locations)
                {
                    if (l.row == i && l.cell == j)
                    {
                        return l;
                    }
                }
                return null;
            }
            public bool DoesShipExist(int row, int cell)
            {
                foreach (Location l in locations)
                {
                    if (l.row == row && l.cell == cell)
                    {
                        //WriteToLog("Searched row: " + l.row + " Searched cell: " + l.cell);
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
            if (Shipmap.Children.Contains(fTimer))
            {
                Shipmap.Children.Remove(fTimer);
                EnemyMap.Children.Add(fTimer);
            }
            else if(EnemyMap.Children.Contains(fTimer))
            {
                EnemyMap.Children.Remove(fTimer);
                Shipmap.Children.Add(fTimer);
            }
        }
        public void Init()
        {
            CreateMap();
            CreateStopDeleteButton();
            InitializeTimerBlock();
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
            MessageBox.Show("You can't delete the ships while you are playing");
            if (sessionnumber == 0)
            {
                deleteMode = true;
                stopDM.Visibility = Visibility.Visible;
            }
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
                        //enemyField.buttons[i - 1][j - 1].Click += enemyField.AddShip;
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
        public void ClearYourField(object sender, RoutedEventArgs e)
        {
            if (sessionnumber == 0)
            {
                MessageBox.Show("Field has been cleared succesfully");
                userField.resetField();
                availableShipField.resetField();
                availableShipField.ShowAvailableShip();
            }
            else
            {
                MessageBox.Show("You can't delete your ships while you are playing");
            }
        }
        public void RestartGame(object sender, RoutedEventArgs e)
        {
            if (sessionnumber == 1)
            {
                userField.resetField();
                enemyField.resetField();
                availableShipField.resetField();
                availableShipField.ShowAvailableShip();
                ReinitializeTimerBlock();
                EnemyMap.Visibility = Visibility.Hidden;
                Shipmap.Visibility = Visibility.Visible;
                MessageBox.Show("Game has been restarted succesfully");
            }
            else
            {
                MessageBox.Show("Toggle button: ClearField to reset it");
            }
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
            if (userField.DoesNotExistAllTypesOfShips())
            {
                MessageBox.Show("You can't start game because of:" +
                        " You haven't placed all ships. Please check available ships on the right field.");
                return false;
            }
            return true;
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
        private void Start(object sender, RoutedEventArgs e)
        {
            if (sessionnumber == 0)
            {
                if (CheckCorrectStart())
                {
                    Shipmap.Visibility = Visibility.Hidden;
                    ReinitializeTimerBlock();
                    EnemyMap.Visibility = Visibility.Visible;
                    sessionnumber++;
                    for (int i = 0; i < userField.buttons.Length; i++)
                    {
                        for (int j = 0; j < userField.buttons[i].Length; j++)
                        {
                            userField.buttons[i][j].Click -= userField.AddShip;
                            enemyField.buttons[i][j].Click -= enemyField.AddShip;
                            enemyField.buttons[i][j].Click += enemyField.PickEnemyShip;
                        }
                    }
                    enemyField.PickShips();
                }
            }
            else if (sessionnumber == 1)
            {
                MessageBox.Show("You have already started playing");
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Shipmap.Visibility = Visibility.Hidden;
        //        EnemyMap.Visibility = Visibility.Visible;
        //        enemyField.resetField();
        //        enemyField.PickShips();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        throw;
        //    }
        //}
    }
}

