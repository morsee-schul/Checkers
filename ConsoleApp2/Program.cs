using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Figgle;

namespace ConsoleApp2
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCP(uint wCodePageID);

        public static bool ReverseRoles = false;
        public const char KnightMark = '♂';
        public const char QueenMark = '♀';

        public const ConsoleColor DarkTeamColor = ConsoleColor.Red;
        public const ConsoleColor LightTeamColor = ConsoleColor.White;

        public static bool ForceJumps = true;
        public static bool QueenGoesFirst = true;

#if DEBUG
        public static bool ClearConsole = false;
        public static bool _Debug = true;
#else
        public static bool ClearConsole = true;
        public static bool _Debug = false;
#endif

        private static Board _board;

        private static void Main(string[] args)
        {
            // Enable unicode in console
            SetConsoleOutputCP(65001);
            SetConsoleCP(65001);

            PrintConfig();

            var playing = true;
            _board = new Board();

#if DEBUG
            // _board.SimpleMove(_board[new PiecePos("h6")], new PiecePos("b4"));
            // _board.SimpleMove(_board[new PiecePos("h8")], new PiecePos("d4"));
            // _board.SimpleKill(_board[new PiecePos("b6")]);
            // _board.SimpleKill(_board[new PiecePos("a7")]);
            // _board.SimpleKill(_board[new PiecePos("b8")]);
            // _board.SimpleMove(_board[new PiecePos("a3")], new PiecePos("a7"));

            // Console.WriteLine(_board.HasObligatory);
            // for (var i = 0; i < _board.GameBoard.GetLength(0); i++)
            //     for (var j = 0; j <= 2; j++)
            //         _board.SimpleKill(_board[new PiecePos(i, j)]);
#endif

            while (playing)
            {
                if (ClearConsole)
                    Console.Clear();
                _board.PrintBoard();
                _board.UserMove(ForceJumps, QueenGoesFirst, ClearConsole, _Debug);

                var canContinue = _board.CanContinue(_board.WhitePlaying);
                if (!canContinue)
                {
                    if (ClearConsole)
                        Console.Clear();

                    Helpers.ColoredWriteLine(FiggleFonts.Banner.Render((_board.WhitePlaying
                            ? Enum.GetName(typeof(ConsoleColor), DarkTeamColor) ?? "Red"
                            : Enum.GetName(typeof(ConsoleColor), LightTeamColor) ?? "White") + " Wins"), null,
                        _board.WhitePlaying ? DarkTeamColor : LightTeamColor);

                    playing = false;
                }
            }

#if !DEBUG
            Console.Read();
#endif
            //
            // Type type = typeof(FiggleFonts); // MyClass is static class with static properties
            // foreach (var p in type.GetProperties( BindingFlags.Static | BindingFlags.Public))
            // {
            //     var font = (FiggleFont)p.GetValue(null);
            //     Console.WriteLine(p.Name);
            //     Console.WriteLine(font.Render("Red & White"));
            // }
        }

        private static void PrintConfig()
        {
            Console.WriteLine("KnightMark: {0}", KnightMark);
            Console.WriteLine("QueenMark: {0}", QueenMark);
            Console.WriteLine("ReverseRoles: {0}", ReverseRoles);
            Console.WriteLine("DarkTeamColor: {0}", DarkTeamColor);
            Console.WriteLine("LightTeamColor: {0}", LightTeamColor);
            Console.WriteLine("ForceJumps: {0}", ForceJumps);
        }
    }
}