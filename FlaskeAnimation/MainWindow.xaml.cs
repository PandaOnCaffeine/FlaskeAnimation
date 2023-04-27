
//EnergyDrinks Consumed while making this program \\  2  // so far.

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
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FlaskeAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Program.Start();
        }
        public void Spawn(String path)
        {
            Thread.Sleep(1000);
            Dispatcher.Invoke(() =>
            {
                Image myImage = new Image();
                myImage.Source = new BitmapImage(new Uri($@"pack://application:,,,/{path}.png"));
                myImage.Margin = new Thickness(0, 150, 0, 0);
                myImage.Height = 100;
                myImage.Width = 100;
                Woop.Children.Add(myImage);

                Storyboard sb = (Storyboard)FindResource("ImageAnimation");
                Storyboard.SetTarget(sb, myImage);
                sb.Completed += (sender, args) => { Woop.Children.Remove(myImage); };
                sb.Begin();
            });
        }
    }
    public class Program
    {
        static MainWindow m1 = (MainWindow)Application.Current.MainWindow;
        //Buffer array, Used by producer and splitter
        static int bufferIndex = 0;
        static Drink[] buffer = new Drink[10];

        //BeersBuffer Array, Used by splitter and beerConsumer
        static int beersIndex = 0;
        static Drink[] beers = new Drink[10];

        //Energydrinks array, Used by splitter and EnergyConsumer  
        static int energyIndex = 0;
        static Drink[] energydrinks = new Drink[10];

        public static void Start()
        {
            Thread p1 = new Thread(new ThreadStart(Producer));
            p1.Name = "Producer";
            p1.Start();

            Thread s1 = new Thread(new ThreadStart(Splitter));
            s1.Name = "Splitter";
            s1.Priority = ThreadPriority.BelowNormal;
            s1.Start();

            Thread c1 = new Thread(new ThreadStart(BeerConsumer));
            c1.Name = "Consumer 1";
            c1.Start();

            Thread c2 = new Thread(new ThreadStart(EnergydrinksConsumer));
            c2.Name = "Consumer 2";
            c2.Start();
        }
        static void Producer()
        {
            //Creates a random variable
            Random rand = new Random();

            while (true)
            {
                //100 millisec delay
                Thread.Sleep(2000);

                //Locks buffer array
                lock (buffer)
                {
                    //if buffer is full then it waits on a pulse from splitter
                    if (bufferIndex == buffer.Length)
                    {
                        Monitor.Wait(buffer);
                    }
                    //random number between 0 and 1
                    int rng = rand.Next(0, 2);

                    //If number is 0 then produces a beer 
                    if (rng == 0)
                    {
                        //Adds a beer to the buffer array
                        buffer[bufferIndex] = new Beer("Beer"); bufferIndex++;
                        m1.Spawn("Beer");
                        Console.WriteLine($"[{Thread.CurrentThread.Name}]  | Produced beer", Console.ForegroundColor = ConsoleColor.DarkBlue);
                    }
                    //Else produces a energydrink
                    else
                    {
                        //Adds a energydrink to the buffer array
                        buffer[bufferIndex] = new EnergiDrink("EnergyDrink"); bufferIndex++;
                        m1.Spawn("Energy2");
                        Console.WriteLine($"[{Thread.CurrentThread.Name}]  | Produced EnergyDrink", Console.ForegroundColor = ConsoleColor.Green);
                    }
                }
            }
        }
        static void Splitter()
        {
            while (true)
            {
                //Locks buffer array
                lock (buffer)
                {
                    //If buffer array is full
                    if (bufferIndex > 9)
                    {
                        //For loop that loop through buffer array and splits the beers into a beers array and energydrinks to a energydrinks array 
                        for (int i = 0; i < bufferIndex; i++)
                        {
                            //100 millisec delay
                            Thread.Sleep(3000);

                            //if object is a beer
                            if (buffer[i].Name == "Beer")
                            {
                                //Locks beers array
                                lock (beers)
                                {
                                    //if beers array is full, then monitor wait, waits till consumer send a pulse and array is empty
                                    if (beersIndex > 9)
                                    {
                                        Monitor.Wait(beers);
                                    }

                                    //Sets a index in beers array to the beer from buffer array
                                    beers[beersIndex] = buffer[i]; beersIndex++;

                                    //Removes the beer from buffer array
                                    buffer[i] = null;
                                    Console.WriteLine($"[{Thread.CurrentThread.Name}]  | Moved Beer from buffer to beers", Console.ForegroundColor = ConsoleColor.DarkBlue);
                                }
                            }
                            //else, its not a beer then its a energydrink
                            else
                            {
                                //Locks energydrinks array
                                lock (energydrinks)
                                {
                                    //If energydrinks array is full, then monitor wait, waits till consumer send a pulse and array is empty
                                    if (energyIndex > 9)
                                    {
                                        Monitor.Wait(energydrinks);
                                    }

                                    //Sets a index in energydrinks array to the energydrink from buffer array
                                    energydrinks[energyIndex] = buffer[i]; energyIndex++;

                                    //Removes the energydrink from buffer array
                                    buffer[i] = null;
                                    Console.WriteLine($"[{Thread.CurrentThread.Name}]  | Moved Energy Drink from buffer to Energydrinks", Console.ForegroundColor = ConsoleColor.Green);
                                }
                            }
                        }
                        //Sets buffer back to 0
                        bufferIndex = 0;

                        //Pulse the producer that it can start producing again
                        Monitor.PulseAll(buffer);
                    }

                }
            }
        }
        static void BeerConsumer()
        {
            while (true)
            {
                //If beers array is full
                if (beersIndex > 9)
                {
                    //Locks beers array
                    lock (beers)
                    {
                        //for loop, loops through beers array and consumes all the beers 
                        for (int i = 0; i < beersIndex; i++)
                        {
                            Thread.Sleep(3000);
                            Console.WriteLine($"[{Thread.CurrentThread.Name}]| Beer has been consumt", Console.ForegroundColor = ConsoleColor.DarkBlue);
                            beers[i] = null;
                        }
                        //Sets beersIndex back to 0 beers, because there's no beers left in the array
                        beersIndex = 0;

                        //Pulses the monitor wait that are waiting on a pulse from beers 
                        Monitor.PulseAll(beers);
                    }
                }
            }
        }
        static void EnergydrinksConsumer()
        {
            while (true)
            {

                //If energydrink array is full
                if (energyIndex > 9)
                {
                    //Locks energydrinks array
                    lock (energydrinks)
                    {
                        //for loop, loops through energydrinks array and consumes the drinks
                        for (int i = 0; i < energyIndex - 1; i++)
                        {
                            Thread.Sleep(3000);
                            Console.WriteLine($"[{Thread.CurrentThread.Name}]| Energy Drink has been consumt", Console.ForegroundColor = ConsoleColor.Green);
                            energydrinks[i] = null;
                        }
                        //sets energydrink index to 0
                        energyIndex = 0;

                        //Pulses all waiting threads that use energydrinks array
                        Monitor.PulseAll(energydrinks);
                    }
                }
            }
        }
    }
    public abstract class Drink
    {
        protected string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        protected int pant;
    }
    class EnergiDrink : Drink
    {
        //private int Caffeine;
        public EnergiDrink(string name)
        {
            _name = name;
        }
    }
    class Beer : Drink
    {
        //private int alcoholProcent;
        public Beer(string name)
        {
            _name = name;
        }
    }
    //internal class movement
    //{
    //    static void MoveToSplitter()
    //    {

    //    }
    //    static void SplitToBeer()
    //    {

    //    }
    //    static void SplitToEnergy()
    //    {

    //    }
    //}
}
