using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace yieldCurve
{
    static class Helper

    {
        
        // Move date to next bussness Day
        public static DateTime bussDay(DateTime date)

        {

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {

                date = date.AddDays(2);


            }

            if (date.DayOfWeek == DayOfWeek.Sunday)

            {

                date = date.AddDays(1);

            }

            return date;

        }

        //Get Tenor final date
        public static DateTime getTenor(DateTime iDate, string tenor)
        {
            
            switch (tenor)
            {
                case "ON":
                    //Console.WriteLine(bussDay(iDate.AddDays(1))+" ON"+" Para la fecha "+iDate);
                    return bussDay(iDate.AddDays(1));
                    
                case "TN":
                    if (iDate.DayOfWeek==DayOfWeek.Friday)
                    {
                        //Console.WriteLine(iDate.AddDays(2).AddDays(1)+ " TN Viernes");
                        return bussDay(iDate.AddDays(2)).AddDays(1);

                    }
                    else
                    {
                        //Console.WriteLine((iDate.AddDays(2))+ " TN No viernes");
                        return bussDay(iDate.AddDays(2));

                    }
                case "3M":
                    return bussDay(iDate.AddDays(2).AddMonths(3));
                case "6M":
                    return bussDay(iDate.AddDays(2).AddMonths(6));
                case "9M":
                    return bussDay(iDate.AddDays(2).AddMonths(9));
                case "12M":
                    return bussDay(iDate.AddDays(2).AddMonths(12));
                case "2Y":
                    return bussDay(iDate.AddDays(2).AddMonths(24));
                case "3Y":
                    return bussDay(iDate.AddDays(2).AddMonths(36));
                case "4Y":
                    return bussDay(iDate.AddDays(2).AddMonths(48));
                case "5Y":
                    return bussDay(iDate.AddDays(2).AddMonths(60));
                case "10Y":
                    return bussDay(iDate.AddDays(2).AddMonths(120));
                default:
                    Console.WriteLine("Tenor not recognised - getTenor Method");
                    Console.ReadLine();
                    System.Environment.Exit(1);
                    return DateTime.Parse("0/0/0/");
            }
        }

        //Credit discount factor for Valuation
        public static double creditDiscountFactor(double riskRate,double totalDays, double days)
        {
            
         
            return Math.Exp(-riskRate * days / 360);


        }
        

        //Discount Factor Lineal Actual 360 Helper Method
        public static double couponFixPart(DateTime iDate, DateTime fDate, double rate)
        {

            return rate * ((fDate - iDate).TotalDays / 360);
                    
        }

        //Discount Factor Lineal Actual 360 Helper Method
        public static double linealActual360(DateTime initial_date, DateTime final_date, double rate)
        {

            var discountFactor = (1 / (((final_date - initial_date).TotalDays / 360) * rate + 1));

            return discountFactor;

        }

        //Discount Factor Compound Actual 360 Helper Method
        public static double compoundActual360(DateTime initial_date, DateTime final_date, double rate)
        {

            var discountFactor = 1 / (1 + Math.Pow(rate, ((final_date - initial_date).TotalDays / 360)));

            return discountFactor;

        }

        //Interpolator scalar
        public static double logLinInterpol(double x0, double y0, double x1, double y1, double x)
        {

            //x0 = Math.Log(x0);
            //x1 = Math.Log(x1);
            //y0 = Math.Log(y0);
            //y1 = Math.Log(y1);
            //x = Math.Log(x);
            
            var result = y0 * (x - x1) / (x0 - x1) + y1 * (x - x0) / (x1 - x0);
            //result = Math.Exp(result);

            return result;

        }
        
        //Interpolator dictionary
        public static double logLinInterpol(Dictionary<double,double> data, double variable)
        {

            var temp = data.Keys.Zip(data.Keys.Skip(1), (a, b) => new { a, b }).Where(x => x.a <= variable && x.b >= variable)
         .FirstOrDefault();

            return logLinInterpol(temp.a, data[temp.a], temp.b, data[temp.b], variable);

        }

        //Retrive data from Excel to a DataTable
        static public DataTable retriveDataExcel(string dataFileName)
        {
            var fileName = string.Format(dataFileName);
            var connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", fileName);
            var adapter = new OleDbDataAdapter("SELECT * FROM [data$]", connectionString);
            var ds = new DataSet();

            adapter.Fill(ds, "values");
            DataTable data = ds.Tables["values"];
            var dataFromExcel = ds.Tables["values"];

            return dataFromExcel;

        }

        //Add data to dictionary from a DataTable
        static public Dictionary<DateTime, Dictionary<string, Double>> dataToDictionaryExcel(DataTable data)
        {

            Dictionary<DateTime, Dictionary<string, Double>> Dictionary = new Dictionary<DateTime, Dictionary<string, Double>>();

            var arrayNames = (from DataColumn x in data.Columns
                              select x.ColumnName).ToArray().Skip(1);

            var data2 = data.AsEnumerable();

            foreach (var c in data2)
            {

                Dictionary.Add(DateTime.Parse(c[0].ToString()), new Dictionary<string, Double>());

                int i = 0;
                foreach (var d in arrayNames)
                {
                    Dictionary[DateTime.Parse(c[0].ToString())].Add(d, Convert.ToDouble(c[i + 1]));
                    i++;
                }

            }

            return Dictionary;
        }

        //Minimisation for bootstrapping
        public static double minimisation( double upperEndPoint, double lowerEndPoint, 
            double tolerance, double maxIterations,double rate, DateTime iDate,DateTime fDate,
            Dictionary<double,double> auxFix, Dictionary<double, double> auxDf, Dictionary<double, double> curve)

        {

            objetiveBootstrapping objetiveFuction = new objetiveBootstrapping(auxFix,auxDf, curve, iDate,fDate, rate);

            int n = 0;
            while (n++ < maxIterations)
            {
                var Midpoint = (upperEndPoint + lowerEndPoint) / 2;
                double tol = (upperEndPoint - lowerEndPoint) / 2;
                

                if (tol <= tolerance || objetiveFuction.result(Midpoint)==0 )
                {
                    return Midpoint;

                }


                if (Math.Sign(objetiveFuction.result(Midpoint)) == Math.Sign(objetiveFuction.result(lowerEndPoint)))
                {
                    lowerEndPoint = Midpoint;
                }
                else
                {
                    upperEndPoint = Midpoint;
                }

                


            }
            Console.WriteLine("Minimization Function - Max iterations Exceded");
            Console.ReadKey();
            return 0;

        }

        
        
        //Minimisation for Adjusted rate
        public static double minimisation(double upperEndPoint, double lowerEndPoint,
            double tolerance, double maxIterations, DateTime date ,string tenor, Dictionary<DateTime, Dictionary<string, double>> dataAdjusted, double CompareTo)

        {

            objetiveRateAdjustment target = new objetiveRateAdjustment(date, tenor, dataAdjusted, CompareTo);

            int n = 0;

           
            while (n++ < maxIterations)
            {

                var Midpoint = (upperEndPoint + lowerEndPoint) / 2;
                double tol = (upperEndPoint - lowerEndPoint) / 2;


                if (tol <= tolerance || target.result(Midpoint) == 0)
                {
                    return Midpoint;

                }


                if (Math.Sign(target.result(Midpoint)) == Math.Sign(target.result(lowerEndPoint)))
                {
                    lowerEndPoint = Midpoint;
                }
                else
                {
                    upperEndPoint = Midpoint;
                }

            }
            Console.WriteLine("Minimization Function - Max iterations Exceded");
            Console.ReadKey();
            return 0;

        }



        //Export adjusted data  to CSV
        static public double dataToCSV(Dictionary<DateTime, Dictionary<string, Dictionary<string, double[]>>> adjData)
        {

            var csv = new StringBuilder();
            var newLine = String.Join(",", "Date" + "," + "Credit rating" + "," + "TENOR" + "," + "Unadjusted Value" + "," + "YOU Adjusted Value"
                + "," + "ME Adjusted Value" + "," + "Value Difference" + "," + "Unadjusted Rate" + "," + "Adjusted Rate" + "," + "Rate Diference");

            csv.AppendLine(newLine);

            foreach (var g in adjData.Keys)
            {

                foreach (var r in adjData[g].Keys)
                {

                newLine = string.Join(Environment.NewLine, adjData[g][r].Select(d => g.ToShortDateString() + "," + r + "," + d.Key + "," + d.Value[0] + 
                "," + d.Value[1] +"," + d.Value[2] + "," + (d.Value[0] - d.Value[1]) + "," + d.Value[3] + ","+adjData[g][r][d.Key][2]
                +","+ ("resta")));

                csv.AppendLine(newLine);

                }



            }
            File.WriteAllText("dataAdjusted.csv", csv.ToString());

            

            return 0;


        }




    }

}
