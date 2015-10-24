using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
//using System.Data;

namespace avlData
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //DataTable dataTable = parseCSVToDataTable();
            //dataTable.DefaultView.Sort = "Datetime ASC";
            //dataTable = dataTable.DefaultView.ToTable();

            List<DateTime> avlDateTime = new List<DateTime>();
            List<String> carNumber = new List<String>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<List<double>> resultList = new List<List<double>>();
            /*
            foreach (DataRow row in dataTable.Rows)
            {
                //Console.Write(row["Datetime"]);
                avlDateTime.Add(Convert.ToDateTime(row["Datetime"]));
                x.Add(Convert.ToDouble(row["x"]));
                y.Add(Convert.ToDouble(row["y"]));
                carNumber.Add(Convert.ToString(row["Car Number"]));                
            }

            //iterate through the datatable and fill up the lists. data table are expensive
			//Final List to store results
			
            //Learn how to maintain either a 2D list with different datatype columns. Or else, must iterate over the csv files in a way that dates are sorted by default 
            //get list of avl csv files
             * */
            string[] dirs = Directory.GetFiles(@"/Users/ayanmukhopadhyay/Documents/Vanderbilt/CERL/SurvivalAnalysis/spatioTemporalModelingUpdatedGMM/avl/statePlaneCSV/batch1/");
            //string[] dirs = Directory.GetFiles(@"D:\Vanderbilt\CERL\Data\AVL\StatePlane");
            for (int counterFile = 0; counterFile < dirs.Count(); counterFile++)
            
            //for (int counterFile = 0; counterFile < 4; counterFile++)
            {
                String filenameCurr = dirs[counterFile];
                if (filenameCurr.Contains(".csv"))
                {
                    int counterLine = 0;
                    var reader = new StreamReader(File.OpenRead(filenameCurr));
                    Console.Write("\n" + filenameCurr);
                    var lineCount = File.ReadLines(filenameCurr).Count();
                   //Console.Write(lineCount);

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
                            counterLine++;
                        }
                        catch (System.FormatException e)
                        {
                            continue;
                        }
                    }

					Console.Write("\n"+Convert.ToString(avlDateTime.Count));
                }

            }
			Console.Write("\n The size of AVL Data generated is  : " + Convert.ToString(avlDateTime.Count));
            
			//Finished generating AVL Data. Now get details for the grids
            //vars
            //double carCountPrior;
            //double carCountPost;
            //double carCountNeighbourPrior;
            //double carCountNeighbourPost;

            Console.Write("Loaded Data Into Memory");
            double gridSize = 1609.34 * 2;
            List<double> survAnalysisRows = new List<double>();
            //read the grid inputs
            //var readerGridInput = new StreamReader(File.OpenRead(@"C:\Users\VAIO\Documents\Visual Studio 2013\Projects\readAVLDataC-\gridInputs.csv"));
			var readerGridInput = new StreamReader(File.OpenRead(@"/Users/ayanmukhopadhyay/Documents/Vanderbilt/CERL/C#/readAVLDataC-/gridInputs.csv"));
            int gridCounter = 1;
            while (!readerGridInput.EndOfStream)
            {
                Console.WriteLine(gridCounter);
                var line = readerGridInput.ReadLine(); //this corresponds to a grid data
                var gridData = new List<String>(line.Split('['));
                //remove the first two elements : blank as the data comes as an array of arrays
                gridData.RemoveAt(1);
                gridData.RemoveAt(0);
                bool counterStartFound = false;
                int counterStart = 0;

                DateTime testStart = DateTime.Now;
                int counterRows = 0;
                //for (String dataRaw in gridData)
				for (int counterRawData =0; counterRawData < gridData.Count; counterRawData++)
				{				
					
                    counterRows++;
					counterStartFound = false;
                    //Test time for one loop                    
                                        
					var data = gridData[counterRawData].Split(',');
                    
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

					//temporary list to store results
					List<double> temp = new List<double>(); 
                    for (int counter = counterStart; counter < avlDateTime.Count(); counter++)
                    {

                        /************PRE POLICE PRESENCE*************/
                        if ((avlDateTime[counter] - testDateTime).TotalSeconds <= 0 && Math.Abs((testDateTime - avlDateTime[counter]).TotalSeconds) < 3600 * 8)
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
                                carsPostGrid.Add(carNumber[counter]);
                            }

                            //else check if car lies in neighboring grids
                            else if ((gridNeighbourXLower < x[counter] && gridNeighbourXUpper > x[counter]) && (gridNeighbourYLower < y[counter] && gridNeighbourYUpper > y[counter]))
                            {
                                //if yes, addd to car neighbor grid
                                carsPostNeighbor.Add(carNumber[counter]);
                            }
                        }
                        //if we have gone ahead in AVL list, break the loop
                        else if ((avlDateTime[counter] - testDateTime).TotalSeconds > 3600*4)
                        {
                            break;
                        }
                    }

					temp.Add(carsPriorGrid.Distinct().Count());
					temp.Add(carsPostGrid.Distinct().Count());
					temp.Add(carsPriorNeighbor.Distinct().Count());
					temp.Add(carsPostNeighbor.Distinct().Count());

					resultList.Add(temp);


                    //Console.Write("The loop breaks at " + Convert.ToString(counter));
                    //Test time for one loop
					/*
                    if (counterRows == 1000)
                    {
						DateTime testEnd = DateTime.Now;
						File.WriteAllLines("testCSharp.txt", resultList.Select(k => string.Join(",", k)));
                        var diff = (testEnd - testStart).TotalSeconds;
                        Console.Write("Time For 1500 Loops is " + Convert.ToString(diff));
                        Console.ReadLine();
                        System.Environment.Exit(1);
                    }
                    */
                }
				String filename = "Csharp" + Convert.ToString(gridCounter) + ".txt";
				File.WriteAllLines(filename, resultList.Select(k => string.Join(",", k)));
				gridCounter++;

            }
        }

		/*
        static DataTable parseCSVToDataTable()
        {
            string[] ColumnNames = null;
            DataTable oDataTable = null;
            string[] dirs = Directory.GetFiles(@"D:\Vanderbilt\CERL\Data\AVL\StatePlane");
            //for (int counterFile = 0; counterFile < dirs.Count(); counterFile++)            
            for (int counterFile = 0; counterFile < 4; counterFile++)
            {
                String filenameCurr = dirs[counterFile];
                #region ifregion
                if (filenameCurr.Contains(".csv"))
                {
                    //initialising a StreamReader type variable and will pass the file location
                    StreamReader oStreamReader = new StreamReader(filenameCurr);
                                                          
                    int RowCount = 0;                    
                    string[] oStreamDataValues = null;
                    //using while loop read the stream data till end
                    while (!oStreamReader.EndOfStream)
                    {
                        String oStreamRowData = oStreamReader.ReadLine().Trim();
                        if (oStreamRowData.Length > 0)
                        {
                            oStreamDataValues = oStreamRowData.Split(',');
                            //Bcoz the first row contains column names, we will poluate 
                            //the column name by
                            //reading the first row and RowCount-0 will be true only once
                            if (RowCount == 0 && counterFile ==0)
                            {
                                
                                RowCount = 1;
                                ColumnNames = oStreamRowData.Split(',');
                                oDataTable = new DataTable();

                                //using foreach looping through all the column names
                                //foreach (string csvcolumn in ColumnNames)
                                for (int counterColumn = 0; counterColumn < ColumnNames.Count(); counterColumn++)
                                {
                                    string csvcolumn = ColumnNames[counterColumn];
                                    if (counterColumn == 0 || counterColumn == 1 || counterColumn == 3)
                                    {
                                        DataColumn oDataColumn = new DataColumn(csvcolumn.ToUpper(), typeof(string));
                                        //adding the newly created column to the table
                                        oDataTable.Columns.Add(oDataColumn);
                                    }

                                    else
                                    {
                                        DataColumn oDataColumn = new DataColumn(csvcolumn.ToUpper(), typeof(DateTime));
                                        //adding the newly created column to the table
                                        oDataTable.Columns.Add(oDataColumn);
                                    }

                                    //setting the default value of empty.string to newly created column
                                    //oDataColumn.DefaultValue = string.Empty;                                
                                }
                            }
                            else
                            {
                                //creates a new DataRow with the same schema as of the oDataTable            
                                DataRow oDataRow = oDataTable.NewRow();

                                //using foreach looping through all the column names
                                for (int i = 0; i < ColumnNames.Length; i++)
                                {
                                    if (i == 2)
                                    {
                                        oDataRow[ColumnNames[i]] = Convert.ToDateTime(oStreamDataValues[i].ToString());
                                    }
                                    else
                                    {
                                        oDataRow[ColumnNames[i]] = oStreamDataValues[i] == null ? string.Empty : oStreamDataValues[i].ToString();
                                    }
                                }

                                //adding the newly created row with data to the oDataTable       
                                oDataTable.Rows.Add(oDataRow);
                            }

                        }
                    }
                    //close the oStreamReader object
                    oStreamReader.Close();
                    //release all the resources used by the oStreamReader object
                    oStreamReader.Dispose();
                    
                }
                #endregion
            }

            return oDataTable;
        }
        */
    }
}
