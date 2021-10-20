using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Figgle;

namespace ConsoleApp2
{
    internal static class Launcher
    {
        #region init

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCP(uint wCodePageID);

        public static GameOptions CurrentGameOptions { get; set; }
        public static GameOptions GlobalGameOptions { get; set; }

#if DEBUG
        public static bool ClearConsole = false;
        public static bool _Debug = true;
#else
        public static bool ClearConsole = true;
        public static bool _Debug = false;
#endif

        private static Board _board;

        #endregion

        private static void Main(string[] args)
        {
            // Enable unicode in console
            SetConsoleOutputCP(65001);
            SetConsoleCP(65001);

            GlobalGameOptions = new GameOptions();

            while (true)
            {
                var cmd = Helpers.Input("> ");
                RunCommand(cmd);
            }

#if !DEBUG
            Console.Read();
#endif
        }

        private static void PrintConfig(GameOptions conf, int indent = 0, bool showNumbers = false)
        {
            string intendStr = " ".Mult(indent);
            Console.WriteLine($"{intendStr}{(showNumbers ? "1." : "")} KnightMark: {conf.KnightMark}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "2." : "")} QueenMark: {conf.QueenMark}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "3." : "")} ReverseRoles: {conf.ReverseRoles}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "4." : "")} DarkTeamColor: {conf.DarkTeamColor}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "5." : "")} LightTeamColor: {conf.LightTeamColor}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "6." : "")} ForceJumps: {conf.ForceJumps}");
            Console.WriteLine($"{intendStr}{(showNumbers ? "7." : "")} QueenGoesFirst: {conf.QueenGoesFirst}");
        }

        private static void RunCommand(string full)
        {
            // Found in post "https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes"
            string[] args = full.Split('"')
                .Select((element, index) =>
                    index % 2 == 0
                        ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        : new[] { element }).SelectMany(element => element).ToArray();

            if (args.Length == 0)
                return;
            var cmd = args[0];

            (cmd switch
            {
                "new" => (Action)RunGame,
                "help" => PrintHelp,
                "showconf" => () => PrintConfig(GlobalGameOptions, 2),
                _ => () => Helpers.Error("Invalid command"),
            })();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands available: ");
            Helpers.WriteLineIndent("\"new\" - start new game", 4);
            Helpers.WriteLineIndent("\"help\" - show this help message", 4);
            Helpers.WriteLineIndent("\"showconf\" - show global config", 4);
        }

        private static void UpdateConfig(GameOptions conf, string endKeyword, string query, ConsoleColor? back = null,
            ConsoleColor? fore = null)
        {
            // bool CustomConvert<T>(string input, ref T targetVar, bool enumIgnoreCase = true)
            // {
            //     bool res;
            //     T target = default(T);
            //
            //     res = typeof(T) switch
            //     {
            //         { IsEnum: true, IsValueType: true } t => ((Func<bool>)(() =>
            //         {
            //             MethodInfo tryParseMethod = typeof(Enum).GetGenericMethod("TryParse",
            //                 new[] { typeof(Type), typeof(String), typeof(Boolean), typeof(Object).MakeByRefType() });
            //             object[] args = { typeof(T), input, enumIgnoreCase, default(T) };
            //             bool result = (bool)tryParseMethod.Invoke(null, args);
            //             if (result)
            //                 target = (T)args[3];
            //             return result;
            //         }))(),
            //         _ => ((Func<bool>)(() =>
            //         {
            //             Helpers.Error("Invalid type");
            //             return false;
            //         }))()
            //     };
            //
            //     targetVar = target;
            //     return res;
            // }

            bool CustomConvert<T>(string input, ref T targetVar)
            {
                try
                {
                    targetVar = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
                }
                catch (Exception ex) when (ex is FormatException or NotSupportedException)
                {
                    return false;
                }

                return true;
            }

            bool EditValue<T>(ref T target, string name)
            {
                bool res;
                if (!(res = CustomConvert(Helpers.Input($"Edit value for \"{name}\" [{typeof(T).Name}]: "),
                    ref target)))
                {
                    Helpers.Error("Invalid value");
                    if (typeof(T).IsEnum)
                        Helpers.WriteLineIndent(
                            $"Valid values are: \"{string.Join("\", \"", Enum.GetNames(typeof(T)))}\"", 4);
                }

                return res;
            }

            // Get response from user
            string response = Helpers.Input(query, back, fore);
            // Loop until user decides to stop editing config
            while (response.ToLower() != endKeyword)
            {
                if (response.ToLower() == "show")
                {
                    PrintConfig(conf, 4, true);
                    response = Helpers.Input(query, back, fore);
                }
                else
                {
                    var isInt = int.TryParse(response, out var itemIndex);
                    ((isInt, itemIndex) switch
                    {
                        // Check for value from user
                        (true, >= 1 and <= 7) => (Action)(() =>
                        {
                            (itemIndex switch
                            {
                                1 => () => { EditValue(ref conf.KnightMark, "KnightMark"); },
                                2 => () => { EditValue(ref conf.QueenMark, "QueenMark"); },
                                3 => () => { EditValue(ref conf.ReverseRoles, "ReverseRoles"); },
                                4 => () => { EditValue(ref conf.DarkTeamColor, "DarkTeamColor"); },
                                5 => () => { EditValue(ref conf.LightTeamColor, "LightTeamColor"); },
                                6 => () => { EditValue(ref conf.ForceJumps, "ForceJumps"); },
                                7 => (Action)(() => { EditValue(ref conf.QueenGoesFirst, "QueenGoesFirst"); }),
                                _ => () => Helpers.Error("Invalid, should be impossible")
                            })();
                        }),
                        (true, _) => () => { Helpers.Error("Index not in range 1-7"); },
                        (false, _) => () => { Helpers.Error("Not a valid integer"); }
                    })();
                    response = Helpers.Input(query, back, fore);
                }
            }
        }

        private static void RunGame()
        {
            var config = (GameOptions)GlobalGameOptions.Clone();
            CurrentGameOptions = config;
            UpdateConfig(config, "start",
                "Enter \"start\" for new game, \"show\" for printing config or number of config item to change for this game: ");

            var gameEnded = false;
            _board = new Board();

            #region debug

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

            #endregion

            while (!gameEnded)
            {
                if (ClearConsole)
                    Console.Clear();
                _board.PrintBoard();
                _board.UserMove(CurrentGameOptions.ForceJumps, CurrentGameOptions.QueenGoesFirst, ClearConsole, _Debug);

                var canContinue = _board.CanContinue(_board.WhitePlaying);
                if (!canContinue)
                {
                    if (ClearConsole)
                        Console.Clear();

                    Helpers.ColoredWriteLine(
                        FiggleFonts.Banner.Render((_board.WhitePlaying
                                                      ? Enum.GetName(typeof(ConsoleColor),
                                                          CurrentGameOptions.DarkTeamColor) ?? "Red"
                                                      : Enum.GetName(typeof(ConsoleColor),
                                                          CurrentGameOptions.LightTeamColor) ?? "White") +
                                                  " Wins"), null,
                        _board.WhitePlaying ? CurrentGameOptions.DarkTeamColor : CurrentGameOptions.LightTeamColor);

                    gameEnded = true;
                }
            }
        }

        public class GameOptions : ICloneable
        {
            public bool ReverseRoles;
            public char KnightMark = '♂';
            public char QueenMark = '♀';

            public ConsoleColor DarkTeamColor = ConsoleColor.Red;
            public ConsoleColor LightTeamColor = ConsoleColor.White;

            public bool ForceJumps = true;
            public bool QueenGoesFirst = true;

            public object Clone() =>
                new GameOptions
                {
                    ReverseRoles = ReverseRoles, KnightMark = KnightMark, QueenMark = QueenMark,
                    DarkTeamColor = DarkTeamColor, LightTeamColor = LightTeamColor, ForceJumps = ForceJumps,
                    QueenGoesFirst = QueenGoesFirst
                };
        }
    }
}