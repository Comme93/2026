using System.Drawing.Drawing2D;

namespace WarehouseManagement.Controls;

public class RoundedButton : Button
{
    private int _borderRadius = 8;

    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (var path = GetRoundedRectanglePath(ClientRectangle, _borderRadius))
        {
            e.Graphics.FillPath(new SolidBrush(BackColor), path);

            if (Focused)
            {
                using (var pen = new Pen(ForeColor, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        var textSize = TextRenderer.MeasureText(Text, Font);
        var textLocation = new Point(
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2
        );

        TextRenderer.DrawText(e.Graphics, Text, Font, textLocation, ForeColor);
    }

    private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int x = rect.X;
        int y = rect.Y;
        int w = rect.Width;
        int h = rect.Height;

        path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
        path.AddArc(x + w - radius * 2, y, radius * 2, radius * 2, 270, 90);
        path.AddArc(x + w - radius * 2, y + h - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(x, y + h - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();

        return path;
    }
}
