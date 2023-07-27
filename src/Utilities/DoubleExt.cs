namespace JTIS.Analysis;
using System.Linq;
public static class Extend
{
    public static double StandardDeviation(this IEnumerable<double> values, bool besselsCorrection = false)
    {
        double avg = values.Average();
        if (besselsCorrection == false)
        {
            return Math.Sqrt(values.Average(v=>Math.Pow(v-avg,2)));
        }
        else 
        {
            double besselFactor = Convert.ToDouble(values.Count()) / (Convert.ToDouble(values.Count()) - 1) ;
            return StandardDeviation(values,false) * besselFactor;
        }
    }

    public static double RoundTwo(this double value)
    {
        return Math.Round(value,2);
    }
    public static double AveragesStdErr(this IEnumerable<double> values, double? stdDeviation = null, bool besselsCorrection = false)
    {
        if (stdDeviation == null)
        {
            stdDeviation = StandardDeviation(values);
        }
        if (besselsCorrection==false)
        {
            return stdDeviation.Value/Math.Sqrt(values.Count());
        }
        else 
        {
            return stdDeviation.Value/Math.Sqrt(values.Count()-1);

        }
    }

    
}