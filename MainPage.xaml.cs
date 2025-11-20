namespace GomokuAI
{
    public partial class MainPage : ContentPage
    {
        private const int BoardSize = 15;

        // 修改点1：初始化为 null! (告诉编译器我会稍后赋值) 或者直接 new
        private string[,] _board = new string[BoardSize, BoardSize];

        private bool _isPlayerTurn;
        private bool _isGameOver;

        // 缓存一个绘图类的引用，方便调用
        private BoardDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();

            // 修改点2：从 GraphicsView 中获取我们在 XAML 里定义的 Drawable
            if (BoardView.Drawable is BoardDrawable drawable)
            {
                _drawable = drawable;
            }
            else
            {
                // 如果获取失败，手动创建一个
                _drawable = new BoardDrawable();
                BoardView.Drawable = _drawable;
            }

            InitializeGame();
        }

        private void InitializeGame()
        {
            // 初始化数组
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    _board[r, c] = "+";
                }
            }

            _isGameOver = false;
            _isPlayerTurn = true;

            StatusLabel.Text = "游戏开始，你执黑";
            AiThinkingIndicator.IsRunning = false;

            // 传递数据给绘图层
            _drawable.BoardState = _board;
            _drawable.LastMove = new Point(-1, -1);

            // 强制重绘
            BoardView.Invalidate();
        }

        private async void OnBoardTapped(object sender, TappedEventArgs e)
        {
            if (_isGameOver || !_isPlayerTurn) return;

            Point? touchPoint = e.GetPosition(BoardView);
            if (touchPoint == null) return;

            double x = touchPoint.Value.X;
            double y = touchPoint.Value.Y;

            float padding = 20;
            float boardWidth = (float)BoardView.Width - (padding * 2);
            if (boardWidth <= 0) return;

            float cellSize = boardWidth / 14;

            int col = (int)Math.Round((x - padding) / cellSize);
            int row = (int)Math.Round((y - padding) / cellSize);

            if (row < 0 || row >= 15 || col < 0 || col >= 15) return;
            if (_board[row, col] != "+") return;

            // 玩家落子
            PlacePiece(row, col, "@");

            if (Evaluate.CheckWin(row, col, "@", _board))
            {
                StatusLabel.Text = "黑棋获胜！";
                _isGameOver = true;
                // 修改点3：确保使用 await
                await DisplayAlertAsync("结果", "你赢了！", "棒");
                return;
            }

            // AI 回合
            _isPlayerTurn = false;
            StatusLabel.Text = "AI 思考中...";
            AiThinkingIndicator.IsRunning = true;

            // 这里的逻辑保持不变
            await Task.Run(async () =>
            {
                await Task.Delay(100);
                var bestMove = AI.FindBestMove(_board, "O", "@");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    PlacePiece(bestMove.Row, bestMove.Col, "O");
                    AiThinkingIndicator.IsRunning = false;

                    if (Evaluate.CheckWin(bestMove.Row, bestMove.Col, "O", _board))
                    {
                        StatusLabel.Text = "白棋获胜！";
                        _isGameOver = true;
                        await DisplayAlertAsync("结果", "AI 赢了，再接再厉！", "好");
                    }
                    else
                    {
                        StatusLabel.Text = "轮到你了";
                        _isPlayerTurn = true;
                    }
                });
            });
        }

        private void PlacePiece(int r, int c, string player)
        {
            _board[r, c] = player;
            _drawable.LastMove = new Point(r, c);
            BoardView.Invalidate(); // 通知画布重绘
        }

        private void OnRestartClicked(object sender, EventArgs e)
        {
            InitializeGame();
        }
    }
}