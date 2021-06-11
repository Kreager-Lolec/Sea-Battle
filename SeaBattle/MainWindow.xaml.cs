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
        public MainWindow()
        {
            InitializeComponent();
            Init();
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
        public void CreateMap()
        {
            this.Width = mapsize * 2 * cellsize + 30;
            this.Height = mapsize * 2 * cellsize + 30;
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
        }
    }
}
