using System;
using System.Windows;
using System.Windows.Media;

namespace DSCons.Revit.Starter.Infrastructure;

/// <summary>Creates simple semantic vector icons without image-file dependencies.</summary>
public static class SemanticRibbonIconFactory
{
    public static ImageSource Create(string icon, bool large)
    {
        var size = large ? 32d : 16d;
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            dc.PushTransform(new ScaleTransform(size / 32d, size / 32d));
            var accent = Brush(UiBranding.PrimaryColor);
            dc.DrawRoundedRectangle(Brush("#F2FAFC"), Pen(Brush("#B5DCE5"), 1), new Rect(.75, .75, 30.5, 30.5), 5, 5);

            if (string.Equals(icon, "filter", StringComparison.OrdinalIgnoreCase))
            {
                DrawPolyline(dc, accent, 2, new Point(6, 7), new Point(26, 7), new Point(19, 15), new Point(19, 25), new Point(13, 22), new Point(13, 15), new Point(6, 7));
            }
            else if (string.Equals(icon, "tag", StringComparison.OrdinalIgnoreCase))
            {
                DrawPolyline(dc, accent, 2, new Point(7, 8), new Point(18, 8), new Point(25, 15), new Point(18, 22), new Point(7, 22), new Point(7, 8));
                dc.DrawEllipse(accent, null, new Point(12, 15), 2, 2);
            }
            else if (string.Equals(icon, "about", StringComparison.OrdinalIgnoreCase))
            {
                dc.DrawEllipse(null, Pen(accent, 2), new Point(16, 11), 4, 4);
                var shoulders = new StreamGeometry();
                using (var context = shoulders.Open())
                {
                    context.BeginFigure(new Point(8, 26), false, false);
                    context.QuadraticBezierTo(new Point(9, 18), new Point(16, 18), true, true);
                    context.QuadraticBezierTo(new Point(23, 18), new Point(24, 26), true, true);
                }
                shoulders.Freeze();
                dc.DrawGeometry(null, Pen(accent, 2), shoulders);
            }
            else if (string.Equals(icon, "view3d", StringComparison.OrdinalIgnoreCase) || string.Equals(icon, "3d", StringComparison.OrdinalIgnoreCase))
            {
                DrawPolyline(dc, accent, 2, new Point(16, 7), new Point(25, 12), new Point(25, 22), new Point(16, 27), new Point(7, 22), new Point(7, 12), new Point(16, 7));
                dc.DrawLine(Pen(accent, 1.8), new Point(16, 17), new Point(16, 27));
                dc.DrawLine(Pen(accent, 1.8), new Point(16, 17), new Point(7, 12));
                dc.DrawLine(Pen(accent, 1.8), new Point(16, 17), new Point(25, 12));
            }
            else if (string.Equals(icon, "align", StringComparison.OrdinalIgnoreCase))
            {
                // Đường gióng chuẩn thẳng đứng (Alignment reference line)
                dc.DrawLine(Pen(accent, 2.2), new Point(8, 5.5), new Point(8, 26.5));

                // 3 thẻ Tag được gióng thẳng tắp vào đường chuẩn
                // Tag 1 (Trên)
                DrawPolyline(dc, accent, 1.6, new Point(11, 7), new Point(21, 7), new Point(25, 9.5), new Point(21, 12), new Point(11, 12), new Point(11, 7));
                dc.DrawLine(Pen(accent, 1.2), new Point(8, 9.5), new Point(11, 9.5));
                dc.DrawEllipse(accent, null, new Point(14, 9.5), 1.2, 1.2);

                // Tag 2 (Giữa)
                DrawPolyline(dc, accent, 1.6, new Point(11, 13.5), new Point(21, 13.5), new Point(25, 16), new Point(21, 18.5), new Point(11, 18.5), new Point(11, 13.5));
                dc.DrawLine(Pen(accent, 1.2), new Point(8, 16), new Point(11, 16));
                dc.DrawEllipse(accent, null, new Point(14, 16), 1.2, 1.2);

                // Tag 3 (Dưới)
                DrawPolyline(dc, accent, 1.6, new Point(11, 20), new Point(21, 20), new Point(25, 22.5), new Point(21, 25), new Point(11, 25), new Point(11, 20));
                dc.DrawLine(Pen(accent, 1.2), new Point(8, 22.5), new Point(11, 22.5));
                dc.DrawEllipse(accent, null, new Point(14, 22.5), 1.2, 1.2);
            }
            else
            {
                dc.DrawRectangle(null, Pen(accent, 2), new Rect(9, 9, 14, 14));
                dc.DrawLine(Pen(accent, 1.8), new Point(16, 5), new Point(16, 9));
                dc.DrawLine(Pen(accent, 1.8), new Point(16, 23), new Point(16, 27));
                dc.DrawLine(Pen(accent, 1.8), new Point(5, 16), new Point(9, 16));
                dc.DrawLine(Pen(accent, 1.8), new Point(23, 16), new Point(27, 16));
            }
            dc.Pop();
        }
        group.Freeze();
        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }

    private static void DrawPolyline(DrawingContext dc, Brush brush, double thickness, params Point[] points)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(points[0], false, false);
            var remaining = new Point[points.Length - 1];
            Array.Copy(points, 1, remaining, 0, remaining.Length);
            context.PolyLineTo(remaining, true, true);
        }
        geometry.Freeze();
        dc.DrawGeometry(null, Pen(brush, thickness), geometry);
    }

    private static Brush Brush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }

    private static Pen Pen(Brush brush, double thickness)
    {
        var pen = new Pen(brush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        pen.Freeze();
        return pen;
    }
}