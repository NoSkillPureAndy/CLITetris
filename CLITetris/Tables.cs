// ReSharper disable UnusedMember.Local

using System.Diagnostics.CodeAnalysis;

namespace CLITetris;

internal static partial class Program
{
    private static readonly string[] BlockColors =
    {
        "[48;5;235m  ", // Empty - Dark Gray
        "[48;5;75m  ", // I - Light Blue
        "[48;5;226m  ", // O - Yellow
        "[48;5;165m  ", // T - Purple
        "[48;5;46m  ", // S - Green
        "[48;5;196m  ", // Z - Red
        "[48;5;21m  ", // J - Blue
        "[48;5;208m  ", // L - Orange
        "[48;5;249m  ", // Ghost - Gray
    };

    private static readonly (int X, int Y)[][][] BlockData =
    {
        new[] //Empty
        {
            new[]
            {
                (0, 0),
                (0, 0),
                (0, 0),
                (0, 0),
            },
            new[]
            {
                (0, 0),
                (0, 0),
                (0, 0),
                (0, 0),
            },
            new[]
            {
                (0, 0),
                (0, 0),
                (0, 0),
                (0, 0),
            },
            new[]
            {
                (0, 0),
                (0, 0),
                (0, 0),
                (0, 0),
            },
        },
        new[] //I
        {
            new[]
            {
                (0, 1), //····
                (1, 1), //####
                (2, 1), //····
                (3, 1), //····
            },
            new[]
            {
                (2, 0), //··#·
                (2, 1), //··#·
                (2, 2), //··#·
                (2, 3), //··#·
            },
            new[]
            {
                (0, 2), //····
                (1, 2), //····
                (2, 2), //####
                (3, 2), //····
            },
            new[]
            {
                (1, 0), //·#··
                (1, 1), //·#··
                (1, 2), //·#··
                (1, 3), //·#··
            },
        },
        new[] //·
        {
            new[]
            {
                (0, 1), //·##·
                (1, 1), //·##·
                (0, 0),
                (1, 0),
            },
            new[]
            {
                (0, 1), //·##·
                (1, 1), //·##·
                (0, 0),
                (1, 0),
            },
            new[]
            {
                (0, 1), //·##·
                (1, 1), //·##·
                (0, 0),
                (1, 0),
            },
            new[]
            {
                (0, 1), //·##·
                (1, 1), //·##·
                (0, 0),
                (1, 0),
            },
        },
        new[] //T
        {
            new[]
            {
                (0, 1), //·#·
                (1, 1), //###
                (2, 1), //···
                (1, 0),
            },
            new[]
            {
                (1, 2), //··#
                (2, 1), //·##
                (1, 1), //··#
                (1, 0),
            },
            new[]
            {
                (1, 2), //···
                (2, 1), //###
                (0, 1), //·#·
                (1, 1),
            },
            new[]
            {
                (1, 2), //·#·
                (0, 1), //##·
                (1, 1), //·#·
                (1, 0),
            },
        },
        new[] //S
        {
            new[]
            {
                (0, 1), //·##
                (1, 1), //##·
                (1, 0), //···
                (2, 0),
            },
            new[]
            {
                (2, 2), //·#·
                (2, 1), //·##
                (1, 1), //··#
                (1, 0),
            },
            new[]
            {
                (0, 2), //···
                (1, 2), //·##
                (1, 1), //##·
                (2, 1),
            },
            new[]
            {
                (1, 2), //#··
                (1, 1), //##·
                (0, 1), //·#·
                (0, 0),
            },
        },
        new[] //Z
        {
            new[]
            {
                (1, 1), //##·
                (2, 1), //·##
                (0, 0), //···
                (1, 0),
            },
            new[]
            {
                (1, 2), //··#
                (1, 1), //.##
                (2, 1), //.#.
                (2, 0),
            },
            new[]
            {
                (1, 2), //···
                (2, 2), //##·
                (0, 1), //·##
                (1, 1),
            },
            new[]
            {
                (0, 2), //·#·
                (0, 1), //##·
                (1, 1), //#··
                (1, 0),
            },
        },
        new[] //J
        {
            new[]
            {
                (0, 1), //#··
                (1, 1), //###
                (2, 1), //···
                (0, 0),
            },
            new[]
            {
                (1, 2), //·##
                (1, 1), //·#·
                (1, 0), //·#·
                (2, 0),
            },
            new[]
            {
                (2, 2), //···
                (2, 1), //###
                (1, 1), //··#
                (0, 1),
            },
            new[]
            {
                (0, 2), //·#·
                (1, 2), //·#·
                (1, 1), //##·
                (1, 0),
            },
        },
        new[] //L
        {
            new[]
            {
                (0, 1), //··#
                (1, 1), //###
                (2, 1), //···
                (2, 0),
            },
            new[]
            {
                (2, 2), //·#·
                (1, 2), //·#·
                (1, 1), //·##
                (1, 0),
            },
            new[]
            {
                (0, 2), //···
                (0, 1), //###
                (1, 1), //#··
                (2, 1),
            },
            new[]
            {
                (1, 2), //##·
                (1, 1), //·#·
                (1, 0), //·#·
                (0, 0),
            },
        },
    };

    private enum Block
    {
        Empty,
        I,
        O,
        T,
        S,
        Z,
        J,
        L,
        Ghost,
    }
    
    private enum Rotation
    {
        Up,
        Right,
        Down,
        Left,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    private enum ScoreValue
    {
        Single = 100,
        MiniTSpin = 100,
        MiniTSpinSingle = 200,
        Double = 300,
        TSpin = 400,
        MiniTSpinDouble = 400,
        Triple = 500,
        B2BMiniTSpinDouble = 600,
        Tetris = 800,
        TSpinSingle = 800,
        B2BTSpinSingle = 1200,
        B2BTetris = 1200,
        TSpinDouble = 1200,
        TSpinTriple = 1600,
        B2BTSpinDouble = 1800,
        B2BTSpinTriple = 2400,
        Combo = 50,
        SoftDrop = 1,
        HardDrop = 2,
    }
    
    private static readonly Dictionary<int, ScoreValue> LineScores = new()
    {
        {1, ScoreValue.Single},
        {2, ScoreValue.Double},
        {3, ScoreValue.Triple},
        {4, ScoreValue.Tetris},
    };
}