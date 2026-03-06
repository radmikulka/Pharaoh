using AldaEngine;

namespace Pharaoh
{
    public interface ISettings
    {
        EGraphicsQuality Quality { get; }
        ELanguageCode Language { get; }
    }
}