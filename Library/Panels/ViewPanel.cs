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

            // layout
            LayoutTreeMapButton.Checked = Model.ViewLayout == LayoutType.TreeMap;
            LayoutCallGraphButton.Checked = Model.ViewLayout == LayoutType.CallGraph;
            LayoutInOrder.Checked = Model.SequenceOrder;

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
        }

        private void LayoutTreeMapButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutTreeMapButton.Checked)
                return;

            Model.ViewLayout = LayoutType.TreeMap;
            Model.MapMode = TreeMapMode.Normal;
            Model.DependenciesMode = ShowDependenciesMode.None;
            Main.RefreshView();
        }

        private void LayoutCallGraphButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutCallGraphButton.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Method;
            Model.DependenciesMode = ShowDependenciesMode.None;
            Main.RefreshView();
        }

        private void LayoutInOrder_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutInOrder.Checked)
                return;

            Model.SequenceOrder = LayoutInOrder.Checked;
            Main.RefreshView();
        }

        private void LayoutClassCallsButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!LayoutClassCallsButton.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Class;
            Model.DependenciesMode = ShowDependenciesMode.None;
            Main.RefreshView();
        }

        private void Layout3dButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!Layout3dButton.Checked)
                return;

            Model.ViewLayout = LayoutType.ThreeD;
            Model.MapMode = TreeMapMode.Normal;
            Model.DependenciesMode = ShowDependenciesMode.None;

            Model.SecondarySizeLayout = Model.SizeLayout;
            Model.SizeLayout = SizeLayouts.Constant;

            Main.RefreshView();
        }

        private void MapDirectDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!MapDirectDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.TreeMap;
            Model.MapMode = TreeMapMode.Dependencies;
            Model.DependenciesMode = ShowDependenciesMode.Direct;
            Main.RefreshView();
        }

        private void MapAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!MapAllDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.TreeMap;
            Model.MapMode = TreeMapMode.Dependencies;
            Model.DependenciesMode = ShowDependenciesMode.All;
            Main.RefreshView();
        }

        private void GraphAllDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!GraphAllDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Dependencies;
            Model.DependenciesMode = ShowDependenciesMode.All;
            Main.RefreshView();
        }

        private void GraphDirectDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!GraphDirectDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Dependencies;
            Model.DependenciesMode = ShowDependenciesMode.Direct;
            Main.RefreshView();
        }

        private void GraphIntermediateDependencies_CheckedChanged(object sender, EventArgs e)
        {
            if (!GraphIntermediateDependencies.Checked)
                return;

            Model.ViewLayout = LayoutType.CallGraph;
            Model.GraphMode = CallGraphMode.Dependencies;
            Model.DependenciesMode = ShowDependenciesMode.Intermediates;
            Model.InterDependencies = Model.GetClassesFromFocusedNodes();
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
            for (int i = 0; i < XRay.CoveredFunctions.Count; i++)
                if (XRay.Nodes[i].StillInside == 0)
                    XRay.CoveredFunctions[i] = false;

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
            Main.RefreshView(true);
        }

        private void CallsRealTimeButton_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowCalls = CallsRealTimeButton.Checked;
            Main.RefreshView(true);
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
    }
}
