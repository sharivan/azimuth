using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

using _3DTools;

using Image = System.Drawing.Image;
using Pen = System.Drawing.Pen;
using Point = System.Windows.Point;

namespace Azimuth
{
    public enum ProjectionType
    {
        AZIMUTHAL = 0, // Projeção Azimutal
        MERCATOR = 1, // Projeção de Mercator
        EQUIRECTANGULAR = 2, // Projeção Cilíndrica Equidistante
        PETERS = 3, // Projeção de Peters
        LAMBERT = 4, // Projeção de Lambert
        ROBINSON = 5, // Projeção de Robinson
    }

    public partial class frmAzimuth : Form
    {
        private const float WHELL_SCALE_FACTOR = 1.1F;

        private readonly Point3D SPHERE_ORIGIN = new Point3D(0, 0, 0);
        private const float SPHERE_RADIUS = 1;
        private const float EPSLON = 0.001F;

        private ProjectionType projectionType = ProjectionType.AZIMUTHAL;

        private Image backgrondImage;
        private Image foregroundImage;
        private Image drawingImage;

        private float scale = 1;
        private Point center;
        private float radiusX;
        private float radiusY;
        private float factor = 1;

        private int startX = 0;
        private int startY = 0;
        private int lastX = 0;
        private int lastY = 0;
        private bool moving = false;
        private bool resizing = false;
        private bool drawingWithPencil = false;
        private bool drawingWithLine = false;
        private bool drawingWithGeodesic = false;

        // Angle used to generate the morphism mesh between sphere and azimuthal projection.
        private float angle = 0;

        // Sphere mesh divisions.
        private int tDiv = 100;
        private int pDiv = 100;

        private TrackballDecorator decorator;
        private Interactive3DDecorator interactive;
        private MeshGeometry3D mesh;
        private Viewport3D viewPort;
        private ModelVisual3D visual3D;
        private PerspectiveCamera camera;
        private float initialCameraDistance;
        private Model3DGroup mainModel3Dgroup;
        private ImageBrush brush;

        private PointF[] points = new PointF[101];

        private frmProjectionTypeDialog openDialog;

        public frmAzimuth()
        {
            InitializeComponent();

            openDialog = new frmProjectionTypeDialog();

            pnl2D.MouseWheel += pnl2D_MouseWheel;
            pnl3D.MouseWheel += pnl3D_MouseWheel;

            decorator = new TrackballDecorator
            {
                Width = wpf3DHost.Width,
                Height = wpf3DHost.Height
            };

            interactive = new Interactive3DDecorator
            {
                Width = wpf3DHost.Width,
                Height = wpf3DHost.Height,
                ContainsInk = true
            };

            viewPort = new Viewport3D
            {
                Width = wpf3DHost.Width,
                Height = wpf3DHost.Height
            };

            interactive.Content = viewPort;
            decorator.Content = interactive;
            wpf3DHost.Child = decorator;

            typeof(System.Windows.Forms.Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pnl2D, new object[] { true });
        }

        // Update the 3D model texture.
        private void Update3DBrush()
        {
            var ms = new MemoryStream();
            using (var target = new Bitmap((int) (2 * radiusX), (int) (2 * radiusY)))
            {
                using (var g = Graphics.FromImage(target))
                {
                    g.DrawImage(backgrondImage, new RectangleF(0, 0, target.Width, target.Height),
                        new RectangleF((float) center.X - radiusX, (float) center.Y - radiusY, 2 * radiusX, 2 * radiusY),
                        GraphicsUnit.Pixel);

                    g.DrawImage(foregroundImage, new RectangleF(0, 0, target.Width, target.Height),
                        new RectangleF((float) center.X - radiusX, (float) center.Y - radiusY, 2 * radiusX, 2 * radiusY),
                        GraphicsUnit.Pixel);

                    g.DrawImage(drawingImage, new RectangleF(0, 0, target.Width, target.Height),
                        new RectangleF((float) center.X - radiusX, (float) center.Y - radiusY, 2 * radiusX, 2 * radiusY),
                        GraphicsUnit.Pixel);
                }

                target.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            }

            var bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();

            brush.ImageSource = bi;
        }

        // Define the 3D model.
        private void DefineModel(Model3DGroup model_group)
        {
            BuildMesh();
            brush = new ImageBrush();
            Update3DBrush();
            var material = new DiffuseMaterial(brush);
            var model = new GeometryModel3D(mesh, material)
            {
                BackMaterial = material
            };
            model_group.Children.Add(model);
        }

        private void DefineLights(Model3DGroup model_group)
        {
            var ambient_light = new AmbientLight(Colors.Gray);
            var directional_light =
                new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
            model_group.Children.Add(ambient_light);
            model_group.Children.Add(directional_light);
        }

        private void Compute2DFields()
        {
            foregroundImage?.Dispose();

            foregroundImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);

            drawingImage?.Dispose();

            drawingImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);

            center = new Point(drawingImage.Width / 2F, drawingImage.Height / 2F);

            switch (projectionType)
            {
                case ProjectionType.AZIMUTHAL:
                    radiusX = (float) Math.Min(center.X, center.Y);
                    radiusY = radiusX;
                    break;

                case ProjectionType.MERCATOR:
                    radiusX = (float) center.X;
                    radiusY = (float) center.Y;
                    break;
            }

            float factorX = (float) pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float) pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale;
        }

        private void frmAzimuth_Load(object sender, EventArgs e)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Azimuth.resources.azimuth1024x1024.png");
            backgrondImage = new Bitmap(stream);

            Compute2DFields();

            camera = new PerspectiveCamera
            {
                FieldOfView = 60
            };
            viewPort.Camera = camera;
            PositionCamera();

            mainModel3Dgroup = new Model3DGroup();
            DefineLights(mainModel3Dgroup);
            DefineModel(mainModel3Dgroup);

            visual3D = new ModelVisual3D
            {
                Content = mainModel3Dgroup
            };

            viewPort.Children.Add(visual3D);

            var transformGroup = new Transform3DGroup();
            var myRotateTransform3D = new RotateTransform3D();
            var myAxisRotation3d = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);

            myRotateTransform3D.CenterX = SPHERE_ORIGIN.X;
            myRotateTransform3D.CenterY = SPHERE_ORIGIN.Y;
            myRotateTransform3D.CenterZ = SPHERE_ORIGIN.Z;
            myRotateTransform3D.Rotation = myAxisRotation3d;

            transformGroup.Children.Add(myRotateTransform3D);

            var translateTransform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            transformGroup.Children.Add(myRotateTransform3D);

            mainModel3Dgroup.Transform = transformGroup;
        }

        private void PositionCamera()
        {
            camera.Position = new Point3D(0, 0, 3);
            camera.LookDirection = new Vector3D(0, 0, -1);
            camera.UpDirection = new Vector3D(0, 1, 0);

            var cameraPos = new al.Vector3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            initialCameraDistance = (float) cameraPos.Length();
        }

        // Transform virtual (image) coordinates to real (visual) coordinates.
        private PointF V2R(PointF point)
        {
            return new PointF((float) ((point.X - center.X) * factor + pnl2D.ClientRectangle.Width / 2), (float) ((point.Y - center.Y) * factor + pnl2D.ClientRectangle.Height / 2));
        }

        // Transform real coordinates to virtual coordinates.
        private PointF R2V(PointF point)
        {
            return new PointF((float) ((point.X - pnl2D.ClientRectangle.Width / 2) / factor + center.X), (float) ((point.Y - pnl2D.ClientRectangle.Height / 2) / factor + center.Y));
        }

        private void pnl2D_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF clipRect = e.ClipRectangle;

            float imageHalfWidth = drawingImage.Width / 2F;
            float imageHalfHeight = drawingImage.Height / 2F;
            var srcLeftTop = new PointF((float) (center.X - radiusX), (float) (center.Y - radiusY));
            var srcTopBottom = new PointF((float) (center.X + radiusX), (float) (center.Y + radiusY));
            var size = new SizeF(srcTopBottom.X - srcLeftTop.X, srcTopBottom.Y - srcLeftTop.Y);
            var srcRect = new RectangleF(srcLeftTop, size);
            var dstRect = new RectangleF
                (
                    pnl2D.ClientRectangle.Width / 2 - size.Width * factor / 2,
                    pnl2D.ClientRectangle.Height / 2 - size.Height * factor / 2,
                    size.Width * factor,
                    size.Height * factor
                );

            // Draw the background.
            g.DrawImage(backgrondImage, dstRect, srcRect, GraphicsUnit.Pixel);

            // Draw the foreground, containing the user traced lines.
            g.DrawImage(foregroundImage, dstRect, srcRect, GraphicsUnit.Pixel);

            // Draw the current tracing (line or geodesic).
            g.DrawImage(drawingImage, dstRect, srcRect, GraphicsUnit.Pixel);

            // If we defining the projection framework...
            if (btnDefineFramework.Checked)
            {
                using (var pen = new Pen(btnColor.BackColor, 2))
                {
                    // ...draw the framework.
                    switch (projectionType)
                    {
                        case ProjectionType.AZIMUTHAL:
                            g.DrawEllipse(pen, dstRect);
                            g.DrawLine(pen, dstRect.Left + dstRect.Width / 2, dstRect.Top, dstRect.Left + dstRect.Width / 2, dstRect.Top + dstRect.Height);
                            g.DrawLine(pen, dstRect.Left, dstRect.Top + dstRect.Height / 2, dstRect.Left + dstRect.Width, dstRect.Top + dstRect.Height / 2);
                            break;

                        case ProjectionType.MERCATOR:
                            g.DrawRectangle(pen, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height);
                            g.DrawLine(pen, dstRect.Left + dstRect.Width / 2, dstRect.Top, dstRect.Left + dstRect.Width / 2, dstRect.Top + dstRect.Height);
                            g.DrawLine(pen, dstRect.Left, dstRect.Top + dstRect.Height / 2, dstRect.Left + dstRect.Width, dstRect.Top + dstRect.Height / 2);
                            break;
                    }
                }
            }
        }

        private void pnl2D_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Zoom in/out from mouse wheel event at the 2D panel;

            if (e.Delta == 0)
                return;

            if (e.Delta > 0)
                scale /= WHELL_SCALE_FACTOR * e.Delta / SystemInformation.MouseWheelScrollDelta;
            else
                scale *= WHELL_SCALE_FACTOR * -e.Delta / SystemInformation.MouseWheelScrollDelta;

            float factorX = (float) pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float) pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale;

            pnl2D.Invalidate();
        }

        private void pnl3D_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Morphism defined by the mouse wheel event at the 3D panel.

            if (e.Delta == 0)
                return;

            angle += WHELL_SCALE_FACTOR * e.Delta / SystemInformation.MouseWheelScrollDelta;
            if (angle < 0)
                angle = 0;

            if (angle > 180)
                angle = 180;

            UpdateMesh();
        }

        // Clamp value to interval.
        public static double Clamp(double val, double min, double max)
        {
            return val < min ? min : val > max ? max : val;
        }

        // Transform degrees to radians.
        private static float DegToRad(float degrees)
        {
            return (float) (degrees * Math.PI / 180F);
        }

        // Transform radians to degrees.
        private static float RadToDeg(float radians)
        {
            return (float) (radians * 180 / Math.PI);
        }

        // Normalize an angle in degress to the range offset to offset + 360.
        private static float NormalizeDeegress(float degrees, float offset)
        {
            degrees -= offset;
            float modulus = Math.Abs(degrees) % 360;
            return degrees < 0 ? 360 - modulus + offset : modulus + offset;
        }

        // Normalize an angle in degress to the range 0 to 360.
        private static float NormalizeDeegress(float degrees)
        {
            return NormalizeDeegress(degrees, 0);
        }

        // Normalize an angle in radians to the range offset to offset + 2pi.
        private static float NormalizeRadians(float radians, float offset)
        {
            radians -= offset;
            float modulus = Math.Abs(radians) % (float) (2 * Math.PI);
            return radians < 0 ? 2 * (float) Math.PI - modulus + offset : modulus + offset;
        }

        // Normalize an angle in radians to the range 0 to 2pi.
        private static float NormalizeRadians(float radians)
        {
            return NormalizeRadians(radians, 0);
        }

        // Transform spherical coordinates of a point above the sphere surface to 3D cartesian coordinates. The parameter alpha is the angle of morphism between the sphere and its azimuthal projection.
        private static Point3D GetPosition(double alpha, double theta, double phi)
        {
            double alphaDivisor = 1 - alpha / Math.PI;

            double x;
            double y;
            double z;

            if (alphaDivisor == 0)
            {
                x = phi * SPHERE_RADIUS * Math.Cos(theta);
                y = phi * SPHERE_RADIUS * Math.Sin(theta);
                z = SPHERE_RADIUS;
            }
            else
            {
                double radius = SPHERE_RADIUS / alphaDivisor;
                phi *= alphaDivisor;

                x = radius * Math.Cos(theta) * Math.Sin(phi);
                y = radius * Math.Sin(theta) * Math.Sin(phi);
                z = radius * Math.Cos(phi) + SPHERE_RADIUS - radius;
            }

            return new Point3D(x, y, z);
        }

        // Convert polar coordinates to cartesian 2D coordinates.
        internal static Point GetPosition(double radius, double theta)
        {
            double x = radius * Math.Cos(theta);
            double y = radius * Math.Sin(theta);

            return new Point(x, y);
        }

        // Compute the normal vector of spherical surface at point defined by spherical coordinates.
        private static Vector3D GetNormal(double theta, double phi)
        {
            double x = Math.Cos(theta) * Math.Sin(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(phi);

            return new Vector3D(x, y, z);
        }

        // Compute the 2D cartesian coordinates of the projection of spherical coordinates from a point above the spherical surface.
        private Point GetTextureCoordinate(double theta, double phi)
        {
            switch (projectionType)
            {
                case ProjectionType.AZIMUTHAL:
                {
                    Point vec = GetPosition(phi * radiusX / Math.PI, theta);
                    return new Point(center.X + vec.X, center.Y - vec.Y);
                }

                case ProjectionType.MERCATOR:
                {
                    return phi <= Math.PI - 2 * Math.Atan(Math.Exp(radiusY * Math.PI / radiusX))
                            ? new Point(center.X - radiusX + (theta - Math.PI / 2) * radiusX / Math.PI, center.Y - radiusY)
                            : phi >= 2 * Math.Atan(Math.Exp(radiusY * Math.PI / radiusX))
                            ? new Point(center.X - radiusX + (theta - Math.PI / 2) * radiusX / Math.PI, center.Y + radiusY)
                            : new Point(center.X - radiusX + (theta - Math.PI / 2) * radiusX / Math.PI, center.Y - Math.Log(Math.Tan((Math.PI - phi) / 2)) * radiusX / Math.PI);
                }
            }

            return new Point(0, 0);
        }

        // Build the 3D mesh. It will be always a spherical surfuce when angle >= 0 and angle < 360. When angle == 360 the mesh will be a circle representing the projection.
        internal void BuildMesh()
        {
            double dt = DegToRad(360F) / tDiv;
            double dp = DegToRad(180F) / pDiv;

            double alpha = DegToRad(angle);

            mesh = new MeshGeometry3D();

            for (int pi = 0; pi <= pDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    mesh.Positions.Add(GetPosition(alpha, theta, phi));
                    mesh.Normals.Add(GetNormal(theta, phi));
                    mesh.TextureCoordinates.Add(GetTextureCoordinate(theta, phi)); // Here is the transformation from selected 2D projection coordinates to 3D coordinates.
                }
            }

            for (int pi = 0; pi < pDiv; pi++)
            {
                for (int ti = 0; ti < tDiv; ti++)
                {
                    int x0 = ti;
                    int x1 = ti + 1;
                    int y0 = pi * (tDiv + 1);
                    int y1 = (pi + 1) * (tDiv + 1);

                    mesh.TriangleIndices.Add(x0 + y0);
                    mesh.TriangleIndices.Add(x0 + y1);
                    mesh.TriangleIndices.Add(x1 + y0);

                    mesh.TriangleIndices.Add(x1 + y0);
                    mesh.TriangleIndices.Add(x0 + y1);
                    mesh.TriangleIndices.Add(x1 + y1);
                }
            }
        }

        // Update the 3D mesh.
        internal void UpdateMesh()
        {
            double dt = DegToRad(360F) / tDiv;
            double dp = DegToRad(180F) / pDiv;

            double alpha = DegToRad(angle);

            int index = 0;
            for (int pi = 0; pi <= pDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    mesh.Positions[index] = GetPosition(alpha, theta, phi);
                    mesh.Normals[index] = GetNormal(theta, phi);
                    mesh.TextureCoordinates[index] = GetTextureCoordinate(theta, phi);
                    index++;
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openDialog.ProjectionType = projectionType;
            DialogResult dr = openDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                projectionType = openDialog.ProjectionType;

                backgrondImage?.Dispose();

                backgrondImage = Image.FromFile(openDialog.FileName);

                Compute2DFields();

                pnl2D.Invalidate();
                UpdateMesh();
                Update3DBrush();
            }
        }

        private void frmAzimuth_Resize(object sender, EventArgs e)
        {
            int clientWidth = ClientSize.Width;
            int clientHeight = ClientSize.Height;

            pnl3D.Width = clientWidth / 2;
            pnl3D.Height = clientHeight;

            wpf3DHost.Width = pnl3D.ClientRectangle.Width;
            wpf3DHost.Height = pnl3D.ClientRectangle.Height;
            decorator.Width = wpf3DHost.Width;
            decorator.Height = wpf3DHost.Height;
            interactive.Width = decorator.Width;
            interactive.Height = decorator.Height;
            viewPort.Width = interactive.Width;
            viewPort.Height = interactive.Height;

            pnl2D.Left = clientWidth / 2;
            pnl2D.Width = clientWidth / 2;
            pnl2D.Height = clientHeight;

            float factorX = (float) pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float) pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale;

            pnl2D.Invalidate();
        }

        private void pnl2D_MouseDown(object sender, MouseEventArgs e)
        {
            startX = e.X;
            startY = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                if (btnDefineFramework.Checked)
                {
                    moving = true;
                }
                else if (btnPencil.Checked)
                {
                    drawingWithPencil = true;

                    using (var g = Graphics.FromImage(foregroundImage))
                    {
                        using (var pen = new Pen(btnColor.BackColor, 2))
                        {
                            PointF v = R2V(new PointF(startX, startY));
                            g.DrawLine(pen, v, new PointF(v.X + 1, v.Y + 1));
                        }
                    }

                    pnl2D.Invalidate();
                }
                else if (btnLine.Checked)
                {
                    drawingWithLine = true;
                }
                else if (btnGeodesic.Checked)
                {
                    drawingWithGeodesic = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (btnDefineFramework.Checked)
                    resizing = true;
            }

            lastX = startX;
            lastY = startY;
        }

        // Convert decimal degrees to a string containing the degrees, minutes and seconds.
        private static string DegToDegMinSec(float degrees)
        {
            int seconds = (int) Math.Abs(degrees * 3600);

            int s = seconds % 60;
            seconds /= 60;
            int m = seconds % 60;
            seconds /= 60;
            int d = seconds;

            return (degrees < 0 ? "-" : "") + d + "°" + m + "'" + s + "''";
        }

        private void pnl2D_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X == lastX && e.Y == lastY)
                return;

            PointF end = R2V(new PointF(e.X, e.Y));

            float theta0 = 0;
            float phi0 = 0;

            switch (projectionType)
            {
                case ProjectionType.AZIMUTHAL:
                {
                    PointF p = ToPolar(new PointF((float) (end.X - center.X), (float) (center.Y - end.Y)));
                    theta0 = p.Y;
                    phi0 = p.X / radiusX * (float) Math.PI;
                    break;
                }

                case ProjectionType.MERCATOR:
                {
                    theta0 = (float) ((end.X - center.X + radiusX) * Math.PI / radiusX - 3 * Math.PI / 2);

                    phi0 = end.Y >= center.Y
                        ? (float) (2 * Math.Atan(Math.Exp((end.Y - center.Y) * Math.PI / radiusX)))
                        : (float) (Math.PI - 2 * Math.Atan(Math.Exp((center.Y - end.Y) * Math.PI / radiusX)));

                    break;
                }
            }

            float latitude = 90 - RadToDeg(phi0);
            float longitude = RadToDeg(theta0) + 90;

            // Show in status bar the geographic coordinates of the mouse position above the projection (2D panel).
            tsslText.Text = "Latitute: " + DegToDegMinSec(Math.Abs(latitude)) + (latitude > 0 ? "N" : latitude < 0 ? "S" : "") + " Longitude: " + DegToDegMinSec(Math.Abs(longitude)) + (longitude > 0 ? "E" : longitude < 0 ? "W" : "");

            int dx = e.X - lastX;
            int dy = e.Y - lastY;

            if (moving)
            {
                center.Offset(dx, dy);
                pnl2D.Invalidate();
            }
            else if (resizing)
            {
                radiusX += dx / 2F;

                if (projectionType == ProjectionType.MERCATOR)
                {
                    radiusY += dy / 2F;
                    center.Offset(dx / 2F, dy / 2F);
                }

                pnl2D.Invalidate();
            }
            else if (drawingWithPencil)
            {
                using (var g = Graphics.FromImage(foregroundImage))
                {
                    using (var pen = new Pen(btnColor.BackColor, 2))
                    {
                        PointF start = R2V(new PointF(lastX, lastY));
                        g.DrawLine(pen, start, end);
                    }
                }

                pnl2D.Invalidate();
            }
            else if (drawingWithLine)
            {
                using (var g = Graphics.FromImage(drawingImage))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    using (var pen = new Pen(btnColor.BackColor, 2))
                    {
                        PointF start = R2V(new PointF(startX, startY));
                        g.DrawLine(pen, start, end);
                    }
                }

                pnl2D.Invalidate();
            }
            else if (drawingWithGeodesic)
            {
                PointF start = R2V(new PointF(startX, startY));

                switch (projectionType)
                {
                    case ProjectionType.AZIMUTHAL:
                        DrawGeodesicAzimuthal(start, end);
                        break;

                    case ProjectionType.MERCATOR:
                        DrawGeodesicMercator(start, end);
                        break;
                }

                pnl2D.Invalidate();
            }

            lastX = e.X;
            lastY = e.Y;
        }

        // Transform 2D cartesian coordinates to polar coordinates
        private PointF ToPolar(PointF p)
        {
            return new PointF((float) Math.Sqrt(p.X * p.X + p.Y * p.Y), (float) Math.Atan2(p.Y, p.X));
        }

        private PointF ToPointF(Point p)
        {
            return new PointF((float) p.X, (float) p.Y);
        }

        // Draw a geodesic curve on azimutal projection between the points p0 and p1.
        private void DrawGeodesicAzimuthal(PointF p0, PointF p1)
        {
            // Convert the points to polar coordinates.
            PointF p0p = ToPolar(new PointF((float) (p0.X - center.X), (float) (center.Y - p0.Y)));
            PointF p1p = ToPolar(new PointF((float) (p1.X - center.X), (float) (center.Y - p1.Y)));

            // Compute the spherical coordinates of the first point.
            float theta0 = NormalizeRadians(p0p.Y);
            float phi0 = p0p.X / radiusX * (float) Math.PI;

            // Compute the spherical coordinates of the second point.
            float theta1 = NormalizeRadians(p1p.Y);
            float phi1 = p1p.X / radiusX * (float) Math.PI;

            // Check the distances between the first angle and second angle and exchange the values if necessary. Its necessary for we get the minimal path.
            if (Math.Abs(theta0 + 2 * (float) Math.PI - theta1) < Math.Abs(theta1 - theta0))
            {
                float temp = theta0 + 2 * (float) Math.PI;
                theta0 = theta1;
                theta1 = temp;

                temp = phi0;
                phi0 = phi1;
                phi1 = temp;
            }

            // Just an angle correction applied to the destination coordinates.
            if (0 <= theta1 && theta1 < theta0 - Math.PI)
                theta1 += 2 * (float) Math.PI;

            // Compute the 3D cartesian coordinates of the first point (given by spherical coordinates).
            float sinPhi0 = (float) Math.Sin(phi0);
            float x0 = SPHERE_RADIUS * (float) Math.Cos(theta0) * sinPhi0;
            float y0 = SPHERE_RADIUS * (float) Math.Sin(theta0) * sinPhi0;
            float z0 = SPHERE_RADIUS * (float) Math.Cos(phi0);

            // Compute the 3D cartesian coordinates of the second point (given by spherical coordinates).
            float sinPhi1 = (float) Math.Sin(phi1);
            float x1 = SPHERE_RADIUS * (float) Math.Cos(theta1) * sinPhi1;
            float y1 = SPHERE_RADIUS * (float) Math.Sin(theta1) * sinPhi1;
            float z1 = SPHERE_RADIUS * (float) Math.Cos(phi1);

            // Compute the normal vector of two vectors defined by the 3D points.
            var normal = Vector3D.CrossProduct(new Vector3D(x0, y0, z0), new Vector3D(x1, y1, z1));

            // Compute the coeficients of the plane Ax + By + Cz = 0 passing by the origin of the sphere. The geodesic curve is the intersection of this plane whith the sphere.
            float A = (float) normal.X;
            float B = (float) normal.Y;
            float C = (float) normal.Z;

            bool flag = theta1 > theta0; // This boolean flag will be used bellow add or not the offset of phi angle.
            // Compute the curve points. The curve will be divided by 100 parts (containing 101 points).
            for (int i = 0; i <= 100; i++)
            {
                float dt = i / 100F; // delta theta
                float theta = theta0 * (1 - dt) + theta1 * dt;

                // The soluction of the plane intersection with the sphere, given by spherical coordinates, is given by the bellow expression, with radius fixed and theta varying.
                float phi = (float) Math.Atan2(-C, A * Math.Cos(theta) + B * Math.Sin(theta));

                // Just a correction to ensure the right value of phi. This offset is needed when the destination position angle is greater than source position angle.
                if (flag)
                    phi += (float) Math.PI;

                // Compute the 2D cartesian coordinates of azimuthal projection.
                points[i] = ToPointF(GetTextureCoordinate(theta, phi));
            }

            // Draw the curve points.
            using (var g = Graphics.FromImage(drawingImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
                using (var pen = new Pen(btnColor.BackColor, 2))
                {
                    g.DrawLines(pen, points);
                }
            }
        }

        // Draw a geodesic curve on Mercator projection between the points p0 and p1.
        private void DrawGeodesicMercator(PointF p0, PointF p1)
        {
            // Compute the spherical coordinates of the first point.
            float theta0 = NormalizeRadians((float) ((p0.X - center.X + radiusX) * Math.PI / radiusX - 3 * Math.PI / 2));

            float phi0 = p0.Y >= center.Y
                ? NormalizeRadians((float) (2 * Math.Atan(Math.Exp((p0.Y - center.Y) * Math.PI / radiusX))))
                : NormalizeRadians((float) (Math.PI - 2 * Math.Atan(Math.Exp((center.Y - p0.Y) * Math.PI / radiusX))));

            // Compute the spherical coordinates of the second point.
            float theta1 = NormalizeRadians((float) ((p1.X - center.X + radiusX) * Math.PI / radiusX - 3 * Math.PI / 2));

            float phi1 = p1.Y >= center.Y
                ? NormalizeRadians((float) (2 * Math.Atan(Math.Exp((p1.Y - center.Y) * Math.PI / radiusX))))
                : NormalizeRadians((float) (Math.PI - 2 * Math.Atan(Math.Exp((center.Y - p1.Y) * Math.PI / radiusX))));

            // Check the distances between the first angle and second angle and exchange the values if necessary. Its necessary for we get the minimal path.
            if (Math.Abs(theta0 + 2 * (float) Math.PI - theta1) < Math.Abs(theta1 - theta0))
            {
                float temp = theta0 + 2 * (float) Math.PI;
                theta0 = theta1;
                theta1 = temp;

                temp = phi0;
                phi0 = phi1;
                phi1 = temp;
            }

            // Just an angle correction applied to the destination coordinates.
            if (0 <= theta1 && theta1 < theta0 - Math.PI)
                theta1 += 2 * (float) Math.PI;

            // Compute the 3D cartesian coordinates of the first point (given by spherical coordinates).
            float sinPhi0 = (float) Math.Sin(phi0);
            float x0 = SPHERE_RADIUS * (float) Math.Cos(theta0) * sinPhi0;
            float y0 = SPHERE_RADIUS * (float) Math.Sin(theta0) * sinPhi0;
            float z0 = SPHERE_RADIUS * (float) Math.Cos(phi0);

            // Compute the 3D cartesian coordinates of the second point (given by spherical coordinates).
            float sinPhi1 = (float) Math.Sin(phi1);
            float x1 = SPHERE_RADIUS * (float) Math.Cos(theta1) * sinPhi1;
            float y1 = SPHERE_RADIUS * (float) Math.Sin(theta1) * sinPhi1;
            float z1 = SPHERE_RADIUS * (float) Math.Cos(phi1);

            // Compute the normal vector of two vectors defined by the 3D points.
            var normal = Vector3D.CrossProduct(new Vector3D(x0, y0, z0), new Vector3D(x1, y1, z1));

            // Compute the coeficients of the plane Ax + By + Cz = 0 passing by the origin of the sphere. The geodesic curve is the intersection of this plane whith the sphere.
            float A = (float) normal.X;
            float B = (float) normal.Y;
            float C = (float) normal.Z;

            using (var g = Graphics.FromImage(drawingImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
            }

            bool flag = theta1 > theta0; // This boolean flag will be used bellow add or not the offset of phi angle.
            float lastTheta = theta0;
            int j = 0;
            while (j <= 100)
            {
                PointF[] points = j == 0 ? this.points : new PointF[101 - j];

                // Compute the curve points. The curve will be divided by 100 parts (containing 101 points).
                for (int i = 0; i < points.Length && j < this.points.Length; i++, j++)
                {
                    float dt = j / 100F; // delta theta
                    float theta = theta0 * (1 - dt) + theta1 * dt;

                    // The soluction of the plane intersection with the sphere, given by spherical coordinates, is given by the bellow expression, with radius fixed and theta varying.
                    float phi = (float) Math.Atan2(-C, A * Math.Cos(theta) + B * Math.Sin(theta));

                    // Just a correction to ensure the right value of phi. This offset is needed when the destination position angle is greater than source position angle.
                    if (flag)
                        phi += (float) Math.PI;

                    if (Math.Abs(NormalizeRadians(theta, -3 * (float) Math.PI / 2) - NormalizeRadians(lastTheta, -3 * (float) Math.PI / 2)) > Math.PI)
                    {
                        var _points = new PointF[i];
                        Array.Copy(points, _points, i);
                        points = _points;
                        lastTheta = theta;
                        break;
                    }

                    // Compute the 2D cartesian coordinates of Mercator projection.
                    points[i] = ToPointF(GetTextureCoordinate(NormalizeRadians(theta, (float) Math.PI / 2), phi));

                    lastTheta = theta;
                }

                if (points.Length > 1)
                {
                    // Draw the curve points.
                    using (var g = Graphics.FromImage(drawingImage))
                    {
                        using (var pen = new Pen(btnColor.BackColor, 2))
                        {
                            g.DrawLines(pen, points);
                        }
                    }
                }
            }
        }

        private void pnl2D_MouseUp(object sender, MouseEventArgs e)
        {
            int dx = e.X - lastX;
            int dy = e.Y - lastY;
            lastX = e.X;
            lastY = e.Y;

            if (moving)
            {
                moving = false;
                center.Offset(dx, dy);
                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (resizing)
            {
                resizing = false;
                radiusX += dx;

                if (projectionType == ProjectionType.MERCATOR)
                    radiusY += dy;

                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (drawingWithPencil)
            {
                drawingWithPencil = false;

                PointF start = R2V(new PointF(lastX, lastY));
                PointF end = R2V(new PointF(e.X, e.Y));
                if (start != end)
                {
                    using (var g = Graphics.FromImage(foregroundImage))
                    {
                        using (var pen = new Pen(btnColor.BackColor, 2))
                        {
                            g.DrawLine(pen, start, end);
                        }
                    }

                    pnl2D.Invalidate();
                }

                Update3DBrush();
            }
            else if (drawingWithLine)
            {
                drawingWithLine = false;

                using (var g = Graphics.FromImage(drawingImage))
                {
                    PointF start = R2V(new PointF(startX, startY));
                    PointF end = R2V(new PointF(e.X, e.Y));
                    if (start != end)
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        using (var pen = new Pen(btnColor.BackColor, 2))
                        {
                            g.DrawLine(pen, start, end);
                        }
                    }

                    using (var g2 = Graphics.FromImage(foregroundImage))
                    {
                        g2.DrawImage(drawingImage, new PointF(0, 0));
                    }

                    g.Clear(System.Drawing.Color.Transparent);
                }

                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (drawingWithGeodesic)
            {
                drawingWithGeodesic = false;

                PointF start = R2V(new PointF(startX, startY));
                PointF end = R2V(new PointF(e.X, e.Y));
                if (start != end)
                {
                    switch (projectionType)
                    {
                        case ProjectionType.AZIMUTHAL:
                            DrawGeodesicAzimuthal(start, end);
                            break;

                        case ProjectionType.MERCATOR:
                            DrawGeodesicMercator(start, end);
                            break;
                    }
                }

                using (var g = Graphics.FromImage(foregroundImage))
                {
                    g.DrawImage(drawingImage, new PointF(0, 0));
                }

                using (var g = Graphics.FromImage(drawingImage))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                }

                pnl2D.Invalidate();
                Update3DBrush();
            }
        }

        private void btnSelection_Click(object sender, EventArgs e)
        {
            btnSelection.Checked = true;
            btnPencil.Checked = false;
            btnLine.Checked = false;
            btnGeodesic.Checked = false;

            if (btnDefineFramework.Checked)
            {
                btnDefineFramework.Checked = false;
                pnl2D.Invalidate();
            }

            pnl2D.Cursor = Cursors.Default;
        }

        private void btnPencil_Click(object sender, EventArgs e)
        {
            btnSelection.Checked = false;
            btnPencil.Checked = true;
            btnLine.Checked = false;
            btnGeodesic.Checked = false;

            if (btnDefineFramework.Checked)
            {
                btnDefineFramework.Checked = false;
                pnl2D.Invalidate();
            }

            pnl2D.Cursor = Cursors.Cross;
        }

        private void btnLine_Click(object sender, EventArgs e)
        {
            btnSelection.Checked = false;
            btnPencil.Checked = false;
            btnLine.Checked = true;
            btnGeodesic.Checked = false;

            if (btnDefineFramework.Checked)
            {
                btnDefineFramework.Checked = false;
                pnl2D.Invalidate();
            }

            pnl2D.Cursor = Cursors.Cross;
        }

        private void btnDefineFramework_Click(object sender, EventArgs e)
        {
            btnSelection.Checked = false;
            btnPencil.Checked = false;
            btnLine.Checked = false;
            btnGeodesic.Checked = false;
            btnDefineFramework.Checked = true;

            pnl2D.Cursor = Cursors.SizeAll;

            pnl2D.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            using (var g = Graphics.FromImage(drawingImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
            }

            using (var g = Graphics.FromImage(foregroundImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
            }

            pnl2D.Invalidate();
            Update3DBrush();
        }

        private void btnGeodesic_Click(object sender, EventArgs e)
        {
            btnSelection.Checked = false;
            btnPencil.Checked = false;
            btnLine.Checked = false;
            btnGeodesic.Checked = true;

            if (btnDefineFramework.Checked)
            {
                btnDefineFramework.Checked = false;
                pnl2D.Invalidate();
            }

            pnl2D.Cursor = Cursors.Cross;
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            DialogResult dr = colorDlg.ShowDialog();
            if (dr == DialogResult.OK)
            {
                btnColor.BackColor = colorDlg.Color;
            }

            if (btnDefineFramework.Checked)
                pnl2D.Invalidate();
        }
    }
}
