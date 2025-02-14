using VeldridGame.Abstractions;

namespace VeldridGame.Assets;

public class StandaloneAssetProvider :IAssetProvider
{
    private readonly AssetBundle[] _packages;
    private readonly Dictionary<Guid, EngineObject> _loaded = [];

    public StandaloneAssetProvider(string assetsPath)
    {

    }
    
    public bool HasAsset(Guid assetId)
    {
        throw new NotImplementedException();
    }

    public AssetRef<T> LoadAsset<T>(string relativeAssetPath) where T : EngineObject
    {
        Guid guid = GetGuidFromPath(relativeAssetPath);

        if (_loaded.TryGetValue(guid, out EngineObject? value))
        {
            return (T)value;
        }

        foreach (AssetBundle package in _packages)
        {
            if (package.TryGetAsset(relativeAssetPath, out EngineObject? asset))
            {
                _loaded[guid] = asset!;
                return (T)asset!;
            }
            
        }

        throw new FileNotFoundException($"Asset with path {relativeAssetPath} not found.");
    }
    
    public AssetRef<T> LoadAsset<T>(Guid assetId) where T : EngineObject
    {
        if (_loaded.TryGetValue(assetId, out EngineObject? value))
        {
            return (T)value;
        }

        foreach (AssetBundle package in _packages)
        {
            if (package.TryGetAsset(assetId, out EngineObject? asset))
            {
                _loaded[assetId] = asset!;
                return (T)asset!;
            }
        }

        throw new FileNotFoundException($"Asset with GUID {assetId} not found.");
    }
    
    public string GetPathFromGUID(Guid guid)
    {
        foreach (AssetBundle package in _packages)
        {
            if (package.TryGetPath(guid, out string? path))
            {
                return path!;
            }
        }

        throw new FileNotFoundException($"Asset with GUID {guid} not found.");
    }

    public Guid GetGuidFromPath(string relativeAssetPath)
    {
        foreach (AssetBundle package in _packages)
        {
            if (package.TryGetGuid(relativeAssetPath, out Guid guid))
            {
                return guid;
            }
        }

        throw new FileNotFoundException($"Asset with path {relativeAssetPath} not found.");
    }
}