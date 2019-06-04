using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yieldCurve
{
    class curveGenerator
    {

        public static Dictionary<double, double> zeroCoupon(DateTime iDate, Dictionary<DateTime, Dictionary<string, Double>> data)
        {

            Dictionary<double, double> curve = new Dictionary<double, double>();

            ShortBT myZeroCouponCurveShort = new ShortBT();
            myZeroCouponCurveShort.getCurve(iDate, data, curve);

            longBT myZeroCouponCurveLong = new longBT();
            myZeroCouponCurveLong.getCurve(iDate, data, curve);

            return curve;

        }






    }
}
