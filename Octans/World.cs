using System.Collections.Generic;

namespace Octans
{
    public class World
    {
        public World()
        {
            var s1 = new Sphere();
            var m = new Material {Color = new Color(0.8f, 1.0f, 0.6f), Diffuse = 0.7f, Specular = 0.2f};
            s1.SetMaterial(m);
            var s2 = new Sphere();
            s2.SetTransform(Matrix.Identity.Scale(0.5f, 0.5f, 0.5f));
            Objects = new[] {s1, s2};
            Lights = new[] {new PointLight(new Point(-10f, 10f, -10f), Colors.White)};
        }

        public IReadOnlyList<ISurface> Objects { get; }
        public IReadOnlyList<PointLight> Lights { get; private set; }

        public IReadOnlyList<Intersection> Intersect(in Ray ray)
        {
            var list = new List<Intersection>();
            foreach (var surface in Objects)
            {
                list.AddRange(surface.Intersect(ray));
            }

            return new Intersections(list);
        }

        public void SetLight(params PointLight[] lights)
        {
            Lights = lights;
        }
    }
}