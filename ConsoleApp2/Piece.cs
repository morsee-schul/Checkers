using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    public abstract class Piece
    {
        private readonly PiecePos _startingPos;
        public PiecePos Pos;
        public List<Piece> Pieces;
        public readonly bool White;

        protected Piece(bool white, PiecePos pos, List<Piece> pieces)
        {
            Pos = pos;
            White = white;
            _startingPos = pos;
            Pieces = pieces;
        }

        public static bool IsValidMove(sbyte x, sbyte y)
        {
            return x >= 0 && x <= 7 && y >= 0 && y <= 7;
        }

        public static bool IsValidMove(int x, int y)
        {
            return x >= 0 && x <= 7 && y >= 0 && y <= 7;
        }

        public static bool IsOnOtherSideEnd(PiecePos pos, bool white)
        {
            return !white ? pos.Y >= 7 : pos.Y <= 0;
        }

        public bool IsOnOtherSideEnd()
        {
            return IsOnOtherSideEnd(Pos, White);
        }

        public static bool IsOnOtherSide(PiecePos pos, bool white)
        {
            return !white ? pos.Y > 3 : pos.Y < 4;
        }

        public bool IsOnOtherSide()
        {
            return IsOnOtherSide(Pos, White);
        }

        public abstract char Mark { get; }

        public ConsoleColor MarkColor =>
            White ? Launcher.CurrentGameOptions.LightTeamColor : Launcher.CurrentGameOptions.DarkTeamColor;

        public abstract MoveTree FindAllMoves(List<Piece> pieces = null);

        public static Piece PieceOnSpot(PiecePos pos, List<Piece> pieces)
        {
            return pieces.FirstOrDefault(piece => piece.Pos == pos);
        }

        public override string ToString()
        {
            return (Pos, Mark).ToString();
        }
    }


    public struct PiecePos
    {
        public byte X { get; set; }
        public const string ColumnNames = "ABCDEFGH";
        public const string RowNames = "12345678";
        public byte Y { get; set; }
        public (string col, string row) PosName => (ColumnName, RowName);
        public string ColumnName => ColumnNames[X].ToString();
        public string RowName => (8 - Y).ToString();

        public PiecePos(byte x, byte y)
        {
            if (x > 7 || y > 7)
                throw new ArgumentException("'x' and 'y' have to be in range 0-7 ");
            X = x;
            Y = y;
        }

        public PiecePos(sbyte x, sbyte y)
        {
            if (x > 7 || y > 7)
                throw new Exception("'x' and 'y' have to be in range 0-7 ");
            X = (byte)x;
            Y = (byte)y;
        }

        public PiecePos(int x, int y)
        {
            if (x > 7 || y > 7)
                throw new Exception("'x' and 'y' have to be in range 0-7 ");
            X = (byte)x;
            Y = (byte)y;
        }

        public PiecePos(string s)
        {
            if (s.Length != 2)
                throw new ArgumentException("string 's' must be of length 2");

            if (ColumnNames.Contains(s[0].ToString().ToUpper()) && RowNames.Contains(s[1]))
            {
                X = (byte)ColumnNames.IndexOf(s[0].ToString().ToUpper(), StringComparison.Ordinal);
                Y = (byte)(7 - RowNames.IndexOf(s[1]));
                return;
            }

            throw new ArgumentException(
                "first char of string 's' must be one of 'ABCDEFGH' and second one of '12345678'");
        }

        public static PiecePos FromInput(byte x, byte y)
        {
            if (x > 8 || y > 8 || x < 1 || y < 1)
                throw new Exception("'x' and 'y' have to be in range 1-8 ");
            return new PiecePos((byte)(x - 1), (byte)(y - 1));
        }

        public static bool operator ==(PiecePos pos1, PiecePos pos2)
        {
            return pos1.X == pos2.X && pos1.Y == pos2.Y;
        }

        public static bool operator !=(PiecePos pos1, PiecePos pos2)
        {
            return !(pos1 == pos2);
        }

        public override string ToString()
        {
            return ColumnName + RowName;
        }
    }
}