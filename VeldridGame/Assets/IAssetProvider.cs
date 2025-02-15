using VeldridGame.Abstractions;
using VeldridGame.Resources;

namespace VeldridGame.Assets;

public interface IAssetProvider
{
    AssetRef<T> LoadAsset<T>(string relativeAssetPath) where T : EngineObject;
}