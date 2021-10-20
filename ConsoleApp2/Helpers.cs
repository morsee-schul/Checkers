using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp2
{
    public static class Helpers
    {
        public static void ColoredWrite(object data, ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            ConsoleColor back = Console.BackgroundColor, fore = Console.ForegroundColor;
            Console.BackgroundColor = background ?? Console.BackgroundColor;
            Console.ForegroundColor = foreground ?? Console.ForegroundColor;

            Console.Write(data);

            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
        }

        public static void ColoredWriteLine(object data, ConsoleColor? background = null,
            ConsoleColor? foreground = null)
        {
            ColoredWrite(data, background, foreground);
            Console.WriteLine();
        }

        public static void Error(object data) => ColoredWriteLine(data, null, ConsoleColor.Red);

        public static string Input(object query, ConsoleColor? background = null, ConsoleColor? foreground = null)
        {
            ColoredWrite(query, background, foreground);
            return Console.ReadLine();
        }
    }

    internal static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static string Mult(this string source, int multiplier)
        {
            var sb = new StringBuilder(multiplier * source.Length);
            for (var i = 0; i < multiplier; i++)
                sb.Append(source);

            return sb.ToString();
        }
    }
}