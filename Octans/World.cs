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
        public IReadOnlyList<ILight> Lights { get; private set; }

        public IIntersections Intersect(in Ray ray)
        {
            var builder = Intersections.Builder();
            for (var index = 0; index < Objects.Count; index++)
            {
                var shape = Objects[index];
                builder.AddRange(shape.Intersects(ray));
            }

            return builder.ToIntersections();
        }

        public void SetLights(params ILight[] lights)
        {
            Lights = lights;
        }

        public void SetObjects(params IShape[] shapes)
        {
            Objects = shapes;
        }

        public void AddObject(IShape shape)
        {
            var list = new List<IShape>(Objects) {shape};
            Objects = list.ToArray();
        }

        public static World Default()
        {
            var w = new World();
            var s1 = new Sphere();
            var m = new Material
                {Pattern = new SolidColor(new Color(0.8f, 1.0f, 0.6f)), Diffuse = 0.7f, Specular = 0.2f};
            s1.SetMaterial(m);
            var s2 = new Sphere();
            s2.SetTransform(Matrix.Identity.Scale(0.5f, 0.5f, 0.5f));
            w.SetObjects(s1, s2);
            w.SetLights(new PointLight(new Point(-10f, 10f, -10f), Colors.White));
            return w;
        }
    }
}