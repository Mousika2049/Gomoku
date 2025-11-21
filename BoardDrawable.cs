using Microsoft.Maui.Graphics;

namespace GomokuAI
{
    public class BoardDrawable : IDrawable
    {
        // 修改点1：加了 ?，允许初始为空，避免编译器报错
        public byte[,]? BoardState { get; set; }
        public Point LastMove { get; set; } = new Point(-1, -1);

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // 修改点2：如果棋盘数据还没准备好，就不画
            if (BoardState == null) return;

            canvas.Antialias = true;

            // 1. 绘制棋盘背景
            canvas.FillColor = Color.FromArgb("#E3C998");
            canvas.FillRectangle(dirtyRect);

            float padding = 20;
            float boardWidth = dirtyRect.Width - (padding * 2);
            // 防止除以0错误
            if (boardWidth <= 0) return;

            float cellSize = boardWidth / 14;

            // 2. 绘制网格线
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 1.5f;

            for (int i = 0; i < 15; i++)
            {
                float pos = padding + i * cellSize;
                canvas.DrawLine(padding, pos, padding + boardWidth, pos);
                canvas.DrawLine(pos, padding, pos, padding + boardWidth); // 假设正方形，高宽一致
            }

            // 3. 绘制天元和星位
            canvas.FillColor = Colors.Black;
            int[] starPoints = { 3, 7, 11 };
            foreach (int r in starPoints)
            {
                foreach (int c in starPoints)
                {
                    float cx = padding + c * cellSize;
                    float cy = padding + r * cellSize;
                    canvas.FillCircle(cx, cy, 4);
                }
            }

            // 4. 绘制棋子
            float pieceRadius = cellSize * 0.45f;

            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    byte cell = BoardState[r, c];
                    if (cell == Piece.EMPTY) continue;

                    float cx = padding + c * cellSize;
                    float cy = padding + r * cellSize;

                    if (cell == Piece.BLACK_PIECE) // 黑棋
                    {
                        canvas.FillColor = Colors.Black;
                        canvas.FillCircle(cx, cy, pieceRadius);
                        canvas.FillColor = Color.FromArgb("#50FFFFFF");
                        canvas.FillCircle(cx - pieceRadius * 0.3f, cy - pieceRadius * 0.3f, pieceRadius * 0.2f);
                    }
                    else if (cell == Piece.WHITE_PIECE) // 白棋
                    {
                        canvas.FillColor = Colors.White;
                        canvas.FillCircle(cx, cy, pieceRadius);
                        canvas.StrokeColor = Colors.Gray;
                        canvas.StrokeSize = 1;
                        canvas.DrawCircle(cx, cy, pieceRadius);
                    }
                }
            }

            // 5. 标记最后一步
            if (LastMove.X >= 0 && LastMove.Y >= 0)
            {
                float lx = padding + (float)LastMove.Y * cellSize;
                float ly = padding + (float)LastMove.X * cellSize;

                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 2;
                canvas.DrawRectangle(lx - 5, ly - 5, 10, 10);
            }
        }
    }
}