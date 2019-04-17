using Octans.Texture;

namespace Octans
{
    public class MaterialInfo
    {
        public MaterialInfo()
        {
            Texture = new SolidColor(Colors.White);
            Ambient = 0.1f;
            Diffuse = 0.9f;
            Specular = 0.9f;
            Shininess = 200f;
            Reflective = 0f;
            Transparency = 0f;
            RefractiveIndex = 1f;
            Roughness = 1f;
            Metallic = 0f;
            SpecularColor = Colors.White;
            CastsShadows = true;
        }

        public bool CastsShadows { get; set; }

        public float Shininess { get; set; }

        public float Specular { get; set; }

        public float Diffuse { get; set; }

        public float Ambient { get; set; }

        public ITexture Texture { get; set; }
        public float Reflective { get; set; }
        public float Transparency { get; set; }
        public float RefractiveIndex { get; set; }
        public float Roughness { get; set; }

        public float Metallic { get; set; }

        public Color SpecularColor { get; set; }
    }
}