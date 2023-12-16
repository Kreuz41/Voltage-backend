namespace FP.Core.Api.Helpers;

public class RangSystem
{
    public static decimal GetRang(int rang)
    {
        return rang switch
        {
            0 => 10000m,
            1 => 20000m,
            2 => 30000m,
            3 => 40000m,
            4 => 50000m,
            5 => 100000m,
            6 => 150000m,
            7 => 200000m,
            8 => 250000m,
            9 => 500000m,
            10 => 650000m,
            11 => 1500000m,
            12 => 2000000m,
            13 => 4000000m,
            14 => 6000000m,
            _ => 0
        };
    }
}