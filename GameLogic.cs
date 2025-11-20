using System.Collections.Concurrent;

namespace GomokuAI
{
    // 用于传递移动信息的简单结构
    public struct Move
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Score { get; set; }
    }

    public class Evaluate
    {
        static private bool IsOnBoard(int r, int c)
        {
            return r >= 0 && r < 15 && c >= 0 && c < 15;
        }

        // 统一使用 [row, col] 坐标系
        static public bool CheckWin(int r, int c, string player, string[,] board)
        {
            // 方向：水平(0,1), 垂直(1,0), 主对角(1,1), 副对角(1,-1)
            int[][] directions = [[0, 1], [1, 0], [1, 1], [1, -1]];

            foreach (var dir in directions)
            {
                int count = 1;
                int dr = dir[0];
                int dc = dir[1];

                // 向一个方向延伸
                for (int i = 1; i <= 4; i++)
                {
                    int nr = r + i * dr;
                    int nc = c + i * dc;
                    if (IsOnBoard(nr, nc) && board[nr, nc] == player) count++;
                    else break;
                }
                // 向相反方向延伸
                for (int i = 1; i <= 4; i++)
                {
                    int nr = r - i * dr;
                    int nc = c - i * dc;
                    if (IsOnBoard(nr, nc) && board[nr, nc] == player) count++;
                    else break;
                }

                if (count >= 5) return true;
            }
            return false;
        }

        static private readonly Dictionary<int, int> aiPatternScore = new()
        {
            {5, 1000000}, {4, 10000}, {3, 1000}, {2, 100}, {1, 10}
        };
        static private readonly Dictionary<int, int> humanPatternScore = new()
        {
            {5, -1000000}, {4, -50000}, {3, -5000}, {2, -500}, {1, -10}
        };

        static public int EvaluateBoard(string[,] board, string aiPlayer, string humanPlayer)
        {
            int overallScore = 0;
            // 这里的逻辑保持不变，只要遍历整个数组即可
            // 水平
            for (int r = 0; r < 15; r++)
                for (int c = 0; c <= 10; c++)
                    overallScore += GetWindowScore(board, r, c, 0, 1, aiPlayer, humanPlayer);
            // 垂直
            for (int r = 0; r <= 10; r++)
                for (int c = 0; c < 15; c++)
                    overallScore += GetWindowScore(board, r, c, 1, 0, aiPlayer, humanPlayer);
            // 主对角线
            for (int r = 0; r <= 10; r++)
                for (int c = 0; c <= 10; c++)
                    overallScore += GetWindowScore(board, r, c, 1, 1, aiPlayer, humanPlayer);
            // 副对角线
            for (int r = 0; r <= 10; r++)
                for (int c = 4; c < 15; c++)
                    overallScore += GetWindowScore(board, r, c, 1, -1, aiPlayer, humanPlayer);

            return overallScore;
        }

        static int GetWindowScore(string[,] board, int startR, int startC, int dr, int dc, string ai, string human)
        {
            int aiCount = 0;
            int humanCount = 0;
            for (int i = 0; i < 5; i++)
            {
                string cell = board[startR + i * dr, startC + i * dc];
                if (cell == ai) aiCount++;
                else if (cell == human) humanCount++;
            }

            if (aiCount > 0 && humanCount > 0) return 0;
            if (aiCount > 0) return aiPatternScore[aiCount];
            if (humanCount > 0) return humanPatternScore[humanCount];
            return 0;
        }
    }

    public static class AI
    {
        static public List<Move> GetOptimizedMoves(string[,] board, string aiPlayer, string humanPlayer)
        {
            bool[,] searchWindow = new bool[15, 15];
            bool hasPiece = false;

            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    if (board[r, c] != "+")
                    {
                        hasPiece = true;
                        for (int rr = -2; rr <= 2; rr++)
                        {
                            for (int cc = -2; cc <= 2; cc++)
                            {
                                int nr = r + rr;
                                int nc = c + cc;
                                if (nr >= 0 && nr < 15 && nc >= 0 && nc < 15 && board[nr, nc] == "+")
                                {
                                    searchWindow[nr, nc] = true;
                                }
                            }
                        }
                    }
                }
            }

            // 如果棋盘是空的（第一步），下在天元
            if (!hasPiece) return new List<Move> { new Move { Row = 7, Col = 7, Score = 0 } };

            List<Move> moves = new();
            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    if (searchWindow[r, c])
                    {
                        // 简单的启发式排序：先评估一层
                        board[r, c] = aiPlayer;
                        int score = Evaluate.EvaluateBoard(board, aiPlayer, humanPlayer); // 简化：只看当前盘面分
                        board[r, c] = "+";
                        moves.Add(new Move { Row = r, Col = c, Score = score });
                    }
                }
            }
            // 降序排列，让更有希望的点先搜
            return moves.OrderByDescending(m => m.Score).ToList();
        }

        static public int MiniMax(string[,] board, int depth, bool isMaximizing, string aiPlayer, string humanPlayer, int alpha, int beta)
        {
            int score = Evaluate.EvaluateBoard(board, aiPlayer, humanPlayer);
            if (Math.Abs(score) >= 100000) return score; // 胜负已分
            if (depth == 0) return score;

            // 在递归层可以稍微减少搜索范围或不重新排序以节省时间，这里沿用你的逻辑
            var moves = GetOptimizedMoves(board, aiPlayer, humanPlayer);

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    board[move.Row, move.Col] = aiPlayer;
                    int eval = MiniMax(board, depth - 1, false, aiPlayer, humanPlayer, alpha, beta);
                    board[move.Row, move.Col] = "+";
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    board[move.Row, move.Col] = humanPlayer;
                    int eval = MiniMax(board, depth - 1, true, aiPlayer, humanPlayer, alpha, beta);
                    board[move.Row, move.Col] = "+";
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        static private string[,] DeepCopyBoard(string[,] board)
        {
            return (string[,])board.Clone();
        }

        public static Move FindBestMove(string[,] board, string aiPlayer, string humanPlayer)
        {
            const int DEPTH = 4;
            var possibleMoves = GetOptimizedMoves(board, aiPlayer, humanPlayer);

            // 并行计算
            var results = new ConcurrentBag<Move>();

            Parallel.ForEach(possibleMoves, move =>
            {
                string[,] tempBoard = DeepCopyBoard(board);
                tempBoard[move.Row, move.Col] = aiPlayer;
                int score = MiniMax(tempBoard, DEPTH - 1, false, aiPlayer, humanPlayer, int.MinValue, int.MaxValue);
                results.Add(new Move { Row = move.Row, Col = move.Col, Score = score });
            });

            // 找到分数最高的
            var bestMove = results.OrderByDescending(m => m.Score).FirstOrDefault();
            return bestMove;
        }
    }
}