using System.Security.Cryptography;
using System.Text;
using VeldridGame.Abstractions;
using VeldridGame.Rendering;
using VeldridGame.Resources;

namespace VeldridGame.Assets;

public sealed class AssetProvider(Game game, string assetRootPath) : IAssetProvider, IDisposable
{
    private readonly Dictionary<Guid, EngineObject> _loaded = new();

    public AssetRef<T> LoadAsset<T>(string relativeAssetPath) where T : EngineObject
    {
        var assetId = GetGuidFromPath(relativeAssetPath);

        if(!_loaded.ContainsKey(assetId))
        {
            var assetFile = new FileInfo(Path.Combine(assetRootPath, relativeAssetPath));
            if (!assetFile.Exists)
            {
                throw new FileNotFoundException($"Asset with path {relativeAssetPath} not found.");
            }

            var readAssetsFromFile = ReadAssetsFromFile<T>(assetFile);
            _loaded[assetId] = readAssetsFromFile;
        }
        
        return (T)_loaded[assetId];
    }

    public void Dispose()
    {
        foreach (var asset in _loaded.Values)
        {
            asset.Dispose();
        }
        _loaded.Clear();
    }

    private static Guid GetGuidFromPath(string relativePath)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(relativePath));
        return new Guid(hash);
    }

    private EngineObject ReadAssetsFromFile<T>(FileInfo assetFile) where T : EngineObject
    {
        return typeof(T) switch
        {
            // case ShaderProgram => LoadShaderProgram(assetId);
            { } t when t == typeof(Texture2D) => LoadTexture(assetFile),
            { } t when t == typeof(Mesh) => LoadMesh(assetFile),
            // case Material => LoadMaterial(assetId);
            _ => throw new NotImplementedException($"Asset type {typeof(T)} not implemented."),
        };
    }

    private EngineObject LoadMesh(FileInfo assetFile)
    {
        return Mesh.Load(game, assetFile.FullName);
    }

    private EngineObject LoadTexture(FileInfo assetFile)
    {
        return new Texture2D(game, assetFile.FullName);
    }
}