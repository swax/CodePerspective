using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XLibrary
{
    /// <summary>
    /// Interaction logic for TreePanelWPF.xaml
    /// </summary>
    public partial class TreePanelWPF : UserControl
    {
        internal XNodeIn Root;

        SolidColorBrush UnknownBrush    = new SolidColorBrush(Colors.Black);
        SolidColorBrush NamespaceBrush  = new SolidColorBrush(Colors.DarkBlue);
        SolidColorBrush ClassBrush      = new SolidColorBrush(Colors.DarkGreen);
        SolidColorBrush MethodBrush     = new SolidColorBrush(Colors.DarkRed);
        SolidColorBrush FieldBrush      = new SolidColorBrush(Colors.Black);


        public TreePanelWPF()
        {
            InitializeComponent();

        }

        private void DrawCube(Model3DGroup scene, SolidColorBrush brush, RectangleD rect, double bottom, double height)
        {
            Model3DGroup cube = new Model3DGroup();
            
            Point3D p0 = new Point3D(rect.X,                bottom, rect.Y);
            Point3D p1 = new Point3D(rect.X + rect.Width,   bottom, rect.Y);
            Point3D p2 = new Point3D(rect.X + rect.Width,   bottom, rect.Y + rect.Height);
            Point3D p3 = new Point3D(rect.X,                bottom, rect.Y + rect.Height);

            Point3D p4 = new Point3D(rect.X,                bottom + height, rect.Y);
            Point3D p5 = new Point3D(rect.X + rect.Width,   bottom + height, rect.Y);
            Point3D p6 = new Point3D(rect.X + rect.Width,   bottom + height, rect.Y + rect.Height);
            Point3D p7 = new Point3D(rect.X,                bottom + height, rect.Y + rect.Height);

            //front side triangles
            cube.Children.Add(CreateTriangleModel(brush, p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(brush, p3, p6, p7));

            //right side triangles
            cube.Children.Add(CreateTriangleModel(brush, p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(brush, p2, p5, p6));

            //back side triangles
            cube.Children.Add(CreateTriangleModel(brush, p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(brush, p1, p4, p5));

            //left side triangles
            cube.Children.Add(CreateTriangleModel(brush, p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(brush, p0, p7, p4));

            //top side triangles
            cube.Children.Add(CreateTriangleModel(brush, p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(brush, p7, p5, p4));

            //bottom side triangles
            //cube.Children.Add(CreateTriangleModel(brush, p2, p3, p0));
            //cube.Children.Add(CreateTriangleModel(brush, p2, p0, p1));

            scene.Children.Add(cube);
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            /*ClearViewport();

            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(5, 0, 0);
            Point3D p2 = new Point3D(5, 0, 5);
            Point3D p3 = new Point3D(0, 0, 5);
            Point3D p4 = new Point3D(0, 5, 0);
            Point3D p5 = new Point3D(5, 5, 0);
            Point3D p6 = new Point3D(5, 5, 5);
            Point3D p7 = new Point3D(0, 5, 5);

            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));

            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));

            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));

            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));

            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));

            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;

            this.mainViewport.Children.Add(model);*/
        }

        public void Init(XNodeIn root)
        {
            Root = root;

            Resize();
        }

        bool x;

        public void Resize()
        {
            if (x)
                return;
            x = true;

            SizeNode(Root, new RectangleD() { Width = 100, Height = 100 });

            ClearViewport();

            Model3DGroup scene = new Model3DGroup();

            DrawNode(scene, Root, 0);

            ModelVisual3D model = new ModelVisual3D();
            model.Content = scene;

            mainViewport.Children.Add(model);
        }

        public void Redraw()
        {
            // change scene objects but dont, re-create
        }

        private void SizeNode(XNodeIn root, RectangleD area)
        {
            if (!root.Show)
                return;

            root.Area = area;

            var nodes = root.Nodes.Cast<XNodeIn>()
                            .Where(n => n.Show)
                            .Select(n => n as InputValue);

            List<Sector> sectors = new TreeMap().Plot(nodes, area.Size);

            foreach (Sector sector in sectors)
            {
                XNodeIn node = sector.OriginalValue as XNodeIn;

                sector.Rect.X += area.X;
                sector.Rect.Y += area.Y;

                SizeNode(node, sector.Rect.Contract(4));
            }
        }

        private void DrawNode(Model3DGroup scene, XNodeIn node, int depth)
        {
            if (!node.Show)
                return;

            SolidColorBrush cubeBrush = UnknownBrush;

            switch (node.ObjType)
            {
                case XObjType.Namespace:
                    cubeBrush = NamespaceBrush;
                    break;

                case XObjType.Class:
                    cubeBrush = ClassBrush;
                    break;

                case XObjType.Method:
                    cubeBrush = MethodBrush;
                    break;

                case XObjType.Field:
                    cubeBrush = FieldBrush;
                    break;
            }

            // blue selection area
            /*SolidBrush rectBrush = NothingBrush;
            if (node.Selected)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                rectBrush = OverBrushes[depth];
            }*/

            DrawCube(scene, cubeBrush, node.Area, depth, depth + 1);
            //buffer.FillRectangle(rectBrush, node.Area);

            // red hit check if function is hit
            /*if (XRay.HitFunctions != null)
            {
                int value = XRay.Conflicts[node.ID];
                if (value > 0)
                    buffer.FillRectangle(ConflictBrush[value], node.Area);
                else
                {
                    value = XRay.HitFunctions[node.ID];
                    if (value > 0)
                        buffer.FillRectangle(HitBrush[value], node.Area);
                }
            }*/

            //buffer.DrawRectangle(rectPen, node.Area.X, node.Area.Y, node.Area.Width, node.Area.Height);

            foreach (XNodeIn sub in node.Nodes)
                DrawNode(scene, sub, depth + 1);

            // after drawing children, draw instance tracking on top of it all
            if (XRay.TrackInstances && node.ObjType == XObjType.Class)
            {
                /*if (XRay.InstanceCount[node.ID] > 0)
                {
                    string count = XRay.InstanceCount[node.ID].ToString();
                    Rectangle x = new Rectangle(node.Area.Location, buffer.MeasureString(count, InstanceFont).ToSize());

                    if (node.Area.Contains(x))
                    {
                        buffer.FillRectangle(NothingBrush, x);
                        buffer.DrawString(count, InstanceFont, InstanceBrush, node.Area.Location.X + 2, node.Area.Location.Y + 2);
                    }
                }*/
            }
        }

        private Model3DGroup CreateTriangleModel(SolidColorBrush brush, Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(brush);
            GeometryModel3D model = new GeometryModel3D(mesh, material);

            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private void ClearViewport()
        {
            var remove = mainViewport.Children.Cast<ModelVisual3D>().Where(c => c.Content.GetType() != typeof(DirectionalLight)).ToList();

            remove.ForEach(i => mainViewport.Children.Remove(i));
        }


        private void SetCamera()
        {
            PerspectiveCamera camera = mainViewport.Camera as PerspectiveCamera;

     
            /*Point3D position = new Point3D(
                Convert.ToDouble(cameraPositionXTextBox.Text),
                Convert.ToDouble(cameraPositionYTextBox.Text),
                Convert.ToDouble(cameraPositionZTextBox.Text)
            );

            Vector3D lookDirection = new Vector3D(
                Convert.ToDouble(lookAtXTextBox.Text),
                Convert.ToDouble(lookAtYTextBox.Text),
                Convert.ToDouble(lookAtZTextBox.Text)
            );

            camera.Position = position;
            camera.LookDirection = lookDirection;*/
        }
    }
}
