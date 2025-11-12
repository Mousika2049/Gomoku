using System.Text;
class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        //棋盘大小为15x15
        const int board = 15;

        //初始化棋盘
        List<List<string>> updatedboard = [];
        for (int i = 0; i < board; i++)
        {
            updatedboard.Add([]);
            for (int j = 0; j < board; j++)
            {
                updatedboard[i].Add("+");
            }
        }

        bool CheckWin(int[] location, string player)
        {
            //辅助函数
            bool IsOnBoard(int x, int y)
            {
                return x > 0 && x <= 15 && y > 0 && y <= 15;
            }
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
                    if (IsOnBoard(location[0] - i * d_x, location[1] - i * d_y) && updatedboard[15 - location[1] + i * d_y][location[0] - 1 - i * d_x] == player)
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
                    if (IsOnBoard(location[0] + j * d_x, location[1] + j * d_y) && updatedboard[15 - location[1] - j * d_y][location[0] - 1 + j * d_x] == player)
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

        

        bool IsGameOver = false;
        while (!IsGameOver)
        {
            while (true)
            {
                Console.WriteLine("你执黑，请输入坐标：");
                string[] input = Console.ReadLine().Split(" ");
                int[] Bmove = Array.ConvertAll(input, int.Parse);
                if (updatedboard[15 - Bmove[1]][Bmove[0] - 1] == "+")
                {
                    updatedboard[15 - Bmove[1]][Bmove[0] - 1] = "@";

                    foreach (var row in updatedboard)
                    {
                        Console.WriteLine(string.Join("", row));
                    }
                    if (CheckWin(Bmove, "@"))
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
                Console.WriteLine("你执白，请输入坐标：");
                string[] input = Console.ReadLine().Split(" ");
                int[] Bmove = Array.ConvertAll(input, int.Parse);
                if (updatedboard[15 - Bmove[1]][Bmove[0] - 1] == "+")
                {
                    updatedboard[15 - Bmove[1]][Bmove[0] - 1] = "O";

                    foreach (var row in updatedboard)
                    {
                        Console.WriteLine(string.Join("", row));
                    }
                    if (CheckWin(Bmove, "O"))
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