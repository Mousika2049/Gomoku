using System.Text;

class Evaluate
{
    //边界检查辅助函数 
    static private bool IsOnBoard(int x, int y)
    {
        return x > 0 && x <= 15 && y > 0 && y <= 15;
    }

    static public bool CheckWin(int[] location, string player, List<List<string>> board)
    {
        //direction参数
        //水平 (1,0)
        //垂直 (0,1)
        //主对角线 (1,-1)
        //斜对角线 (1,1)
        int CountOnAxis(int[] direction, int[] location, string player)
        {
            int d_x = direction[0];
            int d_y = direction[1];

            int count = 1;
            for (int i = 1; i <= 4; i++)
            {
                if (IsOnBoard(location[0] - i * d_x, location[1] - i * d_y) && board[15 - location[1] + i * d_y][location[0] - 1 - i * d_x] == player)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int j = 1; j <= 4; j++)
            {
                if (IsOnBoard(location[0] + j * d_x, location[1] + j * d_y) && board[15 - location[1] - j * d_y][location[0] - 1 + j * d_x] == player)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        if (CountOnAxis([1, 0], location, player) >= 5) return true;
        if (CountOnAxis([0, 1], location, player) >= 5) return true;
        if (CountOnAxis([1, 1], location, player) >= 5) return true;
        if (CountOnAxis([1, -1], location, player) >= 5) return true;
        return false;
    }

    //统计
    static public int GetScoreForLine(int[] location, List<List<string>> board)
    {
        int[] CountOnLine(int[] direction, int[] location, string aiPlayer = "O", string humanPlayer = "@")
        {
            int d_x = direction[0];
            int d_y = direction[1];

            int aiCount = 0;
            int humanCount = 0;
            int emptyCount = 0;
            for (int j = 0; j <= 4; j++)
            {
                if (IsOnBoard(location[0] + j * d_x, location[1] + j * d_y))
                {
                    if (board[15 - location[1] - j * d_y][location[0] - 1 + j * d_x] == aiPlayer)
                    {
                        aiCount++;
                    }
                    else if (board[15 - location[1] - j * d_y][location[0] - 1 + j * d_x] == humanPlayer)
                    {
                        humanCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                }
                else
                {
                    break;
                }
            }
            return [aiCount, humanCount, emptyCount];
        }
        //权重计分板(ai视角，因为要模拟落点算分)
        Dictionary<int, int> aiPatternScore = new()
        {
            {5,1000000 },
            {4,10000 },
            {3,1000 },
            {2,100 },
            {1,10 }
        };
        Dictionary<int, int> humanPatternScore = new()
        {
            {5,-1000000 },
            {4,-50000 },
            {3,-5000 },
            {2,-500 },
            {1,-10 }
        };

        //Directions
        List<int[]> Directions = [[1, 0], [0, 1], [1, -1], [1, 1]];
        int total = 0;
        foreach (int[] i in Directions)
        {
            int[] counts = CountOnLine(i, location);
            if (counts.Sum() == 5)
            {
                if (counts[0] != 0 && counts[1] != 0)
                {
                    continue;
                }
                if (counts[0] != 0)
                {
                    total += aiPatternScore[counts[0]];
                }
                if (counts[1] != 0)
                {
                    total += humanPatternScore[counts[1]];
                }
            }
        }
        return total;
    }

    //统计棋局评分
    static public int EvaluateBoard(List<List<string>> board, string aiPlayer = "O", string humanPlayer = "@")
    {
        int overallScore = 0;

        // 遍历棋盘的每一个格子
        for (int r = 0; r < 15; r++)
        {
            for (int c = 0; c < 15; c++)
            {
                int[] location = [c + 1, 15 - r];
                overallScore += GetScoreForLine(location, board);
            }
        }
        return overallScore;
    }
}

public static class AI
{
    static public List<int[]> GetOptimizedMoves(List<List<string>> board, string aiPlayer, string humanPlayer)
    {
        //设置一个搜索窗口，将搜索范围缩小到有棋子的周围一定区域，减少无用的循环次数
        //二维布尔数组如果不声明布尔值，默认值是false
        bool[,] searchWindow = new bool[15, 15];
        for (int r = 0; r < 15; r++)
        {
            for (int c = 0; c < 15; c++)
            {
                if (board[r][c] != "+")
                {
                    //扫描周围5x5区域
                    for (int a = -2; a <= 2; a++)
                    {
                        for (int b = -2; b <= 2; b++)
                        {
                            int new_r = r + a;
                            int new_c = c + b;
                            if (new_r >= 0 && new_r < 15 && new_c >= 0 && new_c < 15 && board[new_r][new_c] == "+")
                            {
                                searchWindow[new_r, new_c] = true;
                            }
                        }
                    }
                }
            }
        }

        List<int[]> optimizedMove = [];
        for (int r = 0; r < 15; r++)
        {
            for (int c = 0; c < 15; c++)
            {
                if (board[r][c] == "+" && searchWindow[r, c])
                {
                    board[r][c] = aiPlayer;
                    int onedepthScore = MiniMax(board, 0, false, aiPlayer, humanPlayer, int.MinValue, int.MaxValue);
                    board[r][c] = "+";
                    optimizedMove.Add([r, c, onedepthScore]);
                }
            }
        }
        return optimizedMove;

    }
    static public int MiniMax(List<List<string>> board, int depth, bool isMaximizingPlayer, string aiPlayer, string humanPlayer, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        //如果模拟下一子能够凑成5子，说明已经赢了或者输了，不需要递归模拟
        int boardScore = Evaluate.EvaluateBoard(board, aiPlayer, humanPlayer);
        if (boardScore >= 1000000 || boardScore <= -1000000)
        {
            return boardScore;
        }
        if (depth == 0)
        {
            return boardScore;
        }

        var optimizedMoves = GetOptimizedMoves(board, "O", "@");
        if (isMaximizingPlayer)
        {
            int bestScore = int.MinValue;
            var sortedMoves = optimizedMoves.OrderByDescending(k => k[2]);

            foreach (var move in sortedMoves)
            {
                board[move[0]][move[1]] = aiPlayer;
                int score = MiniMax(board, depth - 1, false, aiPlayer, humanPlayer, alpha, beta);
                board[move[0]][move[1]] = "+";
                bestScore = int.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);

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
            var sortedMoves = optimizedMoves.OrderByDescending(k => k[2]);

            foreach (var move in sortedMoves)
            {
                board[move[0]][move[1]] = humanPlayer;
                int score = MiniMax(board, depth - 1, true, aiPlayer, humanPlayer, alpha, beta);
                board[move[0]][move[1]] = "+";
                bestScore = int.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);

                if (beta <= alpha)
                {
                    break;
                }
            }
            return bestScore;
        }
    }

    //添加辅助的深拷贝函数，目的是多线程
    static private List<List<string>> DeepCopyBoard(List<List<string>> board)
    {
        var copiedBoard = new List<List<string>>(board.Count);
        foreach (var list in board)
        {
            copiedBoard.Add(new List<string>(list));
        }
        return copiedBoard;
    }

    static public int[] FindBestMove(List<List<string>> board, string aiPlayer, string humanPlayer)
    {
        int bestScore = int.MinValue;
        int[] bestMove = [-1, -1];

        //设置模拟深度
        const int DEPTH = 3;

        var optimizedMoves = GetOptimizedMoves(board, "O", "@");
        var sortedMoves = optimizedMoves.OrderByDescending(k => k[2]);

        var moveScores = sortedMoves.AsParallel().Select(move =>
        {
            List<List<string>> parallelBoard = DeepCopyBoard(board);
            parallelBoard[move[0]][move[1]] = aiPlayer;
            int moveScore = MiniMax(parallelBoard, DEPTH - 1, false, aiPlayer, humanPlayer, int.MinValue, int.MaxValue);
            return new { Move = move, Score = moveScore };
        }).ToList();

        var sortedResult = moveScores.OrderByDescending(k => k.Score).First();

        bestScore = sortedResult.Score;
        bestMove = [sortedResult.Move[0], sortedResult.Move[1]];

        Console.WriteLine($"AI 选择了 [{bestMove[1] + 1}, {15 - bestMove[0]}]。评估分数: {bestScore}");
        return bestMove;
    }
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            //棋盘大小为15x15
            const int BOARD = 15;

            //初始化棋盘
            List<List<string>> updatedboard = [];
            for (int i = 0; i < BOARD; i++)
            {
                updatedboard.Add([]);
                for (int j = 0; j < BOARD; j++)
                {
                    updatedboard[i].Add("+");
                }
            }

            bool IsGameOver = false;
            while (!IsGameOver)
            {
                while (true)
                {
                    Console.WriteLine("你执黑，请输入坐标：");
                    string[] input = Console.ReadLine().Split(" ");
                    int[] blackMove = Array.ConvertAll(input, int.Parse);
                    if (updatedboard[15 - blackMove[1]][blackMove[0] - 1] == "+")
                    {
                        updatedboard[15 - blackMove[1]][blackMove[0] - 1] = "@";

                        foreach (var row in updatedboard)
                        {
                            Console.WriteLine(string.Join("", row));
                        }
                        if (Evaluate.CheckWin(blackMove, "@", updatedboard))
                        {
                            Console.WriteLine("黑棋获胜！");
                            IsGameOver = true;
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("此点已有落子，重新输入...");
                    }
                }
                if (IsGameOver) break;

                while (true)
                {
                    Console.WriteLine("AI执白...");

                    int[] bestMove = AI.FindBestMove(updatedboard, "O", "@");
                    int[] whiteMove = [bestMove[1] + 1, 15 - bestMove[0]];
                    if (updatedboard[bestMove[0]][bestMove[1]] == "+")
                    {
                        updatedboard[bestMove[0]][bestMove[1]] = "O";

                        foreach (var row in updatedboard)
                        {
                            Console.WriteLine(string.Join("", row));
                        }
                        if (Evaluate.CheckWin(whiteMove, "O", updatedboard))
                        {
                            Console.WriteLine("白棋获胜！");
                            IsGameOver = true;
                            break;
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("此点已有落子，重新输入...");
                    }
                }
            }
        }
    }
}