using System;
using System.Diagnostics;
using Octans.Camera;
using Octans.Geometry;
using Octans.IO;
using Octans.Light;
using Octans.Pipeline;
using Octans.Shading;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    public static class CornellBox
    {
        private static Group CreateYZRect(in Point corner,
                                          float yHeight,
                                          float zWidth,
                                          Material m)
        {
            var dY = new Vector(0, yHeight, 0);
            var dZ = new Vector(0, 0, zWidth);

            var t1 = new Triangle(corner, corner + dY, corner + dY + dZ);
            var t2 = new Triangle(corner, corner + dY + dZ, corner + dZ);

            t1.SetMaterial(m);
            t2.SetMaterial(m);

            var g = new Group(t1, t2);
            return g;
        }

        private static Group CreateXZRect(in Point corner,
                                          float xWidth,
                                          float zWidth,
                                          Material m)
        {
            var dX = new Vector(xWidth, 0, 0);
            var dZ = new Vector(0, 0, zWidth);

            var t1 = new Triangle(corner, corner + dZ, corner + dZ + dX);
            var t2 = new Triangle(corner, corner + dZ + dX, corner + dX);

            t1.SetMaterial(m);
            t2.SetMaterial(m);

            var g = new Group(t1, t2);
            return g;
        }

        private static Group CreateXYRect(in Point corner,
                                          float xWidth,
                                          float yHeight,
                                          Material m)
        {
            var dX = new Vector(xWidth, 0, 0);
            var dY = new Vector(0, yHeight, 0);

            var t1 = new Triangle(corner, corner + dY, corner + dY + dX);
            var t2 = new Triangle(corner, corner + dY + dX, corner + dX);

            t1.SetMaterial(m);
            t2.SetMaterial(m);

            var g = new Group(t1, t2);
            return g;
        }

        private static Cube CreateBox(float width, float height, in Point position, float rotateY, Material m)
        {
            var halfWidth = width / 2f;
            var halfHeight = height / 2f;
            var box = new Cube();
            box.SetTransform(Transforms
                             .RotateY(rotateY)
                             .Translate(1f,1f,1f)
                             .Scale(halfWidth, halfHeight, halfWidth)
                             .Translate(position.X, position.Y, position.Z));
            box.SetMaterial(m);
            return box;
        }

        public static void TestRender()
        {
            var w = BuildBox();

            var width = 200;
            var height = 200;
            //var transform = Transforms.View(new Point(278, 278, -800f), new Point(278, 278, 0), new Vector(0, 1, 0));
            //var c = new PinholeCamera(transform, 278f /400f, width, height);
            var c = new ApertureCamera(278f / 400f, width, height, 0.1f, new Point(278, 278, -800f), new Point(278, 278, 0), 850f);
            var ws = new ComposableWorldShading(5, GGXNormalDistribution.Instance, SchlickBeckmanGeometricShadow.Instance, SchlickFresnelFunction.Instance, w);
            //var ws = new PhongWorldShading(3, w);
            var scene = new Scene(c, ws);
            var aaa = new AdaptiveRenderer(2, 0.01f, scene);
            var canvas = new Canvas(width, height);

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            RenderContext.Render(canvas, aaa);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "cornell");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        private static World BuildBox()
        {
            var green = new Color(0.12f, 0.45f, 0.15f);
            var greenMaterial = new Material
            {
                Texture = SolidColor.Create(green),
                Roughness = 0.9f,
                SpecularColor = green,
                Ambient = 0
            };

            var red = new Color(0.65f, 0.05f, 0.05f);
            var redMaterial = new Material
            {
                Texture = SolidColor.Create(red),
                Roughness =0.9f,
                Ambient = 0,
                SpecularColor = red
            };

            var blue = new Color(0.15f,0.15f, 0.75f);
            var blueMetallic = new Material
            {
                Texture = SolidColor.Create(blue),
                Roughness = 0.3f,
                Ambient = 0,
                SpecularColor = blue,
                Metallic = 1f
            };

            var white = new Color(0.73f, 0.73f, 0.73f);
            var whiteMaterial = new Material
            {
                Texture = SolidColor.Create(white),
                Roughness = 0.9f,
                Ambient = 0,
                SpecularColor = white
            };

            var lightMaterial = new Material
            {
                Texture = SolidColor.Create(1f, 1f, 1f),
                Roughness = 1f,
                SpecularColor = Colors.White,
                //CastsShadows = false,
                Ambient = 1f
            };

            var right = CreateYZRect(new Point(555f, 0f, 0f), 555f, 555f, greenMaterial);
            var left = CreateYZRect(new Point(0, 0, 0), 555f, 555f, redMaterial);
            var top = CreateXZRect(new Point(0, 555f, 0), 555f, 555f, whiteMaterial);
            var bottom = CreateXZRect(new Point(0, 0f, 0), 555f, 555f, whiteMaterial);
          
            var back = CreateXYRect(new Point(0, 0f, 555f), 555f, 555f, whiteMaterial);
            var box1 = CreateBox(165f, 165f, new Point(290, 0, 100), 0.314f, whiteMaterial);
            var box2 = CreateBox(165f, 330f, new Point(100, 0, 260), -0.3925f, blueMetallic);
            var light = CreateXZRect(new Point(213f, 554.001f, 227f), 130f, 105f, lightMaterial);


            var g = new Group(right, left,top, bottom, back, box1, box2, light);
            g.Divide(1);

            var world = new World();
            world.SetObjects(g);
            world.SetLights(new AreaLight(new Point(213f, 554f, 227f), new Vector(130f,0,0), 4, new Vector(0,0,105f), 4, new Color(0.6f, 0.6f, 0.6f)));

            return world;
        }
    }
}