using System;
using System.Collections.Generic;

namespace Octans
{
    public class World
    {
        public World()
        {
            Objects = Array.Empty<IShape>();
            Lights = Array.Empty<PointLight>();
        }

        public IReadOnlyList<IShape> Objects { get; private set; }
        public IReadOnlyList<PointLight> Lights { get; private set; }

        public IReadOnlyList<Intersection> Intersect(in Ray ray)
        {
            var list = new List<Intersection>();
            foreach (var surface in Objects)
            {
                list.AddRange(surface.Intersects(ray));
            }

            return new Intersections(list);
        }

        public void SetLights(params PointLight[] lights)
        {
            Lights = lights;
        }

        public void SetObjects(params IShape[] shapes)
        {
            Objects = shapes;
        }

        public static World Default()
        {
            var w = new World();
            var s1 = new Sphere();
            var m = new Material { Color = new Color(0.8f, 1.0f, 0.6f), Diffuse = 0.7f, Specular = 0.2f };
            s1.SetMaterial(m);
            var s2 = new Sphere();
            s2.SetTransform(Matrix.Identity.Scale(0.5f, 0.5f, 0.5f));
            w.SetObjects(s1, s2 );
            w.SetLights(new PointLight(new Point(-10f, 10f, -10f), Colors.White));
            return w;
        }
    }
}