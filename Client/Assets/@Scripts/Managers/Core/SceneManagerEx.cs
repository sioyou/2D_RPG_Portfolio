using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene => Object.FindFirstObjectByType<BaseScene>();

    public void LoadScene(Define.EScene type, Transform parents = null)
    {
        Managers.Clear();
        Managers.Resource.ClearSceneResources();

        string preloadLabel = Define.AddressableLabels.ScenePreload(type);
        Managers.Resource.LoadAllAsync<Object>(preloadLabel, true, () =>
        {
            SceneManager.LoadScene(GetSceneName(type));
        });
    }

    string GetSceneName(Define.EScene type)
    {
        return System.Enum.GetName(typeof(Define.EScene), type);
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
