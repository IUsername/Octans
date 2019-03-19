using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Octans
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TestRender();
            //TeapotTest();
            SolidTestRender();
        }

        private static Solid RoundedCube(float radius, Material mat)
        {
            Solid SolidFaces(float r)
            {
                var cY = new Cube();
                cY.SetTransform(Transforms.Scale(1f - r, 1f, 1f - r));
                cY.SetMaterial(mat);

                var cX = new Cube();
                cX.SetTransform(Transforms.Scale(1f, 1f - r, 1f - r));
                cX.SetMaterial(mat);
                var su = new Solid(SolidOp.Union, cY, cX);

                var cZ = new Cube();
                cZ.SetTransform(Transforms.Scale(1f - r, 1f - r, 1f));
                cZ.SetMaterial(mat);
                return new Solid(SolidOp.Union, su, cZ);
            }

            Solid Union(IShape a, IShape b) => new Solid(SolidOp.Union, a, b);

            Cylinder CreateCylinder(float r, Point from, Material material, Matrix rotation)
            {
                var dist = 1f - r;
                var fOffset = from * dist;
                var e = new Cylinder {Minimum = 0f, Maximum = 2f * dist / r, IsClosed = true};
                e.SetMaterial(material);
                e.SetTransform(Transforms.Scale(r).Apply(rotation).Translate(fOffset.X, fOffset.Y, fOffset.Z));
                return e;
            }

            Sphere Corner(float r, Point corner, Material material)
            {
                var dist = 1f - r;
                var offset = corner * dist;
                var sphere = new Sphere();
                sphere.SetTransform(Transforms.Scale(r).Translate(offset.X, offset.Y, offset.Z));
                sphere.SetMaterial(material);
                return sphere;
            }

            Cylinder EdgeX(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Transforms.RotateZ(-MathF.PI / 2));

            Cylinder EdgeY(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Matrix.Identity);

            Cylinder EdgeZ(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Transforms.RotateX(MathF.PI / 2));

            var s = SolidFaces(radius);
            var points = new[]
            {
                new Point(-1, 1, -1),
                new Point(1, 1, -1),
                new Point(1, 1, 1),
                new Point(-1, 1, 1),
                new Point(-1, -1, -1),
                new Point(1, -1, -1),
                new Point(1, -1, 1),
                new Point(-1, -1, 1)
            };

            foreach (var point in points)
            {
                s = Union(s, Corner(radius, point, mat));
                if (point.X < 0f)
                {
                    s = Union(s, EdgeX(radius, point, mat));
                }

                if (point.Y < 0f)
                {
                    s = Union(s, EdgeY(radius, point, mat));
                }

                if (point.Z < 0f)
                {
                    s = Union(s, EdgeZ(radius, point, mat));
                }
            }

            return s;
        }

        private static Solid CutPips(Solid solid, Material material)
        {
            Sphere PipSphere(Point point, Material mat)
            {
                var sphere = new Sphere();
                sphere.SetTransform(Transforms.Scale(0.2f).Translate(point.X, point.Y, point.Z));
                sphere.SetMaterial(mat);
                return sphere;
            }

            Solid Diff(IShape s, IShape child) => new Solid(SolidOp.Difference, s, child);

            var offset = 1.15f;

            // 1
            solid = Diff(solid, PipSphere(new Point(0, 0, -offset), material));

            //2
            solid = Diff(solid, PipSphere(new Point(0.4f, offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, offset, -0.4f), material));

            //3
            solid = Diff(solid, PipSphere(new Point(-offset, 0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-offset, -0.4f, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-offset, 0.0f, 0.0f), material));

            //4        
            solid = Diff(solid, PipSphere(new Point(offset, 0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, -0.4f, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, -0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, 0.4f, -0.4f), material));

            //5       
            solid = Diff(solid, PipSphere(new Point(0.4f, -offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -offset, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(0.4f, -offset, -0.4f), material));

            //6       
            solid = Diff(solid, PipSphere(new Point(0.4f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.0f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.4f, -0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.0f, -0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -0.4f, offset), material));

            return solid;
        }

        private static Group ToGroup(IShape shape)
        {
            var g = new Group();
            g.AddChild(shape);
            return g;
        }

        public static void SolidTestRender()
        {
            var radius = 0.2f;
            var red = new Material {Pattern = new SolidColor(new Color(1, 0, 0)), Reflective = 0.3f};
            var blue = new Material {Pattern = new SolidColor(new Color(0, 0, 1)), Reflective = 0.3f};
            var yellow = new Material {Pattern = new SolidColor(new Color(1, 1, 0)), Reflective = 0.3f};
            var white = new Material {Pattern = new SolidColor(new Color(1, 1, 1)), Reflective = 0.3f};
            var blackPip = new Material {Pattern = new SolidColor(new Color(0.1f, 0.1f, 0.1f))};
            var whitePip = new Material {CastsShadows = true};

            var d1 = ToGroup(CutPips(RoundedCube(radius, blue), whitePip));
            var d2 = ToGroup(CutPips(RoundedCube(radius, red), whitePip));
            var d3 = ToGroup(CutPips(RoundedCube(radius, white), blackPip));
            var d4 = ToGroup(CutPips(RoundedCube(radius, yellow), blackPip));

            d1.SetTransform(Transforms.RotateY(-2.2f).TranslateY(1f).Scale(0.5f));
            d2.SetTransform(Transforms.RotateZ(MathF.PI / 2f).TranslateY(1f).TranslateX(2f).TranslateZ(1f).Scale(0.5f));
            d3.SetTransform(Transforms.RotateY(0.5f).TranslateY(1f).TranslateX(-4f).TranslateZ(1f).Scale(0.5f));
            d4.SetTransform(Transforms.RotateY(-0.2f).TranslateY(3f).TranslateX(0.2f).TranslateZ(1.25f).Scale(0.5f));

            var lightGray = new Color(0.6f,0.6f,0.6f);
            var darkGray = new Color(0.4f,0.4f,0.4f);
            var s1 = new StripePattern(lightGray, darkGray);
            var s2 = new StripePattern(lightGray, darkGray);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositePattern(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var floor = new Cube
            {
                Material =
                {
                    Pattern = pattern,
                    Specular = 0.1f,
                    Diffuse = 0.3f,
                    Reflective = 0.15f,
                    Ambient = 0f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1f).Scale(20f));

            var w = new World();
            //w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetLights(new AreaLight(new Point(-3, 6, -4), new Vector(2f, 0, 0), 3, new Vector(0, 2f, 0), 3, new Color(1.4f, 1.4f, 1.4f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));
            //w.SetLights(new AreaLight(new Point(-10, 10, -10), new Vector(1,0,0), 4, new Vector(0,1,0), 3, Colors.White));
            w.SetObjects(floor, d1, d2, d3, d4);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "solid");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
            Console.ReadKey();
        }

        public static void TestRender()
        {
            var s1 = new StripePattern(Colors.White, Colors.Black);
            var s2 = new StripePattern(Colors.White, Colors.Black);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositePattern(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var stripe = new StripePattern(new Color(0.9f, 0, 0), new Color(0.0f, 0.0f, 0.9f));
            stripe.SetTransform(Transforms.Scale(0.25f, 0.25f, 0.25f).RotateY(MathF.PI / 4));
            var perlin = new PerlinRippleCompositePattern(stripe, 0.8f);
            perlin.SetTransform(Transforms.Scale(0.1f, 0.1f, 0.1f));

            var floor = new Cube
            {
                Material =
                {
                    Pattern = pattern,
                    Specular = 0.1f,
                    Reflective = 0.1f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1).Scale(20f));

            var middle = new Sphere
                {Material = {Pattern = perlin, Diffuse = 0.7f, Specular = 1f, Reflective = 0.4f, Shininess = 600}};
            middle.SetTransform(Transforms.Translate(-0.5f, 1f, 0.1f));

            var right = new Sphere
            {
                Material =
                {
                    Pattern = new SolidColor(new Color(0.5f, 1f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f,
                    Reflective = 0.2f
                }
            };
            right.SetTransform(Transforms.Translate(1.5f, 0.5f, -0.5f) * Transforms.Scale(0.5f, 0.5f, 0.5f));

            var left = new Sphere
            {
                Material =
                {
                    Pattern = new SolidColor(new Color(0.9f, 0.9f, 1f)), Diffuse = 0.05f, Specular = 0.9f,
                    Transparency = 0.9f, RefractiveIndex = 1.52f, Reflective = 1.4f, Ambient = 0.11f, Shininess = 300
                }
            };
            left.SetTransform(Transforms.Translate(-2.1f, 0.33f, 0.5f) * Transforms.Scale(0.33f, 0.33f, 0.33f));

            var cube = new Cube
            {
                Material = {Pattern = new GradientPattern(new Color(1f, 0, 0), new Color(1f, 0.8f, 0f))}
            };
            cube.Material.Pattern.SetTransform(Transforms.TranslateX(-0.5f).Scale(2f).RotateZ(MathF.PI / 2f));
            cube.SetTransform(Transforms.RotateY(MathF.PI / 4f).Translate(2.5f, 1f, 3.6f).Scale(1f, 1f, 1f));

            var cone = new Cone
            {
                IsClosed = true,
                Minimum = -1f,
                Maximum = 0f,
                Material =
                {
                    Pattern = new SolidColor(new Color(0.5f, 1f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f,
                    Reflective = 0.2f
                }
            };
            cone.SetTransform(Transforms.Scale(0.6f, 2f, 0.6f).Translate(1.5f, 2.0f, 0));

            var cylinder = new Cylinder
            {
                Minimum = 0f,
                Maximum = 3f,
                IsClosed = true,
                Material = {Reflective = 0.33f, Specular = 0.9f, Diffuse = 0.1f, Ambient = 0.01f, Shininess = 100}
            };
            cylinder.SetTransform(Transforms.Translate(-3f, 0f, 3.5f));

            var t = new Triangle(new Point(0, 0, 0), new Point(1, 0.5f, 0), new Point(0.5f, 1f, 1f))
            {
                Material = {Pattern = new GradientPattern(new Color(0f, 1, 0), new Color(0f, 0f, 1f))}
            };
            t.SetTransform(Transforms.Translate(1f, 2f, 1f));

            var yellowGlass = new Material
            {
                Pattern = new SolidColor(new Color(1f, 0.8f, 0f)),
                Reflective = 0.4f,
                RefractiveIndex = 0.95f,
                //Transparency = 0.95f,
                Shininess = 300,
                Specular = 0.9f,
                Ambient = 0.1f,
                Diffuse = 0.3f
            };
            var co = new Cylinder
            {
                Minimum = -0.01f,
                Maximum = 0.01f,
                IsClosed = true
            };
            co.SetTransform(Transforms.Scale(1.5f, 1f, 1.5f));
            co.SetMaterial(yellowGlass);

            var ci = new Cylinder
            {
                Minimum = -0.1f,
                Maximum = 0.1f,
                IsClosed = true
            };
            ci.SetTransform(Transforms.Scale(1.2f, 1f, 1.2f));
            ci.SetMaterial(yellowGlass);

            var s = new Solid(SolidOp.Difference, co, ci);
            s.SetTransform(Transforms.RotateZ(-0.2f).RotateX(-0.1f).Translate(-0.5f, 1f, 0.1f));

            var gl = new Group();
            gl.AddChild(middle);
            gl.AddChild(left);
            gl.AddChild(cylinder);
            gl.AddChild(s);

            var gr = new Group();
            gr.AddChild(cube);
            gr.AddChild(cone);
            gr.AddChild(t);


            var gt = new Group();
            gt.AddChild(gl);
            gt.AddChild(gr);
            gt.SetTransform(Transforms.TranslateZ(-0.5f));

            var gf = new Group();
            gf.AddChild(floor);

            var w = new World();
            w.SetLights(new AreaLight(new Point(-2, 4, -7), new Vector(0.01f, 0, 0), 2, new Vector(0, 0.4f, 0), 4, new Color(1.4f,1.4f,1.4f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));
            w.SetObjects(gt, gf);

            var x = 1200;
            var y = 800;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
            Console.ReadKey();
        }

        private static void TeapotTest()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(d => string.Equals("bin", d));
            var projectPath = string.Join(Path.DirectorySeparatorChar.ToString(),
                                          pathItems.Take(pathItems.Length - pos - 1));


            //path = Path.Combine(projectPath, "teapot-low.obj");
            path = Path.Combine(projectPath, "teapot.obj");


            Console.WriteLine("Loading file {0}...", path);

            var data = ObjFile.ParseFile(path);
            Console.WriteLine("File parsed...");

            var g = data.Groups[0];
            g.SetTransform(Transforms.Scale(0.10f).RotateX(-MathF.PI / 2f).RotateY(MathF.PI / 8f));

            var chrome = new Material
            {
                Pattern = new SolidColor(new Color(1f, 0.7f, 0.75f)),
                Reflective = 0.35f,
                //RefractiveIndex = 1.1f,
                //Transparency = 0.8f,
                Ambient = 0.05f,
                Diffuse = 0.45f,
                Shininess = 100f,
                Specular = 0.9f
            };

            ApplyMaterialToChildren(g, chrome);

            var checkerboard = new Material
            {
                Pattern = new CheckerPattern(new Color(0.5f, 0.5f, 0.5f), new Color(0.7f, 0.7f, 0.7f)),
                Reflective = 0.2f,
                Ambient = 0.2f,
                Diffuse = 0.3f
            };

            checkerboard.Pattern.SetTransform(Transforms.Scale(0.025f));

            var floor = new Cube();
            floor.SetMaterial(checkerboard);
            var fg = new Group();
            fg.AddChild(floor);
            fg.SetTransform(Transforms.TranslateY(-1).Scale(5f));

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(fg, g);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 3.0f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
            Console.ReadKey();
        }

        private static void ApplyMaterialToChildren(Group group, Material material)
        {
            foreach (var child in group.Children)
            {
                child.SetMaterial(material);
            }
        }
    }
}