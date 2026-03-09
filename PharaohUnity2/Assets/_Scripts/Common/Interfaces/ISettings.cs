using AldaEngine;

namespace Pharaoh
{
    public interface ISettings
    {
        EGraphicsQuality Quality { get; }
        ELanguageCode Language { get; }
        bool Vibrations { get; }
        bool Sound { get; }
        bool Music { get; }
    }
}