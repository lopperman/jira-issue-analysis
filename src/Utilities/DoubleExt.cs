namespace JTIS.Analysis;

public static class Extend
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v=>Math.Pow(v-avg,2)));
    }

    public static double AveragesStdErr(this IEnumerable<double> values)
    {
        var stdDev = StandardDeviation(values);
        return stdDev/Math.Sqrt(values.Count());
    }

    
}