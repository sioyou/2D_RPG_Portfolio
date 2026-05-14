using System;
using UnityEngine;

// 어드레서블 키 기반 로딩 — Resources.Load 대신 AddressableManager를 사용.
// Phase 1부터 프리팹 경로를 Addressable 주소로 등록한 뒤 이 클래스를 통해 로드.
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

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

    public void Load<T>(string address, Action<T> onLoaded) where T : class
    {
        AddressableManager.Instance.LoadAsset(address, onLoaded);
    }

    public void Instantiate(string address, Action<GameObject> onComplete, Transform parent = null)
    {
        AddressableManager.Instance.InstantiateAsync(address, onComplete, parent);
    }

    public void Release(string address)
    {
        AddressableManager.Instance.Release(address);
    }
}
