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

        void FillEllipse(Color color, RectangleF area);

        void FillRectangle(Color color, RectangleF area);
        void FillRectangle(Color color, float x, float y, float width, float height);

        void DrawEllipse(Color color, int lineWidth, float x, float y, float width, float height);

        void DrawRectangle(Color color, int lineWidth, float x, float y, float width, float height);

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
