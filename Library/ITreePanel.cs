using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLibrary
{
    public interface ITreePanel
    {
        void Redraw();

        XNodeIn GetRoot();

        void Dispose2();
    }
}
