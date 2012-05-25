using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary.Panels
{
    public partial class ViewPanel : UserControl
    {
        MainForm Main;
        ViewModel Model;

        public ViewPanel()
        {
            InitializeComponent();

            LayoutInitGraphButton.AttachToolTip("A graph showing which class created which class");

            MapDependencies.AttachToolTip("Select a class node to show dependencies in a treemap\r\nBlue - independent of, red - dependent on, purple - inter-dependent");
            GraphDependencies.AttachToolTip("Select a class node to show dependencies in a dependecy graph\r\nBlue - independent of, red - dependent on, purple - inter-dependent");
            ShowAllDependenciesCheckBox.AttachToolTip("Show dependencies of dependencies");
            GraphIntermediateDependencies.AttachToolTip("Given 2 selected class nodes, show how they depend on each other");

            IncludeOutsideZoomButton.AttachToolTip("Show methods outside current zoom level\r\nIn call graph view these methods are gray");
            IncludeNotXRayedButton.AttachToolTip("Shows methods outside what was xrayed\r\nIn call graph view these methods are circles");
            LayoutInOrder.AttachToolTip("In the call graph methods are shown in the order that they were first called top to bottom");

            ShowAllButton.AttachToolTip("Show all calls ever made.\r\nIn graph mode, red lines are calls LtR, blue lines are RtL and purple is both.");
            
        }

        public void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;

            CallsAllButton.Checked = XRay.ShowAllCalls;
            CallsRealTimeButton.Checked = Model.ShowCalls;

            IncludeOutsideZoomButton.Checked = Model.ShowOutside;
            IncludeNotXRayedButton.Checked = Model.ShowExternal;
            IncludeFields.Checked = Model.ShowFields;
            IncludeMethods.Checked = Model.ShowMethods;
            IncludeAnon.Checked = Model.ShowAnon;

            // layout
            LayoutTreeMapButton.Checked = Model.ViewLayout == LayoutType.TreeMap;
            LayoutCallGraphButton.Checked = Model.ViewLayout == LayoutType.CallGraph;
            TimelineButton.Checked = Model.ViewLayout == LayoutType.Timeline;
            LayoutInOrder.Checked = Model.SequenceOrder;

            // rendering
            RenderGdiButton.Checked = Main.SelectedView is GdiRenderer;
            RenderOpenGLButton.Checked = Main.SelectedView is GLRenderer;
            RenderFpsButton.Checked = Main.SelectedView is FpsRenderer;
            RenderGibsonButton.Checked = Main.SelectedView is GibsonView;

            // show
            ShowAllButton.Checked = Model.ShowLayout == ShowNodes.All;
            ShowHitButton.Checked = Model.ShowLayout == ShowNodes.Hit;
            ShowNotHitButton.Checked = Model.ShowLayout == ShowNodes.Unhit;
            ShowInstancesButton.Checked = Model.ShowLayout == ShowNodes.Instances;

            SizeConstantButton.Checked = Model.SizeLayout == SizeLayouts.Constant;
            SizeLinesButton.Checked = Model.SizeLayout == SizeLayouts.MethodSize;
            SizeTimeInMethodButton.Checked = Model.SizeLayout == SizeLayouts.TimeInMethod;
            SizeCallsButton.Checked = Model.SizeLayout == SizeLayouts.Hits;
            SizeTimePerCallButton.Checked = Model.SizeLayout == SizeLayouts.TimePerHit;

            // tracking
            TrackingMethodCalls.Enabled = XRay.FlowTracking;
            TrackingClassCalls.Enabled = XRay.FlowTracking;
            TrackingInstances.Enabled = XRay.InstanceTracking;

            TrackingMethodCalls.Checked = XRay.FlowTracking;
            TrackingClassCalls.Checked = XRay.ClassTracking;
            TrackingInstances.Checked = XRay.InstanceTracking;

            ShowAllDependenciesCheckBox.Checked = Model.ShowAllDependencies;
        }

        private void LayoutTreeMapButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutTreeMapButton.Checked)
                return;

            Model.ViewLayout = LayoutType.TreeMap;
            Model.MapMode = TreeMapMode.Normal;
            Main.RefreshView();
        }

        private void LayoutCallGraphButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutCallGraphButton.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Method;
            Main.RefreshView();
        }

        private void LayoutInitGraphButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutInitGraphButton.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Init;
            Main.RefreshView();
        }


        private void LayoutInOrder_CheckedChanged(object sender, EventArgs e)
        {
            Model.SequenceOrder = LayoutInOrder.Checked;
            Main.RefreshView();
        }

        private void LayoutClassCallsButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutClassCallsButton.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Class;
            Main.RefreshView();
        }

        private void TimelineButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!TimelineButton.Checked)
                return;

            Model.ViewLayout = LayoutType.Timeline;
            Main.RefreshView();
        }

        private void MapAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!MapDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.TreeMap;
            Model.MapMode = TreeMapMode.Dependencies;
            Model.ShowAllDependencies = true;
            Main.RefreshView();
        }

        private void GraphAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!GraphDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Dependencies;
            Model.ShowAllDependencies = true;
            Main.RefreshView();
        }

        private void GraphIntermediateDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!GraphIntermediateDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Intermediates;
            Model.InterDependencies = Model.GetClassesFromFocusedNodes();
            Main.RefreshView();
        }

        private void ShowAllDependenciesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowAllDependencies = ShowAllDependenciesCheckBox.Checked;
            Main.RefreshView();
        }

        private void ShowAllButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowLayout = ShowNodes.All;
            Main.RefreshView();
        }

        private void ShowHitButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowLayout = ShowNodes.Hit;
            Main.RefreshView();
        }

        private void ShowNotHitButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowLayout = ShowNodes.Unhit;
            Main.RefreshView();
        }

        private void ShowInstancesButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowLayout = ShowNodes.Instances;
            Main.RefreshView();
        }

        private void ResetHitLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            XRay.CoveredNodes.SetAll(false);

            var stillCovered = new List<int>();

            foreach (var node in XRay.Nodes.Where(n => n.StillInside > 0))
            {
                Utilities.IterateParents<XNode>(
                    node,
                    n => stillCovered.Add(n.ID),
                    n => n.Parent);
            }

            foreach (var id in stillCovered)
                XRay.CoveredNodes[id] = true;

            Main.RefreshView();
        }

        private void ResetProfilingLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (var call in XRay.CallMap)
            {
                call.TotalCallTime = 0;
                call.TotalHits = 0;
                call.TotalTimeOutsideDest = 0;
            }

            foreach (var call in XRay.ClassCallMap)
            {
                call.TotalCallTime = 0;
                call.TotalHits = 0;
                call.TotalTimeOutsideDest = 0;
            }

            Main.RefreshView();
        }

        private void CallsAllButton_CheckedChanged(object sender, EventArgs e)
        {
            XRay.ShowAllCalls = CallsAllButton.Checked;
            Main.RefreshView(true, false);
        }

        private void CallsRealTimeButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowCalls = CallsRealTimeButton.Checked;
            Main.RefreshView(true, false);
        }

        private void IncludeOutsideZoomButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowOutside = IncludeOutsideZoomButton.Checked;
            Main.RefreshView();
        }
        
        private void IncludeNotXRayedButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowExternal = IncludeNotXRayedButton.Checked;
            Main.RefreshView();
        }

        private void IncludeFields_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowFields = IncludeFields.Checked;
            Main.RefreshView();
        }

        private void SizeConstantButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.SizeLayout = SizeLayouts.Constant;
            Main.RefreshView();
        }

        private void SizeLinesButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.SizeLayout = SizeLayouts.MethodSize;
            Main.RefreshView();
        }

        private void SizeTimeInMethodButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.SizeLayout = SizeLayouts.TimeInMethod;
            Main.RefreshView();
        }

        private void SizeCallsButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.SizeLayout = SizeLayouts.Hits;
            Main.RefreshView();
        }

        private void SizeTimePerCallButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.SizeLayout = SizeLayouts.TimePerHit;
            Main.RefreshView();
        }

        private void TrackingMethodCalls_CheckedChanged(object sender, EventArgs e)
        {
            XRay.FlowTracking = TrackingMethodCalls.Checked;
        }

        private void TrackingClassCalls_CheckedChanged(object sender, EventArgs e)
        {
            XRay.ClassTracking = TrackingClassCalls.Checked;
        }

        private void TrackingInstances_CheckedChanged(object sender, EventArgs e)
        {
            XRay.InstanceTracking = TrackingInstances.Checked;
        }

        private void IncludeMethods_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowMethods = IncludeMethods.Checked;
            Main.RefreshView();
        }

        private void IncludeAnon_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowAnon = IncludeAnon.Checked;
            Main.RefreshView();
        }

        private void RenderGdiButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!RenderGdiButton.Checked)
                return;

            Main.SetRenderer(typeof(GdiRenderer));
            Main.RefreshView();
        }

        private void RenderOpenGLButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!RenderOpenGLButton.Checked)
                return;

            Main.SetRenderer(typeof(GLRenderer));
            Main.RefreshView();
        }

        private void RenderFpsButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!RenderFpsButton.Checked)
                return;

            Main.SetRenderer(typeof(FpsRenderer));
            Main.RefreshView();
        }

        private void RenderGibsonButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!RenderGibsonButton.Checked)
                return;

            Main.SetRenderer(typeof(GibsonView));
            Main.RefreshView();
        }
    }
}
