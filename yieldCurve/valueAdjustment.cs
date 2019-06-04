using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{
    class valueAdjustment
    {

        public static Dictionary<string, double> adjustedValue(DateTime idate, Dictionary<DateTime, Dictionary<string, Double>> data, Dictionary<Double, Double> zeroCoupon,double youCreditRate,double meCreditRate)
        {
            Dictionary<string, double> adjustedValue = new Dictionary<string, double>();//Diccioario aux de pata fija

            foreach (var c in data[idate])
            {


                if (c.Key == "ON" || c.Key == "TN")
                {

                    var rate = c.Value;
                    var fdate = Helper.getTenor(idate, c.Key);
                    var totalDays = (fdate - idate).TotalDays;

                    var youRiskFactor = Helper.creditDiscountFactor(youCreditRate, totalDays, totalDays);
                    var meRiskFactor = Helper.creditDiscountFactor(meCreditRate, totalDays, totalDays);

                    
                    var flow =(totalDays*rate/360)- ((1 / zeroCoupon[totalDays]) - 1);

                    var value = (Math.Max(flow, 0) * youRiskFactor + Math.Min(flow, 0) * meRiskFactor);


                    adjustedValue.Add(c.Key, value);

                }

                else if (c.Key == "3M" || c.Key == "6M" || c.Key == "9M" || c.Key == "12M")
                {

                    var rate = c.Value;
                    var fdate = Helper.getTenor(idate, c.Key);
                    var totalDays = (fdate - idate.AddDays(2)).TotalDays;

                    var youRiskFactor = Helper.creditDiscountFactor(youCreditRate, totalDays, totalDays);
                    var meRiskFactor = Helper.creditDiscountFactor(meCreditRate, totalDays, totalDays);
                    
                    var flow = ((totalDays * rate / 360) - ((1/zeroCoupon[totalDays])-1))* zeroCoupon[totalDays];

                    var value = (Math.Max(flow, 0) * youRiskFactor + Math.Min(flow, 0) * meRiskFactor);


                    adjustedValue.Add(c.Key, value);

                }

                if (c.Key == "2Y" || c.Key == "3Y" || c.Key == "4Y" || c.Key == "5Y" || c.Key == "10Y")
                {

                    Dictionary<double, double> auxFix = new Dictionary<double, double>();//Diccioario aux de pata fija
                    Dictionary<double, double> auxFloat = new Dictionary<double, double>();//Diccioario aux de pata flotante
                    Dictionary<double, double> auxFlow = new Dictionary<double, double>();//Diccioario aux de los flujos de cada periodo

                    var fDate = Helper.getTenor(idate, c.Key);//Calcula la fecha final del SWAP
                    double rate = c.Value;//Rate del Tenor

                    //calculo de numero de cupones
                    #region
                    int totalMoths = Convert.ToInt32(Math.Round(fDate.Subtract(idate).Days / (365.25 / 12)));//Calcula el numero de meses de vida del SWAP
                    int monthsUntilCoupon = 6; // Numero de Cupones por año 
                    int totalCoupons = (totalMoths / monthsUntilCoupon);//Cuantos Cupones pega en total
                    #endregion

                    //calcula la fecha final de cada cupon y pata fija y pata flotante
                    Double a = 1;
                    for (int i = 1; i < totalCoupons + 1; i++)
                    {

                        DateTime iDateCoupon = Helper.bussDay(idate.AddDays(2).AddMonths((i - 1) * monthsUntilCoupon));
                        DateTime fDateCoupon = Helper.bussDay(idate.AddDays(2).AddMonths(i * monthsUntilCoupon));
                        double coupondsTotaldays = (fDateCoupon - idate.AddDays(2)).TotalDays;
                        
                        //Pata fija
                        var fixpartCoupon = Helper.couponFixPart(iDateCoupon, fDateCoupon, rate);
                        auxFix.Add(coupondsTotaldays, fixpartCoupon);

                        //Pata Flotante
                        Double df = Helper.logLinInterpol(zeroCoupon, coupondsTotaldays);
                        auxFloat.Add(coupondsTotaldays,(a/df)-1);
                        a = df;

                        //Flujo
                        auxFlow.Add(coupondsTotaldays, auxFix[coupondsTotaldays] - auxFloat[coupondsTotaldays]);

                    }


                    //calculo del ajuste

                    Dictionary<double, double> auxAdjustedCurve = new Dictionary<double, double>();//Diccioario conn los flujos ajustados


                    
                    double zeroCurveBefore = 0;
                    double valueBefore = 0;

                    double youDiscountBefore = 0;
                    double meDiscountBefore = 0;

                    double youRiskFactor = 0;
                    double meRiskFactor = 0;

                    

                    double totaldays = 0;

            


                    foreach (var g in auxFlow.Reverse())
                    {
                        


                        if (g.Key==auxFlow.Reverse().First().Key)
                        {

                            auxAdjustedCurve.Add(g.Key, g.Value);
                            
                            valueBefore = auxAdjustedCurve[g.Key];
                            zeroCurveBefore = Helper.logLinInterpol(zeroCoupon, g.Key);

                            

                            youDiscountBefore = Helper.creditDiscountFactor(youCreditRate, totaldays, g.Key);
                            meDiscountBefore = Helper.creditDiscountFactor(meCreditRate, totaldays, g.Key);
                            
                        }


                        else
                        {
                            
                            youRiskFactor = youDiscountBefore / Helper.creditDiscountFactor(youCreditRate, totaldays, g.Key);
                            meRiskFactor = meDiscountBefore / Helper.creditDiscountFactor(meCreditRate, totaldays, g.Key);

                            auxAdjustedCurve.Add(g.Key, g.Value + ( zeroCurveBefore/ Helper.logLinInterpol(zeroCoupon, g.Key)) * (Math.Max(valueBefore, 0) * youRiskFactor + Math.Min(valueBefore, 0) * meRiskFactor));



                            if (g.Key == auxFlow.Reverse().Last().Key)
                            {

                                adjustedValue.Add(c.Key, auxAdjustedCurve[g.Key]);

                                //if (c.Key == "2Y" & idate == DateTime.Parse("2013 / 10 / 7"))
                                //{
                                //    var asdasdasd = 2;
                                //}

                            }

                            valueBefore = auxAdjustedCurve[g.Key];
                            zeroCurveBefore = Helper.logLinInterpol(zeroCoupon, g.Key);
                            youDiscountBefore = Helper.creditDiscountFactor(youCreditRate, totaldays, g.Key);
                            meDiscountBefore = Helper.creditDiscountFactor(meCreditRate, totaldays, g.Key);

                           

                        }

                    }
                    
                }

            }

            return adjustedValue;

        }

    }

}
