using System;
using System.Linq;

public static class Utils
{
    public static readonly Random RNG = new Random();

    public static string GenerateRandomAlphaNumericString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[RNG.Next(s.Length)]).ToArray());
    }

    public static void Times(this int times, Action action)
    {
        for (int i = 0; i < times; ++i)
        {
            action();
        }
    }
}
