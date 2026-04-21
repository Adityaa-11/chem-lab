using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DialogUI : MonoBehaviour
{
    static DialogUI instance;

    GameObject overlay;
    TextMeshProUGUI titleText;
    TextMeshProUGUI bodyText;
    float prevTimeScale = 1f;
    CursorLockMode prevLock;
    bool prevCursorVisible;

    public static void Show(string title, string body)
    {
        EnsureInstance();
        instance.titleText.text = title;
        instance.bodyText.text = body;
        instance.overlay.SetActive(true);

        instance.prevTimeScale = Time.timeScale;
        instance.prevLock = Cursor.lockState;
        instance.prevCursorVisible = Cursor.visible;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        overlay.SetActive(false);
        Time.timeScale = prevTimeScale;
        Cursor.lockState = prevLock;
        Cursor.visible = prevCursorVisible;
    }

    static void EnsureInstance()
    {
        if (instance != null) return;

        var go = new GameObject("DialogUI");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<DialogUI>();
        instance.Build();
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Ensure an EventSystem exists so the Close button is clickable.
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }

        overlay = MakeRT("Overlay", transform);
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.78f);
        Stretch(overlay);

        var panel = MakeRT("Panel", overlay.transform);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.067f, 0.094f, 0.153f, 0.98f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(640, 380);

        var titleGO = MakeRT("Title", panel.transform);
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.13f, 0.83f, 0.93f);
        titleText.alignment = TextAlignmentOptions.TopLeft;
        titleText.enableWordWrapping = true;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -20);
        titleRT.sizeDelta = new Vector2(-48, 50);

        var bodyGO = MakeRT("Body", panel.transform);
        bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyText.fontSize = 15;
        bodyText.color = new Color(0.85f, 0.88f, 0.93f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        bodyText.richText = true;
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0, 0);
        bodyRT.anchorMax = new Vector2(1, 1);
        bodyRT.offsetMin = new Vector2(24, 80);
        bodyRT.offsetMax = new Vector2(-24, -80);

        var btnGO = MakeRT("CloseButton", panel.transform);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.13f, 0.83f, 0.93f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(Hide);
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = btnRT.anchorMax = new Vector2(0.5f, 0);
        btnRT.pivot = new Vector2(0.5f, 0);
        btnRT.sizeDelta = new Vector2(180, 42);
        btnRT.anchoredPosition = new Vector2(0, 20);

        var btnTxtGO = MakeRT("Text", btnGO.transform);
        var btnTxt = btnTxtGO.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Close";
        btnTxt.fontSize = 15;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = new Color(0.04f, 0.055f, 0.09f);
        Stretch(btnTxtGO);

        overlay.SetActive(false);
    }

    static GameObject MakeRT(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
