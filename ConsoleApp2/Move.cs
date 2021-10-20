using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ConsoleApp2
{
    // public class PartMove : ICloneable
    // {
    //     public readonly PiecePos Pos;
    //     public readonly Piece Kill;
    //
    //     public PartMove(PiecePos pos, Piece kill)
    //     {
    //         Kill = kill;
    //         Pos = pos;
    //     }
    //
    //     public object Clone()
    //     {
    //         return new PartMove(Pos, Kill);
    //     }
    // }

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

    // public class FullMove : ICloneable
    // {
    //     public Piece piece;
    //     public List<PartMove> SingleMoves = new();
    //
    //     public FullMove(Piece piece)
    //     {
    //         this.piece = piece;
    //     }
    //
    //     public object Clone()
    //     {
    //         return new FullMove(piece) { SingleMoves = (List<PartMove>)SingleMoves.Clone() };
    //     }
    //
    //     public Piece[] AllKills => SingleMoves.Where(m => m.Kill != null).Select(m => m.Kill).ToArray();
    //     public PiecePos LastPos => SingleMoves[SingleMoves.Count - 1].Pos;
    // }
}