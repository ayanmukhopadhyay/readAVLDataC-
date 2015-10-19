using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace avlData
{
    class Program
    {
        static void Main(string[] args)
        {
            List<DateTime> avlDateTime = new List<DateTime>();
            List<String> carNumber = new List<String>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();

            //Learn how to maintain either a 2D list with different datatype columns. Or else, must iterate over the csv files in a way that dates are sorted by default 
            //get list of avl csv files
            //string[] dirs = Directory.GetFiles(@"/Users/ayanmukhopadhyay/Documents/Vanderbilt/CERL/SurvivalAnalysis/spatioTemporalModelingUpdatedGMM/avl/statePlaneCSV/batch1/");
            string[] dirs = Directory.GetFiles(@"D:\Vanderbilt\CERL\Data\AVL\StatePlane");
            //for (int counterFile = 0; counterFile < dirs.Count(); counterFile++)
            for (int counterFile = 0; counterFile < 3; counterFile++)
            {
                String filenameCurr = dirs[counterFile];
                if (filenameCurr.Contains(".csv"))
                {
                    var reader = new StreamReader(File.OpenRead(filenameCurr));
                    Console.Write("\n" + filenameCurr);
                    var lineCount = File.ReadLines(filenameCurr).Count();
                    Console.Write(lineCount);

                    //List<string> listB = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        try
                        {
                            var splitLine = line.Split(';')[0].Split(',');
                            var currDateTime = Convert.ToDateTime(splitLine[2]);
                            var currCarNumber = splitLine[3];
                            var currX = Convert.ToDouble(splitLine[0]);
                            var currY = Convert.ToDouble(splitLine[1]);
                            avlDateTime.Add(currDateTime);
                            carNumber.Add(currCarNumber);
                            x.Add(currX);
                            y.Add(currY);
                        }
                        catch (System.FormatException e)
                        {
                            continue;
                        }
                    }
                }

            }

            //vars
            double carCountPrior;
            double carCountPost;
            double carCountNeighbourPrior;
            double carCountNeighbourPost;

            Console.Write("Loaded Data Into Memory");
            double gridSize = 1609.34 * 2;
            List<double> survAnalysisRows = new List<double>();
            //read the grid inputs
            var readerGridInput = new StreamReader(File.OpenRead(@"C:\Users\VAIO\Documents\Visual Studio 2013\Projects\readAVLDataC-\gridInputs.csv"));
            while (!readerGridInput.EndOfStream)
            {
                var line = readerGridInput.ReadLine(); //this corresponds to a grid data
                var gridData = new List<String>(line.Split('['));
                //remove the first two elements : blank as the data comes as an array of arrays
                gridData.RemoveAt(1);
                gridData.RemoveAt(0);
                bool counterStartFound = true;
                int counterStart = 0;

                DateTime testStart = DateTime.Now;
                int counterRows = 0;
                foreach (String dataRaw in gridData)
                {
                    counterRows++;
                    //Test time for one loop
                    
                                        
                    var data = dataRaw.Split(',');
                    
                    //define location variables
                    double LocationX = Convert.ToDouble(data[4]);
                    double LocationY = Convert.ToDouble(data[5]);
                    double gridXLower = LocationX - gridSize / 2;
                    double gridXUpper = LocationX + gridSize / 2;
                    double gridYLower = LocationY - gridSize / 2;
                    double gridYUpper = LocationY + gridSize / 2;
                    double gridNeighbourXLower = LocationX - (3 * gridSize / 2);
                    double gridNeighbourXUpper = LocationX + (3 * gridSize / 2);
                    double gridNeighbourYLower = LocationY - (3 * gridSize / 2);
                    double gridNeighbourYUpper = LocationY + (3 * gridSize / 2);

                    DateTime testDateTime = new DateTime(Convert.ToInt16(data[6]), Convert.ToInt16(data[7]), Convert.ToInt16(data[8]), Convert.ToInt16(data[9]), Convert.ToInt16(data[10].Split(']')[0]), 0);
                    //declare lists for grid cars and neighbo grid cars
                    List<String> carsPriorGrid = new List<String>(); //new list for prior car numbers
                    List<String> carsPostGrid = new List<String>(); //new list for post car numbers
                    List<String> carsPriorNeighbor = new List<String>(); //new list for prior car numbers
                    List<String> carsPostNeighbor = new List<String>(); //new list for post car numbers
                    int counter;
                    for (counter = counterStart; counter < avlDateTime.Count(); counter++)
                    {

                        /************PRE POLICE PRESENCE*************/
                        if ((avlDateTime[counter] - testDateTime).TotalSeconds < 0 && Math.Abs((testDateTime - avlDateTime[counter]).TotalSeconds) < 3600 * 8)
                        {                            
                            if (!counterStartFound)
                            {
                                //Set counter Start to True
                                counterStartFound = true;
                                //Update counter Start
                                counterStart = counter;
                            }

                            //check if car lies in grid
                            if ((gridXLower < x[counter] && gridXUpper > x[counter]) && (gridYLower < y[counter] && gridYUpper > y[counter]))
                            {
                                //if yes, add to car prior grid
                                carsPriorGrid.Add(carNumber[counter]);
                            }
                            
                            //else check if car lies in neighboring grids
                            else if ((gridNeighbourXLower < x[counter] && gridNeighbourXUpper > x[counter]) && (gridNeighbourYLower < y[counter] && gridNeighbourYUpper > y[counter]))
                            {
                                //if yes, addd to car neighbor grid
                                carsPriorNeighbor.Add(carNumber[counter]);
                            }
                        }
                        /************POST POLICE PRESENCE*************/
                        else if ((avlDateTime[counter] - testDateTime).TotalSeconds > 0 && Math.Abs((testDateTime - avlDateTime[counter]).TotalSeconds) < 3600 * 4)
                        {
                            //check if car lies in grid
                            if ((gridXLower < x[counter] && gridXUpper > x[counter]) && (gridYLower < y[counter] && gridYUpper > y[counter]))
                            {
                                //if yes, add to car prior grid
                                carsPriorGrid.Add(carNumber[counter]);
                            }

                            //else check if car lies in neighboring grids
                            else if ((gridNeighbourXLower < x[counter] && gridNeighbourXUpper > x[counter]) && (gridNeighbourYLower < y[counter] && gridNeighbourYUpper > y[counter]))
                            {
                                //if yes, addd to car neighbor grid
                                carsPostNeighbor.Add(carNumber[counter]);
                            }
                        }
                        //if we have gone ahead in AVL list, break the loop
                        else if ((avlDateTime[counter] - testDateTime).TotalSeconds > 0)
                        {
                            break;
                        }
                    }

                    carCountPrior = carsPriorGrid.Distinct().Count();
                    carCountPost = carsPriorGrid.Distinct().Count();
                    carCountNeighbourPrior = carsPriorNeighbor.Distinct().Count();
                    carCountNeighbourPost = carsPostNeighbor.Distinct().Count();




                    Console.Write("The loop breaks at " + Convert.ToString(counter));
                    //Test time for one loop
                    if (counterRows == 100)
                    {
                        DateTime testEnd = DateTime.Now;
                        var diff = (testEnd - testStart).TotalSeconds;
                        Console.Write("Time For 2000 Loops is " + Convert.ToString(diff));
                        Console.ReadLine();
                        System.Environment.Exit(1);
                    }
                    
                }
            }
        }
    }
}
