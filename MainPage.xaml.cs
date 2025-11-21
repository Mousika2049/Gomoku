namespace GomokuAI
{
    public partial class MainPage : ContentPage
    {
        private const int BoardSize = 15;

        // 1. 初始化为 null，稍后赋值
        private byte[,] _board = new byte[BoardSize, BoardSize];

        private bool _isPlayerTurn;
        private bool _isGameOver;

        // 2. 新增变量：存储双方执子颜色
        private byte _userPiece = Piece.BLACK_PIECE; // 默认玩家执黑
        private byte _aiPiece = Piece.WHITE_PIECE;

        private BoardDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();

            if (BoardView.Drawable is BoardDrawable drawable)
            {
                _drawable = drawable;
            }
            else
            {
                _drawable = new BoardDrawable();
                BoardView.Drawable = _drawable;
            }

            // 3. 构造函数里不能直接弹窗，用 Dispatcher 延迟执行
            Dispatcher.Dispatch(async () =>
            {
                await StartNewGame();
            });
        }

        // 4. 核心逻辑：开始新游戏流程
        private async Task StartNewGame()
        {
            // 弹出选择框
            string action = await DisplayActionSheetAsync("请选择先手还是后手", "取消", null, "我执黑 (先手)", "我执白 (后手)");

            if (action == "取消" || string.IsNullOrEmpty(action))
            {
                // 如果取消，且是第一次打开，默认给个黑棋，或者保持当前状态
                if (_isGameOver) return; // 保持原样
                action = "我执黑 (先手)"; // 默认
            }

            // 重置棋盘数据
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    _board[r, c] = Piece.EMPTY;

            _isGameOver = false;
            _drawable.BoardState = _board;
            _drawable.LastMove = new Point(-1, -1);
            BoardView.Invalidate();

            // 根据选择设置执子
            if (action == "我执黑 (先手)")
            {
                _userPiece = Piece.BLACK_PIECE;
                _aiPiece = Piece.WHITE_PIECE;
                _isPlayerTurn = true;
                StatusLabel.Text = "你执黑，请落子";
                AiThinkingIndicator.IsRunning = false;
            }
            else
            {
                _userPiece = Piece.WHITE_PIECE;
                _aiPiece = Piece.BLACK_PIECE; // AI 执黑
                _isPlayerTurn = false; // 玩家不能动
                StatusLabel.Text = "你执白，AI (黑) 思考中...";

                // 5. 如果 AI 先手，AI 立即行动
                await AiMakeMove();
            }
        }

        private async void OnBoardTapped(object sender, TappedEventArgs e)
        {
            if (_isGameOver || !_isPlayerTurn) return;

            Point? touchPoint = e.GetPosition(BoardView);
            if (touchPoint == null) return;

            float padding = 20;
            float boardWidth = (float)BoardView.Width - (padding * 2);
            if (boardWidth <= 0) return;

            float cellSize = boardWidth / 14;
            int col = (int)Math.Round((touchPoint.Value.X - padding) / cellSize);
            int row = (int)Math.Round((touchPoint.Value.Y - padding) / cellSize);

            if (row < 0 || row >= 15 || col < 0 || col >= 15) return;
            if (_board[row, col] != Piece.EMPTY) return;

            // 6. 玩家落子 (使用变量 _userPiece)
            PlacePiece(row, col, _userPiece);

            if (Evaluate.CheckWin(row, col, _userPiece, _board))
            {
                StatusLabel.Text = "你赢了！";
                _isGameOver = true;
                await DisplayAlertAsync("结果", "恭喜你赢了！", "太棒了");
                return;
            }

            // 轮到 AI
            _isPlayerTurn = false;
            StatusLabel.Text = "AI 思考中...";

            // 7. 调用提取出来的 AI 逻辑
            await AiMakeMove();
        }

        // 8. 提取 AI 逻辑，方便复用
        private async Task AiMakeMove()
        {
            AiThinkingIndicator.IsRunning = true;

            await Task.Run(async () =>
            {
                await Task.Delay(300); // 稍微延迟一点，显得更有交互感

                // 注意参数顺序：FindBestMove(board, AI的棋子, 对手的棋子)
                var bestMove = AI.FindBestMove(_board, _aiPiece, _userPiece);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    PlacePiece(bestMove.Row, bestMove.Column, _aiPiece);
                    AiThinkingIndicator.IsRunning = false;

                    if (Evaluate.CheckWin(bestMove.Row, bestMove.Column, _aiPiece, _board))
                    {
                        StatusLabel.Text = "AI 赢了！";
                        _isGameOver = true;
                        await DisplayAlertAsync("结果", "AI 赢了，再接再厉！", "好的");
                    }
                    else
                    {
                        StatusLabel.Text = "轮到你了";
                        _isPlayerTurn = true;
                    }
                });
            });
        }

        private void PlacePiece(int r, int c, byte player)
        {
            _board[r, c] = player;
            _drawable.LastMove = new Point(r, c);
            BoardView.Invalidate();
        }

        private async void OnRestartClicked(object sender, EventArgs e)
        {
            // 点击重新开始时，再次询问
            await StartNewGame();
        }
    }
}