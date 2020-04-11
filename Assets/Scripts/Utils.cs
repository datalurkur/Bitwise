using System;
using System.Collections.Generic;
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

    public static void IterateAndRemove<T>(this List<T> list, Func<T, bool> action)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (action(list[i]))
            {
                list.RemoveAt(i--);
            }
        }
    }
}
