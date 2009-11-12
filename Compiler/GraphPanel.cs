using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace XBuilder
{
    public partial class GraphPanel : UserControl
    {
        bool DoRedraw = true;
        bool DoResize = true;
        Bitmap DisplayBuffer;

        List<Node> Nodes = new List<Node>();
        List<Spring> Springs = new List<Spring>();

        SolidBrush NodeBrush = new SolidBrush(Color.Blue);
        Pen SpringPen = new Pen(Color.Black);

        Stopwatch Watch = new Stopwatch();
        long LastRun;


        Node Wall1;
        Node Wall2;

        const double TicksPerSec = (double)TimeSpan.TicksPerSecond;



        // constants
        const float NodeRadius = 10;

        internal float NodeCharge = 2;

        
        public GraphPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Watch.Start();

            Reset();

            new Thread(PhysicsThread).Start();
        }

        internal void Reset()
        {
            Nodes.Clear();
            Springs.Clear();

            Wall1 = new Node() { Position = new PointF(0, 0) };
            Wall2 = new Node() { Position = new PointF(Width, 0) };

            Node a = new Node() { Position = new PointF(100, 100) };
            Node b = new Node() { Position = new PointF(200, 100) };

            Nodes.Add(a);
            Nodes.Add(b);

            Springs.Add(new Spring() { EndA = a, EndB = b });
        }

        private void GraphPanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if (!DoRedraw && !DoResize)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(Color.White);

            foreach (Spring spring in Springs)
                buffer.DrawLine(SpringPen, spring.EndA.Position, spring.EndB.Position);

            foreach (Node node in Nodes)
                buffer.FillEllipse(NodeBrush, node.Position.X, node.Position.Y, NodeRadius, NodeRadius);

            

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
            DoResize = false;
        }

        private void PhysicsTimer_Tick(object sender, EventArgs e)
        {
            DoRedraw = true;
            Refresh();
        }

        void PhysicsThread()
        {
            while (true)
            {
                // double ticks = Watch.ElapsedTicks - LastRun;
                double deltaT = 0.01; // ticks / TicksPerSec;

                // spring: F = -kx 
                // attraction: F = (q1*q2) / r2

                double direction = 1;
                float distance = 0;

                foreach (Node node in Nodes)
                {
                    node.Acceleration = new PointD();

                    // calculate all the forces acting on the node
                    foreach (Node actor in Nodes.Where(n => n != node))
                    {
                        direction = actor.Position.X > node.Position.X ? -1 : 1;
                        distance = Math.Abs(node.Position.X - actor.Position.X);

                        if (distance > 10)
                            node.Acceleration.X += direction * (NodeCharge * NodeCharge) /
                                Math.Pow(distance, 2);

                        direction = actor.Position.Y > node.Position.Y ? -1 : 1;
                        distance = Math.Abs(node.Position.Y - actor.Position.Y);

                        if (distance > 10)
                            node.Acceleration.Y += direction * (NodeCharge * NodeCharge) /
                                Math.Pow(distance, 2);
                    }

                    // walls 
                    distance = Math.Abs(node.Position.X - Wall1.Position.X);

                    if (distance > 10)
                        node.Acceleration.X += 1 * (NodeCharge * NodeCharge) /
                            Math.Pow(distance, 2);

                    distance = Math.Abs(node.Position.X - Wall2.Position.X);

                    if (distance > 10)
                        node.Acceleration.X += -1 * (NodeCharge * NodeCharge) /
                            Math.Pow(distance, 2);

                    // set new velocity / position
                    node.Velocity.X += node.Acceleration.X * Math.Pow(deltaT, 2);
                    node.Velocity.Y += node.Acceleration.Y * Math.Pow(deltaT, 2);

                    node.Position.X += (float)(node.Velocity.X * deltaT);
                    node.Position.Y += (float)(node.Velocity.Y * deltaT);
                }


                LastRun = Watch.ElapsedTicks;

            }
        }

        private void GraphPanel_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                DoResize = true;
                Invalidate();
            }
        }
    }

    struct PointD
    {
        internal double X;
        internal double Y;
    }

    class Node
    {
        internal PointF Position;
        internal PointD Velocity;

        internal PointD Acceleration;
    }

    class Spring
    {
        internal Node EndA;
        internal Node EndB;
    }
}
