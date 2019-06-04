using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{
    class rateAdjustment
    {

        public static Dictionary<DateTime, Dictionary<string, double>> rateAdj(Dictionary<DateTime, Dictionary<string, double>> Dictionary)
        {

            var dataAdjusted = new Dictionary<DateTime, Dictionary<string, double>>();
            double CompareTo = 0;


            foreach (var g in Dictionary.Keys)
            {
                Console.WriteLine(g);
                var zeroCurve = curveGenerator.zeroCoupon(g, Dictionary);
                var valueWithAdjustment = valueAdjustment.adjustedValue(g, Dictionary, zeroCurve, 0.005, 0.005);
                dataAdjusted.Add(g, new Dictionary<string, double>());


                foreach (var j in Dictionary[g].Keys)
                {

                    dataAdjusted[g].Add(j, 0);

                    CompareTo = valueWithAdjustment[j];



                    Helper.minimisation(1, 0, 0.0000001, 1000000, g, j, dataAdjusted, CompareTo);


                }


            }

            return dataAdjusted;


        }



        public static Dictionary<DateTime, Dictionary<string, Dictionary<string, double[]>>> rateAdjLinear(Dictionary<DateTime, Dictionary<string, double>> Dictionary)
        {

            var dataAdjusted = new Dictionary<DateTime, Dictionary<string, Dictionary<string, double[] >>>();

            var dataAdjustedMinimi = new Dictionary<DateTime, Dictionary<string, double>>();


            var creditRiskRates = new Dictionary<string, double>();
            creditRiskRates.Add("A1", 0.04/100);
            creditRiskRates.Add("A2", 0.1 / 100);
            creditRiskRates.Add("A3", 0.25 / 100);
            creditRiskRates.Add("A4", 2.00 / 100);
            creditRiskRates.Add("A5", 4.75 / 100);
            creditRiskRates.Add("A6", 10.00 / 100);
            creditRiskRates.Add("B1", 15.00 / 100);
            creditRiskRates.Add("B2", 22.00 / 100);
            creditRiskRates.Add("B3", 33.00 / 100);
            creditRiskRates.Add("B4", 45.00 / 100);


            foreach (var g in Dictionary.Keys)
            {

                Console.WriteLine(g);
                dataAdjusted.Add(g, new Dictionary<string, Dictionary<string, double[]>>());
                var zeroCurve = curveGenerator.zeroCoupon(g, Dictionary);

                dataAdjustedMinimi.Add(g, new Dictionary<string, double>());


                foreach (var r in creditRiskRates)
                {


                    dataAdjusted[g].Add(r.Key, new Dictionary<string, double[]>());

                    var YouValueWithAdjustment = valueAdjustment.adjustedValue(g, Dictionary, zeroCurve, r.Value,0);
                    var MeValueWithAdjustment = valueAdjustment.adjustedValue(g, Dictionary, zeroCurve, 0, r.Value);

                    var valueWithOutAdjustment = valueAdjustment.adjustedValue(g, Dictionary, zeroCurve, 0, 0);


                    foreach (var j in Dictionary[g].Keys)
                    {

                        double[] Values = new double[] { valueWithOutAdjustment[j], YouValueWithAdjustment[j], MeValueWithAdjustment[j],Dictionary[g][j] };
                        dataAdjusted[g][r.Key].Add(j, Values);

                        
                        double CompareTo = YouValueWithAdjustment[j];
                            
                        dataAdjustedMinimi[g].Add(j+" "+r.Value.ToString(), 0.5);
                        
                        Helper.minimisation(1, 0, 0.0000001, 1000000,g, j, dataAdjustedMinimi, CompareTo);



                        //CompareTo = valueWithAdjustment[j];

                        var varX1 = Dictionary[g][j];
                        var varY1 = valueWithOutAdjustment[j];

                        var varX2 = Dictionary[g][j];
                        var varY2 = valueWithOutAdjustment[j];

                    }

                }

            }

            return dataAdjusted;


        }









    }

}
