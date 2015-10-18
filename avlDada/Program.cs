using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace avlData
{
    class Program
    {
        static void Main(string[] args)
        {
            List<DateTime> avlDateTime = new List<DateTime>();
            //Learn how to maintain either a 2D list with different datatype columns. Or else, must iterate over the csv files in a way that dates are sorted by default 
            //get list of avl csv files
            string[] dirs = Directory.GetFiles(@"E:\avlData");
            for (int counterFile = 0; counterFile < dirs.Count(); counterFile++)
            {
                String filenameCurr = dirs[counterFile];
                var reader = new StreamReader(File.OpenRead(filenameCurr));
                
                //List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    try
                    {
                        var currDateTime = Convert.ToDateTime(line.Split(';')[0].Split(',')[0]);
                        avlDateTime.Add(currDateTime);
                    }
                    catch (System.FormatException e)
                    {
                        continue;
                    }


                    //listA.Add(values[0]);
                    //listB.Add(values[1]);
                }
            }

            //read an input csv file: This should contain a particular grid coordinate, crime time, censored, grid counter x, grid counter y,
            //Run through the below steps in a loop for each entry in the input file. Then read another input file and repeat


            //compare a sample date with all the dates to evaluate performance with python
            DateTime testDateTime = new DateTime(2009, 1, 1, 0, 0, 0);
            DateTime start = DateTime.Now;
            //declare lists for grid cars and neighbo grid cars
            List<String> carsPriorGrid = new List<String>(); //new list for prior car numbers
            List<String> carsPostGrid = new List<String>(); //new list for post car numbers
            List<String> carsPriorNeighbor = new List<String>(); //new list for prior car numbers
            List<String> carsPostNeighbor = new List<String>(); //new list for post car numbers
            //Iterate through AVL List
            //
            for (int counter = 0; counter < avlDateTime.Count(); counter++) 
            {
                //if we have gone ahead in AVL list, break the loop
                if ((avlDateTime[counter] - testDateTime).TotalSeconds > 0)
                {
                    break;
                }
                if ((testDateTime - avlDateTime[counter]).TotalSeconds < 0 && Math.Abs((testDateTime - avlDateTime[counter]).TotalSeconds) < 3600*8)
                { 
                    //Set counter Start to True
                    //Update counter Start
                    //check if car lies in grid
                        //if yes, add to car prior grid
                    //else check if car lies in neighboring grids
                        //if yes, addd to car neighbor grid
                }

                else if ((testDateTime - avlDateTime[counter]).TotalSeconds < 0 && Math.Abs((testDateTime - avlDateTime[counter]).TotalSeconds) < 3600 * 4)
                {
                    //check if car lies in grid
                    //if yes, add to car prior grid
                    //else check if car lies in neighboring grids
                    //if yes, addd to car neighbor grid
                    
                }

                
            }
            DateTime end = DateTime.Now;
            var runTime = (end - start).TotalSeconds;
            Console.Write(runTime);
            Console.ReadLine();

        }

        
    }
}
