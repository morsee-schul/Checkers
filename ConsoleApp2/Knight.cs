using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    public class Knight : Piece
    {
        public Knight(bool white, PiecePos pos, List<Piece> pieces) : base(white, pos, pieces)
        {
        }

        public override MoveTree FindAllMoves(List<Piece> pieces = null)
        {
            pieces = pieces ?? Pieces;

            var move = new MoveTree(this, Pos, null, null);

            // Check if there are available simple moves
            var dir = (sbyte)(!White ? 1 : -1);
            if (Pos.X - 1 >= 0 && Pos.Y + dir < 8 && Pos.Y + dir >= 0)
            {
                var movePos = new PiecePos(Pos.X - 1, Pos.Y + dir);
                if (PieceOnSpot(movePos, pieces) is null)
                {
                    var x = new MoveTree(this, movePos, null, move);
                }
            }

            if (Pos.X + 1 < 8 && Pos.Y + dir < 8 && Pos.Y + dir >= 0)
            {
                var movePos = new PiecePos(Pos.X + 1, Pos.Y + dir);
                if (PieceOnSpot(movePos, pieces) is null)
                {
                    var x = new MoveTree(this, movePos, null, move);
                }
            }

            FindKillMoves(Pos, move, pieces);

            return move;
        }

        private void FindKillMoves(PiecePos pos, MoveTree parent, List<Piece> pieces)
        {
            var dir = (sbyte)(!White ? +1 : -1);

            if (IsValidMove(pos.X - 1, pos.Y + dir))
            {
                var onSpotToKill = PieceOnSpot(new PiecePos(pos.X - 1, pos.Y + dir), pieces);
                PiecePos endPos;
                if (onSpotToKill != null && onSpotToKill.White != White && IsValidMove(pos.X - 2, pos.Y + 2 * dir) &&
                    PieceOnSpot(endPos = new PiecePos(pos.X - 2, pos.Y + 2 * dir), pieces) is null)
                {
                    var move = new MoveTree(this, endPos, onSpotToKill, parent);
                    FindKillMoves(endPos, move, pieces.Where(p => p != onSpotToKill).ToList());
                }
            }

            if (IsValidMove(pos.X + 1, pos.Y + dir))
            {
                var onSpotToKill = PieceOnSpot(new PiecePos(pos.X + 1, pos.Y + dir), pieces);
                PiecePos endPos;
                if (onSpotToKill != null && onSpotToKill.White != White && IsValidMove(pos.X + 2, pos.Y + 2 * dir) &&
                    PieceOnSpot(endPos = new PiecePos(pos.X + 2, pos.Y + 2 * dir), pieces) is null)
                {
                    var move = new MoveTree(this, endPos, onSpotToKill, parent);
                    FindKillMoves(endPos, move, pieces.Where(p => p != onSpotToKill).ToList());
                }
            }
        }

        public Queen Upgrade()
        {
            return new Queen(White, Pos, Pieces);
        }

        public override char Mark => !Launcher.CurrentGameOptions.ReverseRoles
            ? Launcher.CurrentGameOptions.KnightMark
            : Launcher.CurrentGameOptions.QueenMark;
    }
}