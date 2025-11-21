using System.Collections.Concurrent;

namespace GomokuAI
{
    //棋子表示从string类改成byte型，占用内存大幅减少
    public static class Piece
    {
        public const byte EMPTY = 0;
        public const byte BLACK_PIECE = 1;
        public const byte WHITE_PIECE = 2;
    }

    //用结构类型表示落点，值类型栈分配，GC压力小
    public struct Move
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Score { get; set; }
    }

    public class Evaluate
    {
        //边界检查辅助函数 
        static private bool IsOnBoard(int r, int c)
        {
            return r >= 0 && r < 15 && c >= 0 && c < 15;
        }

        static public bool CheckWin(int row, int column, byte player, byte[,] board)
        {
            //directions参数
            //水平 (1,0)
            //垂直 (0,1)
            //主对角线 (1,-1)
            //斜对角线 (1,1)
            int[][] directions = [[0, 1], [1, 0], [1, 1], [1, -1]];

            foreach (var direction in directions)
            {
                int count = 1;
                int directionRow = direction[0];
                int directionColumn = direction[1];

                for (int i = 1; i <= 4; i++)
                {
                    int newRow = row + i * directionRow;
                    int newColumn = column + i * directionColumn;
                    if (IsOnBoard(newRow, newColumn) && board[newRow, newColumn] == player)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = 1; i <= 4; i++)
                {
                    int newRow = row - i * directionRow;
                    int newColumn = column - i * directionColumn;
                    if (IsOnBoard(newRow, newColumn) && board[newRow, newColumn] == player)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (count >= 5)
                {
                    return true;
                }
            }
            return false;
        }

        //权重计分板(ai视角，因为要模拟落点算分)
        static private readonly Dictionary<int, int> aiPatternScore = new()
        {
            {5, 1000000},
            {4, 10000},
            {3, 1000},
            {2, 100},
            {1, 10}
        };
        static private readonly Dictionary<int, int> humanPatternScore = new()
        {
            {5, -1000000},
            {4, -50000},
            {3, -5000},
            {2, -500},
            {1, -10}
        };

        static int GetWindowScore(byte[,] board, int initialRow, int initialColumn, int directionRow, int directionColumn, byte aiPlayer, byte humanPlayer)
        {
            int aiCount = 0;
            int humanCount = 0;
            for (int i = 0; i < 5; i++)
            {
                byte piece = board[initialRow + i * directionRow, initialColumn + i * directionColumn];
                if (piece == aiPlayer)
                {
                    aiCount++;
                }
                else if (piece == humanPlayer)
                {
                    humanCount++;
                }
            }

            if (aiCount > 0 && humanCount > 0)
            {
                return 0;
            }
            if (aiCount > 0)
            {
                return aiPatternScore[aiCount];
            }
            if (humanCount > 0)
            {
                return humanPatternScore[humanCount];
            }
            return 0;
        }

        //统计棋局评分
        static public int EvaluateBoard(byte[,] board, byte aiPlayer, byte humanPlayer)
        {
            int overallScore = 0;
            //水平
            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c <= 10; c++)
                {
                    overallScore += GetWindowScore(board, r, c, 0, 1, aiPlayer, humanPlayer);
                }
            }
            //垂直
            for (int r = 0; r <= 10; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    overallScore += GetWindowScore(board, r, c, 1, 0, aiPlayer, humanPlayer);
                }
            }
            //主对角线
            for (int r = 0; r <= 10; r++)
            {
                for (int c = 0; c <= 10; c++)
                {
                    overallScore += GetWindowScore(board, r, c, 1, 1, aiPlayer, humanPlayer);
                }
            }
            //副对角线
            for (int r = 0; r <= 10; r++)
            {
                for (int c = 4; c < 15; c++)
                {
                    overallScore += GetWindowScore(board, r, c, 1, -1, aiPlayer, humanPlayer);
                }
            }
            return overallScore;
        }
    }

    public static class AI
    {
        static public List<Move> GetOptimizedMoves(byte[,] board, byte aiPlayer, byte humanPlayer)
        {
            //设置一个搜索窗口，将搜索范围缩小到有棋子的周围一定区域，减少无用的循环次数
            //二维布尔数组如果不声明布尔值，默认值是false
            bool[,] searchWindow = new bool[15, 15];
            bool hasPiece=false;
            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    if (board[r, c] != Piece.EMPTY)
                    {
                        hasPiece = true;
                        //扫描周围5x5区域
                        for (int a = -2; a <= 2; a++)
                        {
                            for (int b = -2; b <= 2; b++)
                            {
                                int newRow = r + a;
                                int newColumn = c + b;
                                if (newRow >= 0 && newRow < 15 && newColumn >= 0 && newColumn < 15 && board[newRow, newColumn] == Piece.EMPTY)
                                {
                                    searchWindow[newRow, newColumn] = true;
                                }
                            }
                        }
                    }
                }
            }

            //如果AI先手，默认下在天元位
            if (!hasPiece)
            {
                return [new() { Row = 7, Column = 7, Score = 0 }];
            }


            List<Move> moves = [];
            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    if (searchWindow[r, c])
                    {
                        // 先简单评估盘面分，排序，加快剪枝
                        board[r, c] = aiPlayer;
                        int score = Evaluate.EvaluateBoard(board, aiPlayer, humanPlayer);
                        board[r, c] = Piece.EMPTY;
                        moves.Add(new Move { Row = r, Column = c, Score = score });
                    }
                }
            }
            return moves.OrderByDescending(m => m.Score).ToList();
        }

        static public int MiniMax(byte[,] board, int depth, bool isMaximizing, byte aiPlayer, byte humanPlayer, int alpha, int beta)
        {
            int score = Evaluate.EvaluateBoard(board, aiPlayer, humanPlayer);
            //如果模拟下一子能够凑成5子，说明已经赢了或者输了，不需要递归模拟
            if (Math.Abs(score) >= 100000)
            {
                return score;
            }
            if (depth == 0)
            {
                return score;
            }

            var moves = GetOptimizedMoves(board, aiPlayer, humanPlayer);

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                foreach (var move in moves)
                {
                    board[move.Row, move.Column] = aiPlayer;
                    int eval = MiniMax(board, depth - 1, false, aiPlayer, humanPlayer, alpha, beta);
                    board[move.Row, move.Column] = Piece.EMPTY;
                    bestScore = Math.Max(bestScore, eval);
                    alpha = Math.Max(alpha, eval);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                foreach (var move in moves)
                {
                    board[move.Row, move.Column] = humanPlayer;
                    int eval = MiniMax(board, depth - 1, true, aiPlayer, humanPlayer, alpha, beta);
                    board[move.Row, move.Column] = Piece.EMPTY;
                    bestScore = Math.Min(bestScore, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return bestScore;
            }
        }

        //添加辅助的深拷贝函数，目的是多线程
        static private byte[,] DeepCopyBoard(byte[,] board)
        {
            return (byte[,])board.Clone();
        }

        public static Move FindBestMove(byte[,] board, byte aiPlayer, byte humanPlayer)
        {
            const int DEPTH = 4;
            var possibleMoves = GetOptimizedMoves(board, aiPlayer, humanPlayer);

            // 并行计算
            var results = new ConcurrentBag<Move>(); //线程安全

            Parallel.ForEach(possibleMoves, move =>
            {
                byte[,] tempBoard = DeepCopyBoard(board);
                tempBoard[move.Row, move.Column] = aiPlayer;
                int score = MiniMax(tempBoard, DEPTH - 1, false, aiPlayer, humanPlayer, int.MinValue, int.MaxValue);
                results.Add(new Move { Row = move.Row, Column = move.Column, Score = score });
            });

            // 找到分数最高的
            var bestMove = results.OrderByDescending(m => m.Score).FirstOrDefault();
            return bestMove;
        }
    }
}