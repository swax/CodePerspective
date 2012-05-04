using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class ViewHost : UserControl
    {
        public MainForm Main;
        public GdiPanel GdiView;
        public GLPanel GLView;
        public ViewModel Model;


        public ViewHost()
        {
            InitializeComponent();
        }

        internal void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;

            GdiView = new GdiPanel(this, new BrightColorProfile()) { Dock = DockStyle.Fill };

            Controls.Add(GdiView);

            Model.Renderer = GdiView;

            Model.SetRoot(Model.CurrentRoot); // init first node in history
        }

        public void RefreshView(bool redrawOnly = false, bool resetZoom = true)
        {
            if (Model.ViewLayout == LayoutType.ThreeD)
            {
                GdiView.Visible = false;

                if (GLView == null)
                {
                    GLView = new GLPanel(Main) { Dock = DockStyle.Fill };
                    Controls.Add(GLView);
                }

                GLView.Visible = true;
                Model.DoRevalue = !redrawOnly;
                GLView.Redraw();
            }
            else
            {
                if (GLView != null)
                    GLView.Visible = false;

                GdiView.Visible = true;

                if (resetZoom)
                    Model.ResetZoom();
              
                // check if view exists
                if (redrawOnly)
                {
                    Model.DoRedraw = true;
                    GdiView.Invalidate();
                }
                else
                {
                    Model.DoRevalue = true;
                    GdiView.Invalidate();
                }

                //PauseLink.Visible = (Model.ViewLayout == LayoutType.Timeline);
            }
        }

    }
}
