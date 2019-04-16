using System;

namespace Octans.Reflection
{
    [Flags]
    public enum BxDFType
    {
        None = 0,
        Reflection = 1 << 0,
        Transmission = 1 << 1,
        Diffuse = 1 << 2,
        Glossy = 1 << 3,
        Specular = 1 << 4,
        All = Reflection | Transmission | Diffuse | Glossy | Specular
    }
}