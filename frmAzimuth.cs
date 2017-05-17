using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

using Point = System.Windows.Point;
using Pen = System.Drawing.Pen;
using Image = System.Drawing.Image;

using _3DTools;

namespace Azimuth
{
    public partial class frmAzimuth : Form
    {
        private const int fontSize = 20;
        private const float WHELL_SCALE_FACTOR = 1.1F;

        private readonly Point3D SPHERE_ORIGIN = new Point3D(0, 0, 0);
        private const float SPHERE_RADIUS = 1;
        private const float EPSLON = 0.001F;

        private Image backgrondImage;
        private Image foregroundImage;
        private Image drawingImage;

        private float scale2D = 1;
        private Point center2D;
        private float radius2D;
        private float factor = 1;

        private int startX = 0;
        private int startY = 0;
        private int lastX = 0;
        private int lastY = 0;
        private bool moving2D = false;
        private bool resizing2D = false;
        private bool drawingWithPencil2D = false;
        private bool drawingWithLine2D = false;
        private bool drawingWithGeodesic2D = false;

        private float angle = 0;

        private int tDiv = 100;
        private int pDiv = 100;

        private TrackballDecorator decorator;
        private Interactive3DDecorator interactive;
        private MeshGeometry3D mesh;
        private Viewport3D viewPort;
        private InteractiveVisual3D interactiveVisual3D;
        private ModelVisual3D visual3D;
        private PerspectiveCamera camera;
        private float initialCameraDistance;
        private Model3DGroup mainModel3Dgroup;
        private ImageBrush brush;

        public frmAzimuth()
        {
            InitializeComponent();

            pnl2D.MouseWheel += pnl2D_MouseWheel;
            pnl3D.MouseWheel += pnl3D_MouseWheel;

            decorator = new TrackballDecorator();
            decorator.Width = wpf3DHost.Width;
            decorator.Height = wpf3DHost.Height;

            interactive = new Interactive3DDecorator();
            interactive.Width = wpf3DHost.Width;
            interactive.Height = wpf3DHost.Height;
            interactive.ContainsInk = true;

            viewPort = new Viewport3D();
            viewPort.Width = wpf3DHost.Width;
            viewPort.Height = wpf3DHost.Height;

            interactive.Content = viewPort;
            decorator.Content = interactive;
            wpf3DHost.Child = decorator;

            typeof(System.Windows.Forms.Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pnl2D, new object[] { true });
        }

        private void Update3DBrush()
        {
            MemoryStream ms = new MemoryStream();
            Bitmap target = new Bitmap((int) (2 * radius2D), (int) (2 * radius2D));
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(backgrondImage, new RectangleF(0, 0, target.Width, target.Height),
                    new RectangleF((float) center2D.X - radius2D, (float) center2D.Y - radius2D, 2 * radius2D, 2 * radius2D),
                    GraphicsUnit.Pixel);

                g.DrawImage(foregroundImage, new RectangleF(0, 0, target.Width, target.Height),
                    new RectangleF((float)center2D.X - radius2D, (float)center2D.Y - radius2D, 2 * radius2D, 2 * radius2D),
                    GraphicsUnit.Pixel);

                g.DrawImage(drawingImage, new RectangleF(0, 0, target.Width, target.Height),
                    new RectangleF((float)center2D.X - radius2D, (float)center2D.Y - radius2D, 2 * radius2D, 2 * radius2D),
                    GraphicsUnit.Pixel);
            }

            target.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();

            brush.ImageSource = bi;
        }

        private void DefineModel(Model3DGroup model_group)
        {
            BuildMesh();
            brush = new ImageBrush();
            Update3DBrush();
            DiffuseMaterial material = new DiffuseMaterial(brush);
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.BackMaterial = material;
            model_group.Children.Add(model);
        }

        private void DefineLights(Model3DGroup model_group)
        {
            AmbientLight ambient_light = new AmbientLight(Colors.Gray);
            DirectionalLight directional_light =
                new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
            model_group.Children.Add(ambient_light);
            model_group.Children.Add(directional_light);
        }

        private void frmAzimuth_Load(object sender, EventArgs e)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Azimuth.resources.azimuth1024x1024.png");
            backgrondImage = new Bitmap(stream);
            foregroundImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);
            drawingImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);

            center2D = new Point(drawingImage.Width / 2F, drawingImage.Height / 2F);
            radius2D = (float) Math.Min(center2D.X, center2D.Y);

            float factorX = (float)pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float)pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale2D;

            camera = new PerspectiveCamera();
            camera.FieldOfView = 60;
            viewPort.Camera = camera;
            PositionCamera();

            mainModel3Dgroup = new Model3DGroup();
            DefineLights(mainModel3Dgroup);
            DefineModel(mainModel3Dgroup);

            visual3D = new ModelVisual3D();
            visual3D.Content = mainModel3Dgroup;

            viewPort.Children.Add(visual3D);

            Transform3DGroup transformGroup = new Transform3DGroup();
            RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D myAxisRotation3d = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);

            myRotateTransform3D.CenterX = SPHERE_ORIGIN.X;
            myRotateTransform3D.CenterY = SPHERE_ORIGIN.Y;
            myRotateTransform3D.CenterZ = SPHERE_ORIGIN.Z;
            myRotateTransform3D.Rotation = myAxisRotation3d;

            transformGroup.Children.Add(myRotateTransform3D);

            TranslateTransform3D translateTransform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            transformGroup.Children.Add(myRotateTransform3D);

            mainModel3Dgroup.Transform = transformGroup;
        }

        private void PositionCamera()
        {
            camera.Position = new Point3D(0, 0, 3);
            camera.LookDirection = new Vector3D(0, 0, -1);
            camera.UpDirection = new Vector3D(0, 1, 0);

            al.Vector3D cameraPos = new al.Vector3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            initialCameraDistance = (float) cameraPos.Length();
        }

        private PointF V2R(PointF point)
        {
            return new PointF((float) ((point.X - center2D.X) * factor + pnl2D.ClientRectangle.Width / 2), (float) ((point.Y - center2D.Y) * factor + pnl2D.ClientRectangle.Height / 2));
        }

        private PointF R2V(PointF point)
        {
            return new PointF((float)((point.X - pnl2D.ClientRectangle.Width / 2) / factor + center2D.X), (float)((point.Y - pnl2D.ClientRectangle.Height / 2) / factor + center2D.Y));
        }

        private void pnl2D_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF clipRect = e.ClipRectangle;

            float imageHalfWidth = drawingImage.Width / 2F;
            float imageHalfHeight = drawingImage.Height / 2F;
            PointF srcLeftTop = new PointF((float) (center2D.X - radius2D), (float) (center2D.Y - radius2D));
            PointF srcTopBottom = new PointF((float) (center2D.X + radius2D), (float) (center2D.Y + radius2D ));
            SizeF size = new SizeF(srcTopBottom.X - srcLeftTop.X, srcTopBottom.Y - srcLeftTop.Y);
            RectangleF srcRect = new RectangleF(srcLeftTop, size);
            RectangleF dstRect = new RectangleF
                (
                    pnl2D.ClientRectangle.Width / 2 - size.Width * factor / 2,
                    pnl2D.ClientRectangle.Height / 2 - size.Height * factor / 2,
                    size.Width * factor, 
                    size.Height * factor
                );
 
            g.DrawImage(backgrondImage, dstRect, srcRect, GraphicsUnit.Pixel);
            g.DrawImage(foregroundImage, dstRect, srcRect, GraphicsUnit.Pixel);
            g.DrawImage(drawingImage, dstRect, srcRect, GraphicsUnit.Pixel);

            if (btnDefineFramework.Checked)
                using (Pen pen = new Pen(btnColor.BackColor, 2))
                {
                    g.DrawEllipse(pen, dstRect);
                    g.DrawLine(pen, dstRect.Left + dstRect.Width / 2, dstRect.Top, dstRect.Left + dstRect.Width / 2, dstRect.Top + dstRect.Height);
                    g.DrawLine(pen, dstRect.Left, dstRect.Top + dstRect.Height / 2, dstRect.Left + dstRect.Width, dstRect.Top + dstRect.Height / 2);
                }
        }

        private void pnl2D_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta == 0)
                return;

            if (e.Delta > 0)
                scale2D /= WHELL_SCALE_FACTOR * e.Delta / SystemInformation.MouseWheelScrollDelta;
            else
                scale2D *= WHELL_SCALE_FACTOR * -e.Delta / SystemInformation.MouseWheelScrollDelta;

            float factorX = (float)pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float)pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale2D;

            pnl2D.Invalidate();
        }

        private void pnl3D_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta == 0)
                return;

            angle += WHELL_SCALE_FACTOR * e.Delta / SystemInformation.MouseWheelScrollDelta;
            if (angle < 0)
                angle = 0;

            if (angle > 180)
                angle = 180;

            UpdateMesh();
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min)
                return min;

            if (val > max)
                return max;

            return val;
        }

        private static float DegToRad(float degrees)
        {
            return (float) (degrees * Math.PI / 180F);
        }

        private static float RadToDeg(float radians)
        {
            return (float) (radians * 180 / Math.PI);
        }

        private static float NormalizeDeegress(float degrees)
        {
            float modulus = Math.Abs(degrees) % 360;
            if (degrees < 0)
                return 360 - modulus;

            return modulus;
        }

        private static float NormalizeRadians(float radians)
        {
            float modulus = Math.Abs(radians) % (float) (2 * Math.PI);
            if (radians < 0)
                return 2 * (float) Math.PI - modulus;

            return modulus;
        }

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

        internal static Point GetPosition(double radius, double theta)
        {
            double x = radius * Math.Cos(theta);
            double y = radius * Math.Sin(theta);

            return new Point(x, y);
        }

        private static Vector3D GetNormal(double theta, double phi)
        {
            double x = Math.Cos(theta) * Math.Sin(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(phi);

            return new Vector3D(x, y, z);
        }

        private Point GetTextureCoordinate(double theta, double phi)
        {
            Point vec = GetPosition(phi * radius2D / Math.PI, theta);
            return new Point(vec.X + center2D.X, center2D.Y - vec.Y);
        }

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
                    mesh.TextureCoordinates.Add(GetTextureCoordinate(theta, phi));
                }
            }

            for (int pi = 0; pi < pDiv; pi++)
            {
                for (int ti = 0; ti < tDiv; ti++)
                {
                    int x0 = ti;
                    int x1 = (ti + 1);
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
            DialogResult dr = openFileDlg.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (backgrondImage != null)
                    backgrondImage.Dispose();

                backgrondImage = Image.FromFile(openFileDlg.FileName);

                if (foregroundImage != null)
                    foregroundImage.Dispose();

                foregroundImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);

                if (drawingImage != null)
                    drawingImage.Dispose();

                drawingImage = new Bitmap(backgrondImage.Width, backgrondImage.Height);

                center2D = new Point(backgrondImage.Width / 2F, backgrondImage.Height / 2F);
                radius2D = (float)Math.Min(center2D.X, center2D.Y);

                float factorX = (float)pnl2D.ClientRectangle.Width / drawingImage.Width;
                float factorY = (float)pnl2D.ClientRectangle.Height / drawingImage.Height;
                factor = Math.Min(factorX, factorY) / scale2D;

                pnl2D.Invalidate();
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

            float factorX = (float)pnl2D.ClientRectangle.Width / drawingImage.Width;
            float factorY = (float)pnl2D.ClientRectangle.Height / drawingImage.Height;
            factor = Math.Min(factorX, factorY) / scale2D;

            pnl2D.Invalidate();
        }

        private void pnl2D_MouseDown(object sender, MouseEventArgs e)
        {
            startX = e.X;
            startY = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                if (btnDefineFramework.Checked)
                    moving2D = true;
                else if (btnPencil.Checked)
                {
                    drawingWithPencil2D = true;

                    using (Graphics g = Graphics.FromImage(foregroundImage))
                    {
                        using (Pen pen = new Pen(btnColor.BackColor, 2))
                        {
                            PointF v = R2V(new PointF(startX, startY));
                            g.DrawLine(pen, v, new PointF(v.X + 1, v.Y + 1));
                        }
                    }

                    pnl2D.Invalidate();
                }
                else if (btnLine.Checked)
                {
                    drawingWithLine2D = true;

                    using (Graphics g = Graphics.FromImage(drawingImage))
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        using (Pen pen = new Pen(btnColor.BackColor, 2))
                        {
                            PointF v = R2V(new PointF(startX, startY));
                            g.DrawLine(pen, v, new PointF(v.X + 1, v.Y + 1));
                        }
                    }

                    pnl2D.Invalidate();
                }
                else if (btnGeodesic.Checked)
                {
                    drawingWithGeodesic2D = true;

                    using (Graphics g = Graphics.FromImage(drawingImage))
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        using (Pen pen = new Pen(btnColor.BackColor, 2))
                        {
                            PointF v = R2V(new PointF(startX, startY));
                            g.DrawLine(pen, v, new PointF(v.X + 1, v.Y + 1));
                        }
                    }

                    pnl2D.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (btnDefineFramework.Checked)
                    resizing2D = true;
            }

            lastX = startX;
            lastY = startY;
        }

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

            PointF p = ToPolar(new PointF((float)(end.X - center2D.X), (float)(center2D.Y - end.Y)));

            float theta0 = p.Y;
            float phi0 = p.X / radius2D * (float) Math.PI;

            float latitude = 90 - RadToDeg(phi0);
            float longitude = RadToDeg(theta0) + 90;

            tsslText.Text = "Latitute: " + DegToDegMinSec(Math.Abs(latitude)) + (latitude > 0 ? "N" : latitude < 0 ? "S" : "") + " Longitude: " + DegToDegMinSec(Math.Abs(longitude)) + (longitude > 0 ? "E" : longitude < 0 ? "W" : "");

            int dx = e.X - lastX;
            int dy = e.Y - lastY;

            if (moving2D)
            {
                center2D.Offset(dx, dy);
                pnl2D.Invalidate();
            }
            else if (resizing2D)
            {
                radius2D += dx;
                pnl2D.Invalidate();
            }
            else if (drawingWithPencil2D)
            {
                using (Graphics g = Graphics.FromImage(foregroundImage))
                {
                    using (Pen pen = new Pen(btnColor.BackColor, 2))
                    {
                        PointF start = R2V(new PointF(lastX, lastY));
                        g.DrawLine(pen, start, new PointF(end.X + 1, end.Y + 1));
                    }
                }

                pnl2D.Invalidate();
            }
            else if (drawingWithLine2D)
            {
                using (Graphics g = Graphics.FromImage(drawingImage))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    using (Pen pen = new Pen(btnColor.BackColor, 2))
                    {
                        PointF start = R2V(new PointF(startX, startY));
                        g.DrawLine(pen, start, new PointF(end.X + 1, end.Y + 1));
                    }
                }

                pnl2D.Invalidate();
            }
            else if (drawingWithGeodesic2D)
            {
                PointF start = R2V(new PointF(startX, startY));
                DrawGeodesic(start, end);
                pnl2D.Invalidate();
            }

            lastX = e.X;
            lastY = e.Y;
        }

        private PointF ToPolar(PointF p)
        {
            return new PointF((float) Math.Sqrt(p.X * p.X + p.Y * p.Y), (float) Math.Atan2(p.Y, p.X));
        }

        private PointF ToPointF(Point p)
        {
            return new PointF((float) p.X, (float) p.Y);
        }

        private void DrawGeodesic(PointF p0, PointF p1)
        {
            PointF p0p = ToPolar(new PointF((float) (p0.X - center2D.X), (float) (center2D.Y - p0.Y)));
            PointF p1p = ToPolar(new PointF((float) (p1.X - center2D.X), (float) (center2D.Y - p1.Y)));

            float theta0 = p0p.Y;
            float phi0 = p0p.X / radius2D * (float) Math.PI;
            if (phi0 < 0)
            {
                phi0 = -phi0;
                theta0 += (float) Math.PI;
            }

            theta0 = NormalizeRadians(theta0);

            float theta1 = p1p.Y;
            float phi1 = p1p.X / radius2D * (float) Math.PI;
            if (phi1 < 0)
            {
                phi1 = -phi1;
                theta1 += (float)Math.PI;
            }

            theta1 = NormalizeRadians(theta1);

            if (Math.Abs(theta0 + 2 * (float)Math.PI - theta1) < Math.Abs(theta1 - theta0))
            {
                float temp = theta0 + 2 * (float)Math.PI;
                theta0 = theta1;
                theta1 = temp;

                temp = phi0;
                phi0 = phi1;
                phi1 = temp;
            }

            float sinPhi0 = (float) Math.Sin(phi0);
            float x0 = SPHERE_RADIUS * (float) Math.Cos(theta0) * sinPhi0;
            float y0 = SPHERE_RADIUS * (float) Math.Sin(theta0) * sinPhi0;
            float z0 = SPHERE_RADIUS * (float) Math.Cos(phi0);

            float sinPhi1 = (float) Math.Sin(phi1);
            float x1 = SPHERE_RADIUS * (float) Math.Cos(theta1) * sinPhi1;
            float y1 = SPHERE_RADIUS * (float) Math.Sin(theta1) * sinPhi1;
            float z1 = SPHERE_RADIUS * (float) Math.Cos(phi1);

            Vector3D normal = Vector3D.CrossProduct(new Vector3D(x0, y0, z0), new Vector3D(x1, y1, z1));

            float A = (float) normal.X;
            float B = (float) normal.Y;
            float C = (float) normal.Z;

            using (Graphics g = Graphics.FromImage(drawingImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
                using (Pen pen = new Pen(btnColor.BackColor, 2))
                {
                    /*if (Math.Abs(C) < EPSLON)
                    {
                        g.DrawLine(pen, ToPointF(GetTextureCoordinate(theta0, phi0)), ToPointF(GetTextureCoordinate(theta1, phi1)));
                    }
                    else*/
                    {
                        PointF lastP = ToPointF(GetTextureCoordinate(theta0, phi0));
                        for (int i = 0; i < 101; i++)
                        {
                            float t = i / 100F;

                            float theta = theta0 * (1 - t) + theta1 * t;
                            float phi = (float)Math.Atan2(-C, A * Math.Cos(theta) + B * Math.Sin(theta));

                            if (theta1 > theta0)
                                phi += (float)Math.PI;

                            PointF p = ToPointF(GetTextureCoordinate(theta, phi));

                            g.DrawLine(pen, lastP, new PointF(p.X + 1, p.Y + 1));

                            lastP = p;
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

            if (moving2D)
            {
                moving2D = false;
                center2D.Offset(dx, dy);
                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (resizing2D)
            {
                resizing2D = false;
                radius2D += dx;
                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (drawingWithPencil2D)
            {
                drawingWithPencil2D = false;

                PointF start = R2V(new PointF(lastX, lastY));
                PointF end = R2V(new PointF(e.X, e.Y));
                if (start != end)
                {
                    using (Graphics g = Graphics.FromImage(foregroundImage))
                    {
                        using (Pen pen = new Pen(btnColor.BackColor, 2))
                        {
                            g.DrawLine(pen, start, new PointF(end.X + 1, end.Y + 1));
                        }
                    }

                    pnl2D.Invalidate();
                    Update3DBrush();
                }
            }
            else if (drawingWithLine2D)
            {
                drawingWithLine2D = false;

                using (Graphics g = Graphics.FromImage(drawingImage))
                {
                    PointF start = R2V(new PointF(startX, startY));
                    PointF end = R2V(new PointF(e.X, e.Y));
                    if (start != end)
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        using (Pen pen = new Pen(btnColor.BackColor, 2))
                        {
                            g.DrawLine(pen, start, new PointF(end.X + 1, end.Y + 1));
                        }
                    }

                    using (Graphics g2 = Graphics.FromImage(foregroundImage))
                    {
                        g2.DrawImage(drawingImage, new PointF(0, 0));
                    }

                    g.Clear(System.Drawing.Color.Transparent);
                }

                pnl2D.Invalidate();
                Update3DBrush();
            }
            else if (drawingWithGeodesic2D)
            {
                drawingWithGeodesic2D = false;

                PointF start = R2V(new PointF(startX, startY));
                PointF end = R2V(new PointF(e.X, e.Y));
                if (start != end)
                    DrawGeodesic(start, end);

                using (Graphics g = Graphics.FromImage(foregroundImage))
                {
                    g.DrawImage(drawingImage, new PointF(0, 0));
                }

                using (Graphics g = Graphics.FromImage(drawingImage))
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
            using (Graphics g = Graphics.FromImage(drawingImage))
            {
                g.Clear(System.Drawing.Color.Transparent);
            }

            using (Graphics g = Graphics.FromImage(foregroundImage))
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
