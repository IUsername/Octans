namespace Octans
{
    public class Material
    {
        public Material()
        {
            Color = new Color(1f, 1f, 1f);
            Ambient = 0.1f;
            Diffuse = 0.9f;
            Specular = 0.9f;
            Shininess = 200f;
        }

        public float Shininess { get; set; }

        public float Specular { get; set; }

        public float Diffuse { get; set; }

        public Color Color { get; set; }

        public float Ambient { get; set; }
    }
}