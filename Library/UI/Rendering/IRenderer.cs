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

        void Start();
        void Stop();

        SizeF MeasureString(string text, Font font);

        void DrawString(string text, Font font, Color color, float x, float y, float width, float height);
        void DrawNodeLabel(string text, Font font, Color color, RectangleF rect, NodeModel node, int depth);
        
        void DrawTextBackground(Color color, float x, float y, float width, float height);

        void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth);
        void DrawNodeOutline(Color pen, int penWidth, RectangleF area, bool outside, NodeModel node, int depth);

        void DrawCallLine(Color color, int lineWidth, PointF start, PointF end, bool live, NodeModel source, NodeModel destination);

        void ViewInvalidate();

        Point GetCursorPosition();

    }

    public interface IMainUI
    {
        void UpdateBreadCrumbs();
        void NavigatePanelTo(NodeModel node);
    }
}
