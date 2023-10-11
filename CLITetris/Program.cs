using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CLITetris;
// Note: actual namespace depends on the project name.

internal static partial class Program
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 4;

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetStdHandle(int nStdHandle);

    private const int BoardWidth = 10;
    private const int BoardHeight = 20;
    private const string LineEnd = "[0m\n";

    private static readonly Blocks[,] Board = new Blocks[BoardWidth, BoardHeight];

    private static (int X, int Y)[] _occupiedBlocks = new (int X, int Y)[4];

    private static readonly (int X, int Y) ResetPosition = (4, 0);

    private static Blocks _heldBlock;
    private static Blocks _currentBlock;
    private static Rotation _currentRotation = Rotation.Up;
    private static int _score;
    private static int _level = 1;
    private static int _lagCounter;
    private static bool _playerIsAlive = true;
    private static ConsoleKeyInfo _bufferedInput;
    private static (int X, int Y) _position = ResetPosition;
    private static double _timer;

    private static readonly List<Blocks> Bag = new();
    private static readonly List<Blocks> NextBag = new();

    private static readonly Random Random = new();

    private static void EnableANSI()
    {
        nint handle = GetStdHandle(StdOutputHandle);
        GetConsoleMode(handle, out uint mode);
        mode |= EnableVirtualTerminalProcessing;
        SetConsoleMode(handle, mode);
    }

    public static void Main()
    {
        EnableANSI();


        Console.CursorVisible = false;
        Console.WriteLine("Press any key to start...");
        _bufferedInput = Console.ReadKey(true);
        Console.SetCursorPosition(0, 0);
        Stopwatch s = new();

        Bag.AddRange(Enum.GetValues<Blocks>()[1..8]);
        Bag.Shuffle();
        _currentBlock = Bag[0];
        NextBag.AddRange(Enum.GetValues<Blocks>()[1..8]);
        NextBag.Shuffle();

        while (true)
        {
            ResetGame();
            _position = ResetPosition;
            while (_playerIsAlive)
            {
                s.Start();
                if (Console.KeyAvailable)
                    _bufferedInput = Console.ReadKey(true);

                UpdateBoard();
                RenderBoard();

                _bufferedInput = default;
                s.Stop();
                _lagCounter = (int)s.ElapsedMilliseconds;
                s.Reset();
                _timer += (int)Math.Pow(_level, 3) / 2000d;
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Game Over!");
            Console.WriteLine($"Score: {_score}");
            Console.WriteLine($"Level: {_level}");
            Console.WriteLine("Press any key to restart...");
            Thread.Sleep(200); // Give the user time to stop pressing keys
            while (Console.KeyAvailable)
                Console.ReadKey(true); // Flush input buffer
            Console.ReadKey(true);
        }
    }

    private static void RenderBoard()
    {
        StringBuilder board = new();
        for (int y = 0; y < BoardHeight; y++)
        {
            for (int x = 0; x < BoardWidth; x++)
                board.Append(BlockColors[(int)Board[x, y]]);

            board.Append(LineEnd);
        }

        string infoText =
            $"<<Next: {Bag[1]} | Held: {_heldBlock} | Score: {_score} | Lag: {_lagCounter}ms | Level: {_level} | Timer: {_timer} | X: {_position.X}>>";
        infoText = infoText.PadLeft(infoText.Length + (BoardWidth - infoText.Length) / 2);
        Console.SetCursorPosition(0, 0);
        board.Append(infoText);
        Console.WriteLine(board.ToString());
    }

    private static void UpdateBoard()
    {
        for (int i = 0; i < 4; i++)
        {
            _occupiedBlocks[i] = (X: _position.X + BlockData[(int)_currentBlock][(int)_currentRotation][i].X,
                Y: _position.Y + BlockData[(int)_currentBlock][(int)_currentRotation][i].Y);
            if (_bufferedInput.Key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.DownArrow
                    or ConsoleKey.Spacebar or ConsoleKey.UpArrow or ConsoleKey.Z or ConsoleKey.X or ConsoleKey.C || _timer >= 1)
                Board[_occupiedBlocks[i].X, _occupiedBlocks[i].Y] = Blocks.Empty;
        }

        if (_timer >= 1)
        {
            _timer = 0;
            Down(true);
        }

        switch (_bufferedInput.Key)
        {
            case ConsoleKey.LeftArrow:
            {
                bool success = true;
                LoopOver((x, y) =>
                {
                    if (x != 0 && Board[x - 1, y] == Blocks.Empty)
                        return true;
                    success = false;
                    return false;
                });

                if (success)
                {
                    LoopOver((x, y) =>
                    {
                        Board[x - 1, y] = _currentBlock;
                        return true;
                    });
                    _position.X--;
                }
                else
                    LoopOver((x, y) =>
                    {
                        Board[x, y] = _currentBlock;
                        return true;
                    });
                break;
            }
            case ConsoleKey.RightArrow:
            {
                bool success = true;
                LoopOver((x, y) =>
                {
                    if (x != BoardWidth - 1 && Board[x + 1, y] == Blocks.Empty)
                        return true;
                    success = false;
                    return false;
                });

                if (success)
                {
                    LoopOver((x, y) =>
                    {
                        Board[x + 1, y] = _currentBlock;
                        return true;
                    });
                    _position.X++;
                }
                else
                    LoopOver((x, y) =>
                    {
                        Board[x, y] = _currentBlock;
                        return true;
                    });
                break;
            }
            case ConsoleKey.DownArrow:
            {
                Down();
                break;
            }

            case ConsoleKey.UpArrow or ConsoleKey.X: // Rotate clockwise
            {
                _currentRotation = (Rotation)(((int)_currentRotation + 1) % 4);
                LoopOver((x, y) =>
                {
                    Board[x, y] = _currentBlock;
                    return true;
                });
                break;
            }
            
            case ConsoleKey.Spacebar: // Hard drop
            {
                bool success = true;
                while (success)
                {
                    LoopOver((x, y) =>
                    {
                        if (y != BoardHeight - 1 && Board[x, y + 1] == Blocks.Empty)
                            return true;
                        success = false;
                        _position.Y--;
                        return false;
                    });
                    _position.Y++;
                }
            
                if (success)
                {
                    LoopOver((x, y) =>
                    {
                        Board[x, y + 1] = _currentBlock;
                        return true;
                    });
                    _position.Y++;
                }
                else
                {
                    LoopOver((x, y) =>
                    {
                        Board[x, y] = _currentBlock;
                        return true;
                    });
                    _timer = 0;
                    QueueNextBlock();
                }
                break;
            }

            case ConsoleKey.C:
            {
                if (_heldBlock == Blocks.Empty)
                {
                    _heldBlock = _currentBlock;
                    QueueNextBlock();
                }
                else
                {
                    (Blocks, Blocks) temp = (_heldBlock, _currentBlock);
                    _heldBlock = temp.Item2;
                    _currentBlock = temp.Item1;
                    _position = ResetPosition;
                    _currentRotation = Rotation.Up;
                }
                break;
            }
        }

        for (int y = 0; y < BoardHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < BoardWidth; x++)
                if (Board[x, y] == Blocks.Empty)
                {
                    full = false;
                    break;
                }

            if (!full)
                continue;
            
            for (int x = 0; x < BoardWidth; x++)
                Board[x, y] = Blocks.Empty;
            
            for (int i = y; i > 0; i--)
            for (int x = 0; x < BoardWidth; x++)
                Board[x, i] = Board[x, i - 1];
            
            _score += 100;
            _level = _score / 1000 + 1;
        }

        void LoopOver(Func<int, int, bool> func)
        {
            for (int i = 0; i < 4; i++)
            {
                (int X, int Y) blockData = BlockData[(int)_currentBlock][(int)_currentRotation][i];
                int x = blockData.X + _position.X;
                int y = blockData.Y + _position.Y;
                if (!func(x, y))
                    return;
            }
        }

        void Down(bool stopAtGround = false)
        {
            bool success = true;
            LoopOver((x, y) =>
            {
                if (y != BoardHeight - 1 && Board[x, y + 1] == Blocks.Empty)
                    return true;
                success = false;
                return false;
            });
            
            if (success)
            {
                LoopOver((x, y) =>
                {
                    Board[x, y + 1] = _currentBlock;
                    return true;
                });
                _position.Y++;
            }
            else
            {
                LoopOver((x, y) =>
                {
                    Board[x, y] = _currentBlock;
                    return true;
                });
                if (stopAtGround)
                {
                    _timer = 0;
                    QueueNextBlock();
                }
            }
        }
    }

    private static void QueueNextBlock()
    {
        if (NextBag.Count == 0)
        {
            NextBag.AddRange(Enum.GetValues<Blocks>()[1..8]);
            NextBag.Shuffle();
        }

        Bag.RemoveAt(0);
        Bag.Add(NextBag[0]);
        NextBag.RemoveAt(0);
        _currentBlock = Bag[0];
        _position = ResetPosition;
        _currentRotation = Rotation.Up;
    }

    private static void ResetGame()
    {
    }

    private static void Drop()
    {
    }

    private static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}