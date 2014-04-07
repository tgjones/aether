using System;

namespace Aether.Reflection
{
    [Flags]
    public enum BxdfType
    {
        Reflection = 1 << 0,
        Transmission = 1 << 1,
        Diffuse = 1 << 2,
        Glossy = 1 << 3,
        Specular = 1 << 4,

        AllTypes = Diffuse | Glossy | Specular,

        AllReflection = Reflection | AllTypes,

        AllTransmission = Transmission | AllTypes,
        All = AllReflection | AllTransmission
    }
}