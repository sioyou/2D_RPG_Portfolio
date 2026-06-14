using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    Dictionary<string, Object> _resources = new Dictionary<string, Object>();
    Dictionary<string, Object> _sceneResources = new Dictionary<string, Object>();
    List<AsyncOperationHandle> _globalHandles = new List<AsyncOperationHandle>();
    List<AsyncOperationHandle> _sceneHandles = new List<AsyncOperationHandle>();

    #region 리소스 로드
    public bool CheckResource<T>(string key) where T : Object
    {
        if (TryGetCached(key, out _))
            return true;

        if (typeof(T) == typeof(Sprite))
        {
            key = key + ".sprite";
            if (TryGetCached(key, out _))
                return true;
        }

        return false;
    }

    public T Load<T>(string key) where T : Object
    {
        if (TryGetCached(key, out Object resource))
            return resource as T;

        return null;
    }

    bool TryGetCached(string key, out Object resource)
    {
        if (_resources.TryGetValue(key, out resource))
            return true;

        if (_sceneResources.TryGetValue(key, out resource))
            return true;

        resource = null;
        return false;
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
            return Managers.Pool.Pop(prefab);

        GameObject go = Object.Instantiate(prefab, parent);

        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        if (Managers.Pool.Push(go))
            return;

        Object.Destroy(go);
    }

    #endregion

    #region 어드레서블

    public void LoadAsync<T>(string key, bool isScene, Action<T> callback = null) where T : Object
    {
        Dictionary<string, Object> cache = isScene ? _sceneResources : _resources;

        if (cache.TryGetValue(key, out Object cached))
        {
            callback?.Invoke(cached as T);
            return;
        }

        string loadKey = key;
        if (typeof(T) == typeof(Sprite))
            loadKey = $"{key}[{key}]";

        AsyncOperationHandle<T> asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        List<AsyncOperationHandle> handles = isScene ? _sceneHandles : _globalHandles;
        handles.Add(asyncOperation);

        asyncOperation.Completed += (op) =>
        {
            if (cache.ContainsKey(key) == false)
                cache.Add(key, op.Result);

            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, bool isScene, Action onCompleteCallback) where T : Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        List<AsyncOperationHandle> handles = isScene ? _sceneHandles : _globalHandles;
        handles.Add(opHandle);

        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            if (totalCount == 0)
            {
                onCompleteCallback?.Invoke();
                return;
            }

            foreach (var result in op.Result)
            {
                if (result.ResourceType == typeof(Texture2D) || result.ResourceType == typeof(Sprite))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, isScene, (obj) =>
                    {
                        loadCount++;
                        if (loadCount >= totalCount)
                            onCompleteCallback?.Invoke();
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, isScene, (obj) =>
                    {
                        loadCount++;
                        if (loadCount >= totalCount)
                            onCompleteCallback?.Invoke();
                    });
                }
            }
        };
    }

    public void ClearSceneResources()
    {
        foreach (AsyncOperationHandle handle in _sceneHandles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        _sceneHandles.Clear();
        _sceneResources.Clear();
    }

    #endregion
}
