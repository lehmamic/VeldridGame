using VeldridGame.Abstractions;

namespace VeldridGame.Assets;

public struct AssetRef<T> : IEquatable<AssetRef<T>> where T : EngineObject
{
    private T? _instance;
    
    private Guid _assetId = Guid.Empty;
    
    // private ushort _fileId = 0;

    public T? Res
    {
        get
        {
            if (_instance == null || _instance.IsDestroyed)
            {
                RetrieveInstance();
            }

            return _instance;
        }
        set
        {
            _assetId = value == null ? Guid.Empty : value.AssetId;
            // _fileId = value == null ? (ushort)0 : value.FileId;
            _instance = value;
        }
    }
    
    public T? ResWeak => _instance == null || _instance.IsDestroyed ? null : _instance;

    public Guid AssetId
    {
        get => _assetId;
        set
        {
            _assetId = value;
            if (_instance != null && _instance.AssetId != value)
            {
                _instance = null;
            }
        }
    }

    // public ushort FileID
    // {
    //     get => _fileId;
    //     set => _fileId = value;
    // }
    
    public bool IsExplicitNull
    {
        get
        {
            return _instance == null && _assetId == Guid.Empty;
        }
    }
    
    public bool IsAvailable
    {
        get
        {
            if (_instance != null && !_instance.IsDestroyed)
            {
                return true;
            }

            RetrieveInstance();

            return _instance != null;
        }
    }
    
    public bool IsLoaded
    {
        get
        {
            if (_instance != null && !_instance.IsDestroyed)
            {
                return true;
            }

            return false;
            //return ServiceLocator.Current.GetInstance<IAssetProvider>().HasAsset(_assetId);
        }
    }
    
    public bool IsRuntimeResource => _instance != null && _assetId == Guid.Empty;

    // public string Name
    // {
    //     get
    //     {
    //         if (_instance != null)
    //         {
    //             return _instance.IsDestroyed ? "DESTROYED_" + _instance.Name : _instance.Name;
    //         }
    //
    //         return "No Instance";
    //     }
    // }

    public Type InstanceType => typeof(T);
    
    public AssetRef(Guid id)
    {
        _instance = null;
        _assetId = id;
    }
    
    // public AssetRef(Guid id, ushort fileId)
    // {
    //     _instance = null;
    //     _assetId = id;
    //     _fileId = fileId;
    // }
    
    
    public AssetRef(T? res)
    {
        _instance = res;
        _assetId = res != null ? res.AssetId : Guid.Empty;
        // _fileId = res != null ? res.FileId : (ushort)0;
    }
    
    public object? GetInstance()
    {
        return Res;
    }
    
    public void SetInstance(object? obj)
    {
        if (obj is T res)
        {
            Res = res;
        }
        else
        {
            Res = null;
        }
    }

    public void Detach()
    {
        _instance = null;
    }

    public void EnsureLoaded()
    {
        if (_instance == null) // || _instance.IsDestroyed)
        {
            RetrieveInstance();
        }
    }

    private void RetrieveInstance()
    {
        throw new NotImplementedException("Loading assets not implemented");
        // if (_assetId != Guid.Empty)
        // {
        //     _instance = (T)ServiceLocator.Current.GetInstance<IAssetProvider>().LoadAsset<T>(_assetId, _fileId);
        // }
        // else if (_instance != null && _instance.AssetId != Guid.Empty)
        // {
        //     _instance = (T)ServiceLocator.Current.GetInstance<IAssetProvider>().LoadAsset<T>(_instance.AssetId, _instance.FileId);
        // }
        // else
        // {
        //     _instance = null;
        // }
    }
    
    public override string ToString()
    {
        Type resType = typeof(T);

        char stateChar;
        if (IsRuntimeResource)
        {
            stateChar = 'R';
        }
        else if (IsExplicitNull)
        {
            stateChar = 'N';
        }
        else if (IsLoaded)
        {
            stateChar = 'L';
        }
        else
        {
            stateChar = '_';
        }

        return $"[{stateChar}] {resType.Name}";
    }

    public bool Equals(AssetRef<T>? other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is AssetRef<T> @ref)
        {
            return this == @ref;
        }
        else
        {
            return base.Equals(obj);
        }
    }
    
    public override int GetHashCode()
    {
        if (_assetId != Guid.Empty)
        {
            return _assetId.GetHashCode();  // + _fileId.GetHashCode();
        }
        else if (_instance != null)
        {
            return _instance.GetHashCode();
        }
        else
        {
            return 0;
        }
    }
    
    public bool Equals(AssetRef<T> other)
    {
        return this == other;
    }

    public static implicit operator AssetRef<T>(T res)
    {
        return new AssetRef<T>(res);
    }
    
    public static explicit operator T(AssetRef<T> res)
    {
        return res.Res;
    }
    
    public static bool operator ==(AssetRef<T> first, AssetRef<T> second)
    {
        // Completely identical
        if (first._instance == second._instance && first._assetId == second._assetId)
        {
            return true;
        }
        // Same instances
        else if (first._instance != null && second._instance != null)
        {
            return first._instance == second._instance;
        }
        // Null checks
        else if (first.IsExplicitNull)
        {
            return second.IsExplicitNull;
        }
        else if (second.IsExplicitNull)
        {
            return first.IsExplicitNull;
        }
        // Path comparison
        else
        {
            Guid? firstPath = first._instance != null ? first._instance.AssetId : first._assetId;
            Guid? secondPath = second._instance != null ? second._instance.AssetId : second._assetId;
            return firstPath == secondPath; // && first._fileId == second._fileId;
        }
    }
    
    public static bool operator !=(AssetRef<T> first, AssetRef<T> second)
    {
        return !(first == second);
    }
}