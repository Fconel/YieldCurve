using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace yieldCurve
{
    class Program
    {
        static void Main(string[] args)
        {

            var iDate = DateTime.Parse("2013/10/7");//Dummy initial date

            var Dictionary = Helper.dataToDictionaryExcel(Helper.retriveDataExcel("data.xls"));

            //var curve = curveGenerator.zeroCoupon(iDate,Dictionary);
            //var adsdfsdf = valueAdjustment.adjustedValue(iDate, Dictionary, curve, 0.005, 0.005);


            var rateAdjusted = rateAdjustment.rateAdjLinear(Dictionary);




            Helper.dataToCSV(rateAdjusted);




            Console.Clear();
            Console.WriteLine("Curvas generadas en dataAdjusted.csv");
            Console.ReadKey();
        }

    }

}

