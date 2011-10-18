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
        TreePanelGdiPlus MainView;

        public ViewPanel()
        {
            InitializeComponent();
        }

        public void Init(TreePanelGdiPlus mainView)
        {
            MainView = mainView;

            CallsAllButton.Checked = XRay.ShowAllCalls;
            CallsRealTimeButton.Checked = MainView.ShowCalls;

            IncludeOutsideZoomButton.Checked = MainView.ShowOutside;
            IncludeNotXRayedButton.Checked = MainView.ShowExternal;
            IncludeFields.Checked = MainView.ShowFields;

            // layout
            LayoutTreeMapButton.Checked = MainView.ViewLayout == LayoutType.TreeMap;
            LayoutCallGraphButton.Checked = MainView.ViewLayout == LayoutType.CallGraph;
            LayoutInOrder.Checked = MainView.SequenceOrder;

            ShowAllButton.Checked = MainView.ShowLayout == ShowNodes.All;
            ShowHitButton.Checked = MainView.ShowLayout == ShowNodes.Hit;
            ShowNotHitButton.Checked = MainView.ShowLayout == ShowNodes.Unhit;
            ShowInstancesButton.Checked = MainView.ShowLayout == ShowNodes.Instances;

            SizeConstantButton.Checked = MainView.SizeLayout == SizeLayouts.Constant;
            SizeLinesButton.Checked = MainView.SizeLayout == SizeLayouts.MethodSize;
            SizeTimeInMethodButton.Checked = MainView.SizeLayout == SizeLayouts.TimeInMethod;
            SizeCallsButton.Checked = MainView.SizeLayout == SizeLayouts.Hits;
            SizeTimePerCallButton.Checked = MainView.SizeLayout == SizeLayouts.TimePerHit;

            // tracking
            TrackingMethodCalls.Enabled = XRay.FlowTracking;
            TrackingClassCalls.Enabled = XRay.FlowTracking;
            TrackingInstances.Enabled = XRay.InstanceTracking;

            TrackingMethodCalls.Checked = XRay.FlowTracking;
            TrackingClassCalls.Checked = XRay.ClassTracking;
            TrackingInstances.Checked = XRay.InstanceTracking;
        }

        private void LayoutTreeMapButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ViewLayout = LayoutType.TreeMap;
            MainView.MapMode = TreeMapMode.Normal;
            MainView.RecalcValues();
        }

        private void LayoutCallGraphButton_CheckedChanged(object sender, EventArgs e)
        {         
            MainView.ViewLayout = LayoutType.CallGraph;
            MainView.GraphMode = CallGraphMode.Method;
            MainView.RecalcValues();
        }

        private void LayoutInOrder_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SequenceOrder = LayoutInOrder.Checked;
            MainView.RecalcValues();
        }

        private void LayoutClassCallsButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ViewLayout = LayoutType.CallGraph;
            MainView.GraphMode = CallGraphMode.Class;
            MainView.RecalcValues();
        }

        private void MapDirectDependencies_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ViewLayout = LayoutType.TreeMap;
            MainView.MapMode = TreeMapMode.DirectDependencies;
            MainView.RecalcValues();
        }

        private void MapAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ViewLayout = LayoutType.TreeMap;
            MainView.MapMode = TreeMapMode.AllDependencies;
            MainView.RecalcValues();
        }

        private void GraphAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ViewLayout = LayoutType.CallGraph;
            MainView.GraphMode = CallGraphMode.Dependencies;
            MainView.RecalcValues();
        }

        private void ShowAllButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowLayout = ShowNodes.All;
            MainView.RecalcValues();
        }

        private void ShowHitButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowLayout = ShowNodes.Hit;
            MainView.RecalcValues();
        }

        private void ShowNotHitButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowLayout = ShowNodes.Unhit;
            MainView.RecalcValues();
        }

        private void ShowInstancesButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowLayout = ShowNodes.Instances;
            MainView.RecalcValues();
        }

        private void ResetHitButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < XRay.CoveredFunctions.Count; i++)
                if (XRay.Nodes[i].StillInside == 0)
                    XRay.CoveredFunctions[i] = false;

            MainView.RecalcValues();
        }

        private void ResetProfilingButton_Click(object sender, EventArgs e)
        {
            foreach(var call in XRay.CallMap)
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

            MainView.RecalcValues();
        }

        private void CallsAllButton_CheckedChanged(object sender, EventArgs e)
        {
            XRay.ShowAllCalls = CallsAllButton.Checked;
            MainView.Redraw();
        }

        private void CallsRealTimeButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowCalls = CallsRealTimeButton.Checked;
            MainView.Redraw();
        }

        private void IncludeOutsideZoomButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowOutside = IncludeOutsideZoomButton.Checked;
            MainView.RecalcValues();
        }
        
        private void IncludeNotXRayedButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowExternal = IncludeNotXRayedButton.Checked;
            MainView.RecalcValues();
        }

        private void IncludeFields_CheckedChanged(object sender, EventArgs e)
        {
            MainView.ShowFields = IncludeFields.Checked;
            MainView.RecalcValues();
        }

        private void SizeConstantButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SizeLayout = SizeLayouts.Constant;
            MainView.RecalcValues();
        }

        private void SizeLinesButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SizeLayout = SizeLayouts.MethodSize;
            MainView.RecalcValues();
        }

        private void SizeTimeInMethodButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SizeLayout = SizeLayouts.TimeInMethod;
            MainView.RecalcValues();
        }

        private void SizeCallsButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SizeLayout = SizeLayouts.Hits;
            MainView.RecalcValues();
        }

        private void SizeTimePerCallButton_CheckedChanged(object sender, EventArgs e)
        {
            MainView.SizeLayout = SizeLayouts.TimePerHit;
            MainView.RecalcValues();
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
    }
}
