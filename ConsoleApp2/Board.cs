using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;

namespace ConsoleApp2
{
    internal class Board
    {
        public enum MoveResult
        {
            Ok,
            SpotOccupied,
            MoveUnreachable,
            PieceOfOpposingPlayer,
            PieceNull
        }

        public Piece[,] GameBoard = new Piece[8, 8];
        public List<Piece> Pieces = new();
        public bool WhitePlaying = true;
        public List<Piece> WhiteKills = new(), DarkKills = new();
        public (byte? x, byte? y) Selected { get; private set; } = (null, null);

        public Board()
        {
            for (var i = 0; i < 8; i++)
            {
                MakeNewPiece(new PiecePos(i, (i + 1) % 2), false, false);
                if (i % 2 != 0)
                    MakeNewPiece(new PiecePos(i, 2), false, false);

                MakeNewPiece(new PiecePos(i, (i + 1) % 2 + 6), true, false);
                if (i % 2 == 0)
                    MakeNewPiece(new PiecePos(i, 5), true, false);
            }
        }

        public (MoveResult, MoveTree) Move(Piece piece, PiecePos pos)
        {
            if (piece is null)
                return (MoveResult.PieceNull, null);

            if (piece.White != WhitePlaying)
                return (MoveResult.PieceOfOpposingPlayer, null);

            if (GameBoard[pos.X, pos.Y] is not null)
                return (MoveResult.SpotOccupied, null);

            MoveTree userMove, validMoves = piece.FindAllMoves();

            if ((userMove = validMoves.NextMoves.FirstOrDefault(m => m.Pos == pos)) != null)
            {
                SimpleMove(piece, userMove.Pos, userMove.Kill);

                return (MoveResult.Ok, userMove);
            }

            return (MoveResult.MoveUnreachable, null);
        }

        public void UserMove(bool forceJumps = true, bool queenFirst = true, bool clearConsole = false,
            bool debug = false)
        {
            bool moveMore = true;
            MoveTree firstMove = null, move = null;
            bool isFirstMove = true;

            // Loop through all moves player will make
            while (moveMore)
            {
                // Select piece, only during first position selection
                if (firstMove is null)
                {
                    PiecePos piecePos = new PiecePos();
                    bool errorPieceChoosing = true;
                    do
                    {
                        try
                        {
                            if (forceJumps && HasObligatory)
                            {
                                if (queenFirst && QueenHasToKill)
                                    Helpers.ColoredWriteLine(
                                        $"You have to choose QUEEN that can jump over one of these pieces: [{String.Join(", ", QueenKillList)}]");
                                else
                                    Helpers.ColoredWriteLine(
                                        $"You have to choose piece that can jump over one of these pieces: [{String.Join(", ", PieceKillList)}]");
                            }

                            piecePos = new PiecePos(Helpers.Input("Position of piece you wanna move: "));

                            // Check for possible problems with user defined position
                            if (this[piecePos] is null)
                                Helpers.Error("Spot is empty");
                            else if (this[piecePos].White != WhitePlaying)
                                Helpers.Error("Piece is not yours");
                            else
                                switch (forceJumps)
                                {
                                    case true when HasObligatory && queenFirst && QueenHasToKill &&
                                                   this[piecePos] is Queen &&
                                                   !this[piecePos].FindAllMoves().HasObligatory:
                                        Helpers.Error("This queen can't take any of obligatory pieces");
                                        break;
                                    case true when HasObligatory && queenFirst && QueenHasToKill &&
                                                   this[piecePos] is not Queen:
                                        Helpers.Error("This piece is not queen");
                                        break;
                                    case true when HasObligatory && !(queenFirst && QueenHasToKill) &&
                                                   !this[piecePos].FindAllMoves().HasObligatory:
                                        Helpers.Error("This piece can't take any pieces");
                                        break;
                                    default:
                                    {
                                        // Check if selected piece has any moves avalable
                                        var moves = this[piecePos].FindAllMoves();
                                        if (moves.NextMoves.Count == 0)
                                            Helpers.Error("This piece has no valid moves available");
                                        else
                                        {
                                            // Piece selection OK
                                            errorPieceChoosing = false;
                                            firstMove = this[piecePos].FindAllMoves();
                                            move = firstMove;
                                        }

                                        break;
                                    }
                                }
                        }
                        catch (Exception e)
                        {
                            // Error in PiecePos constructor
                            if (debug)
                                Console.WriteLine(e);
                            Helpers.Error("Error, choose correct position");
                            errorPieceChoosing = true;
                        }
                    } while (errorPieceChoosing); // Loop until piece selection is OK

                    // Select piece and print board
                    Select(piecePos.X, piecePos.Y);
                    if (clearConsole)
                        Console.Clear();
                    PrintBoard();
                }

                var movePos = new PiecePos();
                var errorMoveChoosing = true;
                do
                {
                    try
                    {
                        if (forceJumps && move.HasObligatory)
                            Helpers.ColoredWriteLine(
                                $"You have to jump over one of these pieces: [{String.Join(", ", move.AllToKillCandidates)}]");

                        string resp =
                            Helpers.Input(
                                $@"Where to move {firstMove.Piece} piece{
                                    (!forceJumps && !isFirstMove ? " (leave empty for no more moves)" : "")}: ");

                        // Stop this move if player is not forced to play and this is not first move
                        if (!forceJumps && !isFirstMove && resp.Length == 0)
                        {
                            errorMoveChoosing = false;
                            moveMore = false;
                            break;
                        }

                        movePos = new PiecePos(resp);
                    }
                    catch (Exception e)
                    {
                        // Error during creating movePos
                        if (debug)
                            Console.WriteLine(e);
                        Helpers.Error("Choose correct position");
                        continue;
                    }

                    // Check is chosen move is valid
                    if (!move.ValidMove(movePos, forceJumps || !isFirstMove))
                        Helpers.Error("Move is invalid");
                    else // Everything goes well
                    {
                        move = move.MoveToPos(movePos);
                        errorMoveChoosing = false;
                    }
                } while (errorMoveChoosing);

                // Apply move
                SimpleMove(firstMove.Piece, movePos, move.Kill);

                // Clear console and print board
                if (clearConsole)
                    Console.Clear();

                // Disable first move code
                isFirstMove = false;

                // Check if its last possible move, otherwise print board
                if (!move.HasObligatory)
                    moveMore = false;
                else
                    PrintBoard();
            }

            // Deselect pieces and change currently playing player
            Select(null, null);
            WhitePlaying = !WhitePlaying;

            if (firstMove.Piece.IsOnOtherSideEnd() && firstMove.Piece is Knight knight)
            {
                SimpleKill(knight);
                this[knight.Pos] = knight.Upgrade();
                Pieces.Add(this[knight.Pos]);
            }
        }

        #region helper_functions"

        public void SimpleMove(Piece piece, PiecePos pos, Piece kill = null)
        {
            this[piece.Pos.X, piece.Pos.Y] = null;
            this[pos.X, pos.Y] = piece;

            SimpleKill(kill);

            piece.Pos.X = pos.X;
            piece.Pos.Y = pos.Y;
        }

        public void SimpleKill(Piece kill)
        {
            if (kill != null)
            {
                Pieces.Remove(kill);
                this[kill.Pos.X, kill.Pos.Y] = null;
                
                if (kill.White)
                    DarkKills.Add(kill);
                else
                    WhiteKills.Add(kill);
            }
        }

        public Piece this[int x, int y]
        {
            get => GameBoard[x, y];
            set => GameBoard[x, y] = value;
        }

        public Piece this[PiecePos pos]
        {
            get => this[pos.X, pos.Y];
            set => GameBoard[pos.X, pos.Y] = value;
        }

        public bool Select(byte? x, byte? y)
        {
            try
            {
                var pos = new PiecePos(x ?? 0, y ?? 0);
                Selected = (x is null ? null : pos.X, y is null ? null : pos.Y);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public List<Piece> PiecesOfColor(bool white) => Pieces.Where(p => p.White == white).ToList();

        public bool CanContinue(bool white)
        {
            var piecesOfColor = PiecesOfColor(white);
            return piecesOfColor.Count > 0 &&
                   piecesOfColor.FirstOrDefault(p => p.FindAllMoves().NextMoves.Count > 0) != null;
        }

        public bool HasObligatory =>
            PiecesOfColor(WhitePlaying).FirstOrDefault(p => p.FindAllMoves().HasObligatory) != null;

        public bool QueenHasToKill => PiecesOfColor(WhitePlaying).FirstOrDefault(p =>
            p is Queen && p.FindAllMoves().HasObligatory) != null;

        public List<Piece> QueenKillList => PiecesOfColor(WhitePlaying).Where(p => p is Queen)
            .SelectMany(p => p.FindAllMoves().AllToKillCandidates).Distinct().ToList();

        public List<Piece> PieceKillList =>
            PiecesOfColor(WhitePlaying).SelectMany(p => p.FindAllMoves().AllToKillCandidates).Distinct().ToList();

        #endregion

        #region print

        public void PrintBoard()
        {
            PrintPlayingPlayer();
            Console.WriteLine();

            PrintColNames();
            PrintSelectedCol();
            PrintHorLine();

            for (var y = 0; y < GameBoard.GetLength(1); y++)
            {
                PrintRowNames(y);
                for (var x = 0; x < GameBoard.GetLength(0); x++)
                    Helpers.ColoredWrite((GameBoard[x, y]?.Mark.ToString() ?? " ") + " ", null,
                        GameBoard[x, y]?.MarkColor);
                PrintRowNames(y, true);
                Console.WriteLine();
            }

            PrintHorLine(true);
            PrintSelectedCol(true);
            PrintColNames();

            Console.WriteLine();
        }

        private void PrintSelectedCol(bool reverse = false)
        {
            // Print arrows if selecyed
            Console.Write("     " + (Selected.x != null
                ? " ".Mult((int)Selected.x * 2) + (reverse ? "↑" : "↓") + " ".Mult((7 - (int)Selected.x) * 2)
                : " ".Mult(2 * 8)));
            
            // Print ῲ if this player plays
            Helpers.ColoredWrite(!(WhitePlaying ^ reverse) ? "   ῲ" : "    ", null,
                WhitePlaying ? Program.LightTeamColor : Program.DarkTeamColor);
            
            // Print taken pieces
            Helpers.ColoredWriteLine("    " + (reverse
                    ? String.Join("", WhiteKills.Select(p => p.Mark))
                    : String.Join("", DarkKills.Select(p => p.Mark))), null,
                reverse ? Program.LightTeamColor : Program.DarkTeamColor);
        }

        private void PrintColNames()
        {
            Console.Write("     ");
            for (var x = 0; x < GameBoard.GetLength(0); x++)
                Console.Write(new PiecePos(x, 0).ColumnName + " ");
            Console.WriteLine();
        }

        private void PrintHorLine(bool bottom = false)
        {
            Console.WriteLine("   " + (bottom ? "└" : "┌") + "─".Mult(17) + (bottom ? "┘" : "┐"));
        }

        private void PrintRowNames(int y, bool reverse = false)
        {
            void PrintName()
            {
                Console.Write((reverse ? " " : "") + new PiecePos(0, y).RowName + " ");
            }

            if (!reverse) PrintName();
            Console.Write(Selected.y == y ? reverse ? "│←" : "→│ " : reverse ? "│ " : " │ ");
            if (reverse) PrintName();
        }

        private void MakeNewPiece(PiecePos pos, bool white, bool queen)
        {
            var piece = queen ? new Queen(white, pos, Pieces) : (Piece)new Knight(white, pos, Pieces);
            GameBoard[pos.X, pos.Y] = piece;
            Pieces.Add(piece);
        }

        private void PrintPlayingPlayer()
        {
            Helpers.ColoredWrite(WhitePlaying
                    ? $"Player {Enum.GetName(typeof(ConsoleColor), Program.LightTeamColor)} is playing"
                    : $"Player {Enum.GetName(typeof(ConsoleColor), Program.DarkTeamColor)} is playing", null,
                WhitePlaying ? Program.LightTeamColor : Program.DarkTeamColor);
            Console.WriteLine();
        }

        #endregion
    }
}