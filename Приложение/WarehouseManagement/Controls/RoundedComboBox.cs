using System.Drawing.Drawing2D;

namespace WarehouseManagement.Controls;

public class RoundedComboBox : ComboBox
{
    private int _borderRadius = 8;
    private Color _borderColor = Color.FromArgb(59, 130, 246);

    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    public RoundedComboBox()
    {
        DoubleBuffered = true;
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
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
                using (var pen = new Pen(_borderColor, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        base.OnPaint(e);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);
        e.Graphics.DrawString(Items[e.Index].ToString(), Font, new SolidBrush(ForeColor), e.Bounds);
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
