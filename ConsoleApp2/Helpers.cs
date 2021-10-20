using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static void WriteLineIndent(object data, uint indent, ConsoleColor? back = null,
            ConsoleColor? fore = null)
        {
            Console.Write(" ".Mult((int)indent));
            ColoredWriteLine(data, back, fore);
        }
    }

    internal static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        public static string Mult(this string source, int multiplier)
        {
            var sb = new StringBuilder(multiplier * source.Length);
            Enumerable.Range(0, multiplier).ForEach(i => sb.Append(source));
            return sb.ToString();
        }
    }


    // Copied from "https://stackoverflow.com/questions/3189861/pass-a-lambda-expression-in-place-of-icomparer-or-iequalitycomparer-or-any-singl"
    /// <summary>
    /// Utility class for creating <see cref="IEqualityComparer{T}"/> instances 
    /// from Lambda expressions.
    /// </summary>
    public static class EqualityComparerFactory
    {
        /// <summary>Creates the specified <see cref="IEqualityComparer{T}" />.</summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="getHashCode">The get hash code delegate.</param>
        /// <param name="equals">The equals delegate.</param>
        /// <returns>An instance of <see cref="IEqualityComparer{T}" />.</returns>
        public static IEqualityComparer<T> Create<T>(
            Func<T, int> getHashCode,
            Func<T, T, bool> equals)
        {
            if (getHashCode == null)
            {
                throw new ArgumentNullException(nameof(getHashCode));
            }

            if (equals == null)
            {
                throw new ArgumentNullException(nameof(equals));
            }

            return new Comparer<T>(getHashCode, equals);
        }

        private class Comparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, int> _getHashCode;
            private readonly Func<T, T, bool> _equals;

            public Comparer(Func<T, int> getHashCode, Func<T, T, bool> equals)
            {
                _getHashCode = getHashCode;
                _equals = equals;
            }

            public bool Equals(T x, T y) => _equals(x, y);

            public int GetHashCode(T obj) => _getHashCode(obj);
        }
    }
}


// From "https://stackoverflow.com/questions/4035719/getmethod-for-generic-method"
public static class TypeExtensions
{
    private class SimpleTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.Assembly == y.Assembly &&
                   x.Namespace == y.Namespace &&
                   x.Name == y.Name;
        }

        public int GetHashCode(Type obj)
        {
            throw new NotImplementedException();
        }
    }

    public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
    {
        var methods = type.GetMethods();
        foreach (var method in methods.Where(m => m.Name == name))
        {
            Console.WriteLine(method);
            var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
            {
                Console.WriteLine("hh");
                return method;
            }
        }

        return null;
    }
}