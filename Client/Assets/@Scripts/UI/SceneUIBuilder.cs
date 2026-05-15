using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Addressables UI 프리팹이 없을 때 씬 UI를 런타임에 구성합니다.
/// </summary>
public static class SceneUIBuilder
{
    public static GameObject CreateTitleSceneUI(string name = "UI_TitleScene")
    {
        GameObject root = CreateUIRoot(name);

        CreateStatusText(root.transform);
        CreateStartButton(root.transform);

        root.AddComponent<UI_TitleScene>();
        return root;
    }

    static GameObject CreateUIRoot(string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return root;
    }

    static void CreateStatusText(Transform parent)
    {
        GameObject go = new GameObject("StatusText", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.65f);
        rect.anchorMax = new Vector2(0.5f, 0.65f);
        rect.sizeDelta = new Vector2(800f, 80f);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = 36f;
        text.alignment = TextAlignmentOptions.Center;
        text.text = string.Empty;
    }

    static void CreateStartButton(Transform parent)
    {
        GameObject go = new GameObject("StartButton", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.35f);
        rect.anchorMax = new Vector2(0.5f, 0.35f);
        rect.sizeDelta = new Vector2(320f, 80f);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.45f, 0.85f, 1f);
        go.AddComponent<Button>();

        GameObject labelGo = new GameObject("Text", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.text = "Start";
    }
}
