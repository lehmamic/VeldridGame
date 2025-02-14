using VeldridGame.Abstractions;
using VeldridGame.Utils;

namespace VeldridGame.Assets;

public abstract class AssetBundle
{
   protected readonly Dictionary<Guid, string> GuidToPath = new();
   protected readonly Dictionary<string, Guid> PathToGuid = new(StringComparer.OrdinalIgnoreCase);
   
   public bool HasAsset(Guid assetId) => GuidToPath.ContainsKey(assetId);
   
   public bool TryGetGuid(string assetPath, out Guid guid) => PathToGuid.TryGetValue(NormalizePath(assetPath), out guid);

   public bool TryGetPath(Guid guid, out string? assetPath) => GuidToPath.TryGetValue(guid, out assetPath);

   public abstract bool TryGetAsset(Guid guid, out EngineObject? asset);

   public abstract bool TryGetAsset(string assetPath, out EngineObject? asset);
   
   protected string NormalizePath(string path)
   {
      if (string.IsNullOrWhiteSpace(path))
      {
         throw new ArgumentException("Path cannot be null or empty.");
      }

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      path = path.Replace(@"\\", @"\").Replace(@"\", @"/");

      // If it starts with a / remove it
      if (path.StartsWith("/"))
      {
         path = path[1..];
      }

      return path;
   }
}

public class DirectoryAssetBundle : AssetBundle
{
   private readonly DirectoryInfo _dataDirectory;

   public DirectoryAssetBundle(DirectoryInfo dataDirectory)
   {
      _dataDirectory = dataDirectory;
      
        foreach (var file in dataDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
           var relativePath = GetRelativePath(dataDirectory, file);
           var assetId = relativePath.ComputeGuid();
           
           GuidToPath[assetId] = relativePath;
           PathToGuid[relativePath] = assetId;
        }
   }

   public override bool TryGetAsset(Guid guid, out EngineObject? asset)
   {
      throw new NotImplementedException();
   }

   public override bool TryGetAsset(string assetPath, out EngineObject? asset)
   {
      throw new NotImplementedException();
   }
   
   private string GetRelativePath(DirectoryInfo baseDirectory, FileInfo filePath)
   {
      Uri baseUri = new Uri(baseDirectory.FullName);
      Uri fileUri = new Uri(filePath.FullName);

      if (baseUri.Scheme != fileUri.Scheme)
      {
         throw new InvalidOperationException("URI schemes do not match");
      }

      Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
      string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

      if (fileUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
      {
         relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      }

      return NormalizePath(relativePath);
   }
}