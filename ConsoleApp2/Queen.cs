using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    public class Queen : Piece
    {
        public Queen(bool white, PiecePos pos, List<Piece> pieces) : base(white, pos, pieces)
        {
        }

        public override MoveTree FindAllMoves(List<Piece> pieces = null)
        {
            pieces ??= Pieces;

            // List<FullMove> moves = new List<FullMove>();
            var move = new MoveTree(this, Pos);


            (sbyte x, sbyte y)[] offsets = { (-1, -1), (-1, 1), (1, -1), (1, 1) };
            for (var i = 0; i < offsets.GetLength(0); i++)
            {
                sbyte x = (sbyte)(Pos.X + offsets[i].x), y = (sbyte)(Pos.Y + offsets[i].y);
                PiecePos newPos;
                while (IsValidMove(x, y) && PieceOnSpot(newPos = new PiecePos((byte)x, (byte)y), pieces) is null)
                {
                    // move.NextMoves.Add(new MoveTree(this, newPos, null, move));
                    new MoveTree(this, newPos, null, move);

                    x += offsets[i].x;
                    y += offsets[i].y;
                }
            }

            // Get kill available kill moves and add them to all moves
            FindKillMoves(Pos, move, pieces, null);

            return move;
        }

        // TODO: Add functionality to work with new position of queen in 'PieceOnSpot' function instead of first one
        private void FindKillMoves(PiecePos pos, MoveTree parent, List<Piece> pieces,
            (sbyte x, sbyte y)? dontLookInto)
        {
            // Test all directions
            (sbyte x, sbyte y)[] dirs = { (-1, -1), (-1, 1), (1, -1), (1, 1) };
            if (dontLookInto != null)
                dirs = dirs.Where(d => !(d.x == dontLookInto?.x && d.y == dontLookInto?.y)).ToArray();

            // Cycle through all of 'em
            for (byte i = 0; i < dirs.Length; i++)
            {
                // List through all valid moves in that direction
                sbyte x = (sbyte)(pos.X + dirs[i].x), y = (sbyte)(pos.Y + dirs[i].y);
                while (IsValidMove(x, y))
                {
                    var thisPos = new PiecePos(x, y);
                    var pieceOnSpotToKill = PieceOnSpot(thisPos, pieces);
                    if (pieceOnSpotToKill != null)
                    {
                        if (pieceOnSpotToKill.White != White)
                        {
                            sbyte _x = (sbyte)(x + dirs[i].x), _y = (sbyte)(y + dirs[i].y);
                            if (!IsValidMove(_x, _y))
                                break;

                            var posAfterJump = new PiecePos(_x, _y);
                            var pieceOnSpotAfterJump = PieceOnSpot(posAfterJump, pieces);

                            while (pieceOnSpotAfterJump is null && IsValidMove(_x, _y))
                            {
                                posAfterJump = new PiecePos(_x, _y);
                                pieceOnSpotAfterJump = PieceOnSpot(posAfterJump, pieces);

                                var move = new MoveTree(this, posAfterJump, pieceOnSpotToKill, parent);

                                // Run this function recursively
                                FindKillMoves(posAfterJump, move,
                                    pieces.Where(p => p != pieceOnSpotToKill).ToList(),
                                    ((sbyte)-dirs[i].x, (sbyte)-dirs[i].y));

                                // Update pos for another run
                                _x += dirs[i].x;
                                _y += dirs[i].y;
                            }
                        }

                        // Break out of while loop after any jump, if we can jump more, it's job of recursive call above
                        break;
                    }

                    x += dirs[i].x;
                    y += dirs[i].y;
                }
            }
        }

        public override char Mark => Program.ReverseRoles ? Program.KnightMark : Program.QueenMark;
    }
}