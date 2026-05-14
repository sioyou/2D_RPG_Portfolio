using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance { get; private set; }

    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadAsset<T>(string address, Action<T> onLoaded) where T : class
    {
        if (_handles.TryGetValue(address, out var cached))
        {
            onLoaded?.Invoke(cached.Result as T);
            return;
        }

        var handle = Addressables.LoadAssetAsync<T>(address);
        _handles[address] = handle;
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                onLoaded?.Invoke(op.Result);
            else
                Debug.LogError($"[Addressable] Failed to load: {address}");
        };
    }

    public void InstantiateAsync(string address, Action<GameObject> onComplete, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(address, parent);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                onComplete?.Invoke(op.Result);
            else
                Debug.LogError($"[Addressable] Failed to instantiate: {address}");
        };
    }

    public void Release(string address)
    {
        if (!_handles.TryGetValue(address, out var handle)) return;
        Addressables.Release(handle);
        _handles.Remove(address);
    }

    private void OnDestroy()
    {
        foreach (var handle in _handles.Values)
            Addressables.Release(handle);
        _handles.Clear();
    }
}
