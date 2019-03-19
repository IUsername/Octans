namespace Octans
{
    public class Material
    {
        public Material()
        {
            Pattern = new SolidColor(Colors.White);
            Ambient = 0.1f;
            Diffuse = 0.9f;
            Specular = 0.9f;
            Shininess = 200f;
            Reflective = 0f;
            Transparency = 0f;
            RefractiveIndex = 1f;
            CastsShadows = true;
        }

        public bool CastsShadows { get; set; }

        public float Shininess { get; set; }

        public float Specular { get; set; }

        public float Diffuse { get; set; }

        public float Ambient { get; set; }

        public IPattern Pattern { get; set; }
        public float Reflective { get; set; }
        public float Transparency { get; set; }
        public float RefractiveIndex { get; set; }
    }
}