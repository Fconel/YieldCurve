using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{
    class objetiveRateAdjustment
    {

        Dictionary<DateTime, Dictionary<string, Double>> data;
        DateTime date;
        double objetive;
        string tenor;
        Dictionary<double, double> curve2;
        Dictionary<string, double> valueWithOutAdjustment;

        public objetiveRateAdjustment(DateTime _date,string _tenor, Dictionary<DateTime, Dictionary<string, Double>> _data,double _objetive)
        {

            this.date = _date;
            this.data = _data;
            this.objetive = _objetive;
            this.tenor = _tenor;
            
        }

        public double result(double var)
        {

            data[date][tenor] = var;

            curve2 = curveGenerator.zeroCoupon(date, data);

            valueWithOutAdjustment = valueAdjustment.adjustedValue(date, data, curve2, 0, 0);

            curve2 = null;
      
            return (valueWithOutAdjustment[tenor]-objetive);

        }

    }

}
