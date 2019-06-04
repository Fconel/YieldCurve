using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{
    class objetiveBootstrapping
    {

        double rate;
        DateTime initialDate;
        DateTime finalDate;
        Dictionary<double, double> fixAux;
        Dictionary<double, double> auxDf;
        Dictionary<double, double> df;

        public objetiveBootstrapping(Dictionary<double,double> _fixAux, Dictionary<double, double> _auxDf, Dictionary<double, double> _df, DateTime _iDate,DateTime _fDate, double _rate)
        {

            this.fixAux = _fixAux;
            this.auxDf = _auxDf;
            this.df = _df;
            this.rate = _rate;
            this.initialDate = _iDate;
            this.finalDate = _fDate;

        }

        public double result(double var)
        {

            var totalDays= (finalDate - initialDate.AddDays(2)).TotalDays;
            df[totalDays] = var;
                        
            foreach (var c in fixAux)//crea los descuentos de cada cupon
            {

                var dfInterpol= Helper.logLinInterpol(df, c.Key);
                auxDf[c.Key] = dfInterpol;

            }

            double store = 0;
            foreach (var c in fixAux)//sumaproducto de los factoresde descuento de los cupones y la pata fija
            {

                store += auxDf[c.Key] * fixAux[c.Key];

            }

            return 1 - store - auxDf[totalDays];

        }

    }
    
}


