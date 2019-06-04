using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{

    //clase Base
    abstract class Bootstrapper
    {

        public Dictionary<double, double> curve = new Dictionary<double, double>();

        public abstract Dictionary<double, double> getCurve(DateTime idate, Dictionary<DateTime, Dictionary<string, Double>> data, Dictionary<double, double> curve);

    }


    //Short Part
    class ShortBT : Bootstrapper
    {

        public override Dictionary<double, double> getCurve(DateTime idate, Dictionary<DateTime, Dictionary<string, Double>> data, Dictionary<double, double> curve)
        {

            foreach (var c in data[idate])
            {

                if (c.Key == "ON"|| c.Key == "TN")
                {

                    var fdate = Helper.getTenor(idate, c.Key);
                    var totalDays = (fdate - idate).TotalDays;
                    var rate = Helper.linealActual360(idate, fdate, c.Value);



                    curve.Add(totalDays, rate);

                }

                else if (c.Key == "3M" || c.Key == "6M"|| c.Key == "9M" || c.Key == "12M")
                {

                    var fdate = Helper.getTenor(idate, c.Key);
                    var totalDays = (fdate - idate.AddDays(2)).TotalDays;
                    var rate = Helper.linealActual360(idate.AddDays(2), fdate, c.Value);

                    curve.Add(totalDays, rate);

                }

                else if (c.Key == "2Y")
                {

                    break; // get out of the loop

                }

                else
                {
                    Console.WriteLine("ERROR - Tenor not recognized: {0} ",c.Key);
                    Console.ReadLine();
                    System.Environment.Exit(1);
                }

            }

            return curve;

        }

    }
    

    //Long part
    class longBT:Bootstrapper
    {
               
        public override Dictionary<double, double> getCurve(DateTime idate, Dictionary<DateTime, Dictionary<string, Double>> data, Dictionary<double, double> curve)
        {

            foreach (var c in data[idate])
            {
                
                if (c.Key == "2Y" || c.Key == "3Y" || c.Key == "4Y" || c.Key == "5Y" || c.Key == "10Y")
                {

                    Dictionary<double, double> auxFix = new Dictionary<double, double>();//Diccioario aux de pata fija
                    Dictionary<double, double> auxDf = new Dictionary<double, double>();//Store all Discount Factors from all coupons
                    
                    var fDate = Helper.getTenor(idate, c.Key);//Calcula la fecha final del SWAP
                    double rate = c.Value;//Rate del Tenor
                     
                    //calculo de numero de cupones
                    #region
                    int totalMoths = Convert.ToInt32(Math.Round(fDate.Subtract(idate).Days / (365.25 / 12)));//Calcula el numero de meses de vida del SWAP
                    int monthsUntilCoupon = 6; // Numero de Cupones por año 
                    int totalCoupons = (totalMoths / monthsUntilCoupon);//Cuantos Cupones pega en total
                    #endregion

                    for (int i = 1; i < totalCoupons+1; i++)
                    {

                        DateTime iDateCoupon = Helper.bussDay(idate.AddDays(2).AddMonths((i - 1) * monthsUntilCoupon));
                        DateTime fDateCoupon = Helper.bussDay(idate.AddDays(2).AddMonths(i * monthsUntilCoupon));

                        double coupondsTotaldays = (fDateCoupon - idate.AddDays(2)).TotalDays;
                        var fixpartCoupon = Helper.couponFixPart(iDateCoupon, fDateCoupon, rate);
                     
                        auxFix.Add(coupondsTotaldays, fixpartCoupon);
                        auxDf.Add(coupondsTotaldays, 0);
                                                
                    }
                    if (c.Key=="3Y")
                    {

                    }
                    var aaa =Helper.minimisation(1,0,0.000000000001,1000000,rate,idate,fDate,auxFix,auxDf,curve);

                }

            }

            return curve;

        }

    }
     
}
