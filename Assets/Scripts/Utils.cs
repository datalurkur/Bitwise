using System;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Game;

public static class Utils
{
    public static readonly char NonBreakingSpace = '\u00A0';
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

    public static bool IterateAndRemove<T>(this List<T> list, Func<T, bool> action)
    {
        bool itemRemoved = false;
        for (int i = 0; i < list.Count; ++i)
        {
            if (action(list[i]))
            {
                itemRemoved = true;
                list.RemoveAt(i--);
            }
        }
        return itemRemoved;
    }

    public static List<ConsoleHistory.ConsoleEvent> BuildStatusCheck(string target, string statusData, string status, string statusColor, float delaySpeed)
    {
        List<ConsoleHistory.ConsoleEvent> events = new List<ConsoleHistory.ConsoleEvent>()
        {
            new ConsoleHistory.ConsoleEvent(target, false),
            new ConsoleHistory.ConsoleEvent(".", false, delaySpeed),
            new ConsoleHistory.ConsoleEvent(".", false, delaySpeed),
            new ConsoleHistory.ConsoleEvent(".", false, delaySpeed)
        };
        if (!string.IsNullOrEmpty(statusData))
        {
            events.Add(new ConsoleHistory.ConsoleEvent(statusData, false));
        }
        events = events.Concat(BuildStatusBlock(status, statusColor)).ToList();
        events.Add(new ConsoleHistory.ConsoleEvent(""));
        return events;
    }

    public static List<ConsoleHistory.ConsoleEvent> BuildStatusBlock(string status, string color)
    {
        return new List<ConsoleHistory.ConsoleEvent>()
        {
            new ConsoleHistory.ConsoleEvent("[", false),
            new ConsoleHistory.ConsoleEvent(status, false, 0f, null, color),
            new ConsoleHistory.ConsoleEvent("]", false)
        };
    }
}
