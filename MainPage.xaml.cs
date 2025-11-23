using System.Collections.Concurrent;

namespace GomokuAI
{
    public partial class MainPage : ContentPage
    {
        private const int BoardSize = 15;
        private readonly byte[,] _board = new byte[BoardSize, BoardSize];

        private bool _isPlayerTurn;
        private bool _isGameOver;

        // 执子颜色
        private byte _userPiece = Piece.BLACK_PIECE;
        private byte _aiPiece = Piece.WHITE_PIECE;

        private BoardDrawable _drawable;

        private int _aiDepth = 2;

        // 新增：历史记录栈，用于悔棋 (记录每一步的坐标)
        private Stack<Point> _moveHistory = new();

        // 构造函数接收难度参数
        public MainPage(int difficulty)
        {
            InitializeComponent();
            _aiDepth = difficulty; // 设置难度

            if (BoardView.Drawable is BoardDrawable drawable)
            {
                _drawable = drawable;
            }
            else
            {
                _drawable = new BoardDrawable();
                BoardView.Drawable = _drawable;
            }

            // 延迟启动游戏
            Dispatcher.Dispatch(async () =>
            {
                await StartNewGame();
            });
        }

        // 无参构造函数（为了兼容性）
        public MainPage() : this(2) { }

        private async Task StartNewGame()
        {
            string action = await DisplayActionSheetAsync("请选择先手还是后手", "取消", null, "我执黑 (先手)", "我执白 (后手)");

            if (action == "取消" || string.IsNullOrEmpty(action))
            {
                // 如果已经有棋局且没结束，取消则不做任何事
                if (!_isGameOver && _moveHistory.Count > 0) return;
                action = "我执黑 (先手)";
            }

            // 1. 重置棋盘数据
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    _board[r, c] = Piece.EMPTY;

            // 2. 重置状态
            _isGameOver = false;
            _moveHistory.Clear(); // 清空历史记录

            _drawable.BoardState = _board;
            _drawable.LastMove = new Point(-1, -1);
            BoardView.Invalidate();

            // 3. 设置先手后手
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
                _aiPiece = Piece.BLACK_PIECE;
                _isPlayerTurn = false;
                StatusLabel.Text = "你执白，AI (黑) 思考中...";
                await AiMakeMove();
            }
        }

        // 统一落子方法（包含记录历史）
        private void PlacePiece(int r, int c, byte player)
        {
            _board[r, c] = player;

            // 记录到历史栈
            var p = new Point(r, c);
            _moveHistory.Push(p);

            // 更新 UI
            _drawable.LastMove = p;
            BoardView.Invalidate();
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

            // 玩家落子
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
            await AiMakeMove();
        }

        private async Task AiMakeMove()
        {
            AiThinkingIndicator.IsRunning = true;

            await Task.Run(async () =>
            {
                // 稍微延迟，避免瞬间落子
                await Task.Delay(300);

                // 调用 AI (传入 _aiDepth)
                var bestMove = AI.FindBestMove(_board, _aiPiece, _userPiece, _aiDepth);

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

        // ========== 缺失的重新开始方法 ==========
        private async void OnRestartClicked(object sender, EventArgs e)
        {
            // 即使正在思考也可以强制重开
            await StartNewGame();
        }

        // ========== 新增的悔棋方法 ==========
        private void OnUndoClicked(object sender, EventArgs e)
        {
            // 1. 如果 AI 正在思考，为了线程安全，禁止悔棋
            if (AiThinkingIndicator.IsRunning) return;

            // 2. 如果没有步数，无法悔棋
            if (_moveHistory.Count == 0) return;

            // 策略：悔两步（回退到玩家上一步落子之前的状态）
            // 正常回合是：玩家下 -> AI下。现在轮到玩家，所以要撤销 AI的一步 和 玩家的一步。
            int stepsToUndo = 2;

            // 执行回退
            while (stepsToUndo > 0 && _moveHistory.Count > 0)
            {
                Point last = _moveHistory.Pop();
                _board[(int)last.X, (int)last.Y] = Piece.EMPTY;
                stepsToUndo--;
            }

            // 恢复 LastMove 标记到现在的最后一步
            if (_moveHistory.Count > 0)
            {
                _drawable.LastMove = _moveHistory.Peek();
            }
            else
            {
                _drawable.LastMove = new Point(-1, -1);
            }

            // 恢复游戏状态
            _isGameOver = false;
            _isPlayerTurn = true;
            StatusLabel.Text = "悔棋成功，请落子";
            BoardView.Invalidate();
        }
    }
}