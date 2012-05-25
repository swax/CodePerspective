using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XLibrary
{
    public interface IRenderer
    {
        float ViewWidth { get; }
        float ViewHeight { get; }

        SizeF MeasureString(string text, Font font);

        void DrawString(string text, Font font, Color color, PointF point);
        void DrawString(string text, Font font, Color color, float x, float y);
        void DrawString(string text, Font font, Color color, RectangleF rect);

        void DrawTextBackground(Color color, float x, float y, float width, float height);

        void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth);
        void DrawNodeOutline(Color pen, int penWidth, RectangleF area, bool outside, NodeModel node, int depth);

        void DrawLine(Color color, int lineWidth, PointF start, PointF end, bool dashed);

        void ViewInvalidate();
        void ViewRefresh();

        Point GetCursorPosition();

    }

    public interface IMainUI
    {
        void UpdateBreadCrumbs();
        void NavigatePanelTo(NodeModel node);
    }
}
