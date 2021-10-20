using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    public class MoveTree : ICloneable
    {
        public List<MoveTree> NextMoves = new();
        public Piece Piece;
        public PiecePos Pos;
        public Piece Kill;
        public MoveTree Parent;
        public List<MoveTree> GetObligatoryOnes => NextMoves.Where(m => m.Kill != null).ToList();
        public bool HasObligatory => NextMoves.FirstOrDefault(m => m.Kill != null) != null;

        public List<Piece> AllToKillCandidates =>
            NextMoves.Where(m => m.Kill is not null).Select(m => m.Kill).Distinct().ToList();

        public MoveTree(Piece piece, PiecePos pos, Piece kill = null, MoveTree parent = null)
        {
            Piece = piece;
            Pos = pos;
            Kill = kill;
            Parent = parent;

            Parent?.NextMoves.Add(this);
        }

        public object Clone()
        {
            return new MoveTree(Piece, Pos, Kill, Parent)
                { NextMoves = (List<MoveTree>)NextMoves.Clone() };
        }

        public bool ValidMove(PiecePos move, bool forced = true) => forced && HasObligatory
            ? GetObligatoryOnes.Select(m => m.Pos).Contains(move)
            : NextMoves.Select(m => m.Pos).Contains(move);

        public MoveTree MoveToPos(PiecePos move) => NextMoves.FirstOrDefault(m => m.Pos == move);
    }
}