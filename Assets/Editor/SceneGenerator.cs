#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SceneGenerator
{
    private const float BtnWidth = 700;
    private const float BtnHeight = 65;
    private const float BtnSpacing = 80;
    private const int BtnFontSize = 24;
    private const int TitleFontSize = 36;
    private const int StatusFontSize = 22;

    [MenuItem("Tools/Generate Demo Scenes")]
    public static void GenerateAll()
    {
        GenerateAuthScene();
        GenerateOperationsScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Auth.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Operations.unity", true),
        };

        AssetDatabase.Refresh();
        Debug.Log("[SceneGenerator] All 2 demo scenes generated.");
    }

    private static void GenerateAuthScene()
    {
        var canvas = CreateSceneBase();
        CreateTitle(canvas.transform, "MWA Demo — Auth");
        AddSingletons(canvas.transform);

        var networkLabel = CreateLabel(canvas.transform, "NetworkLabel", "Network: DevNet",
            new Vector2(0, 730), new Vector2(400, 35));
        networkLabel.alignment = TextAnchor.MiddleCenter;
        networkLabel.fontSize = 20;
        var devnetBtn = CreateButton(canvas.transform, "DevnetButton", "DevNet", 0);
        devnetBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        devnetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-170, 690);
        var mainnetBtn = CreateButton(canvas.transform, "MainnetButton", "MainNet", 0);
        mainnetBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        mainnetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(170, 690);

        var chainLabel = CreateLabel(canvas.transform, "ChainLabel", "Chain: not connected",
            new Vector2(0, 640), new Vector2(600, 30));
        chainLabel.alignment = TextAnchor.MiddleCenter;
        chainLabel.fontSize = 18;

        float y = 590;
        var loginBtn = CreateButton(canvas.transform, "LoginButton", "Login", y); y -= BtnSpacing;
        var siwsBtn = CreateButton(canvas.transform, "SiwsButton", "Login with Sign-In (SIWS)", y); y -= BtnSpacing;
        var reconnectBtn = CreateButton(canvas.transform, "ReconnectButton", "Reconnect", y); y -= BtnSpacing;
        var disconnectBtn = CreateButton(canvas.transform, "DisconnectButton", "Disconnect", y); y -= BtnSpacing;
        var deauthorizeBtn = CreateButton(canvas.transform, "DeauthorizeButton", "Deauthorize", y); y -= BtnSpacing;
        var cloneAuthBtn = CreateButton(canvas.transform, "CloneAuthButton", "Clone Authorization", y);

        float statusTop = y - BtnHeight / 2 - 20;
        float statusH = 250;
        float statusCenter = statusTop - statusH / 2;
        var statusBg = CreateImage(canvas.transform, "StatusBg", new Vector2(0, statusCenter), new Vector2(920, statusH),
            new Color(0, 0, 0, 0.5f));
        statusBg.raycastTarget = false;
        var statusText = CreateLabel(canvas.transform, "StatusText", "",
            new Vector2(0, statusCenter), new Vector2(880, statusH - 20));
        statusText.fontSize = 20;
        statusText.alignment = TextAnchor.UpperCenter;

        var navBtn = CreateButton(canvas.transform, "NavButton", "→ Operations", -850);
        navBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 70);

        var controllerGo = new GameObject("AuthScreenController");
        controllerGo.transform.SetParent(canvas.transform, false);
        var controller = controllerGo.AddComponent<AuthScreenController>();

        var so = new SerializedObject(controller);
        WireField(so, "statusText", statusText);
        WireField(so, "chainLabel", chainLabel);
        WireField(so, "loginButton", loginBtn);
        WireField(so, "siwsButton", siwsBtn);
        WireField(so, "reconnectButton", reconnectBtn);
        WireField(so, "disconnectButton", disconnectBtn);
        WireField(so, "deauthorizeButton", deauthorizeBtn);
        WireField(so, "cloneAuthButton", cloneAuthBtn);
        WireField(so, "devnetButton", devnetBtn);
        WireField(so, "mainnetButton", mainnetBtn);
        WireField(so, "networkLabel", networkLabel);
        WireField(so, "navButton", navBtn);
        so.ApplyModifiedPropertiesWithoutUndo();

        SaveScene("Assets/Scenes/Auth.unity");
    }

    private static void GenerateOperationsScene()
    {
        var canvas = CreateSceneBase();
        CreateTitle(canvas.transform, "MWA Demo — Operations");
        AddSingletons(canvas.transform);

        var networkLabel = CreateLabel(canvas.transform, "NetworkLabel", "Network: DevNet",
            new Vector2(0, 730), new Vector2(400, 35));
        networkLabel.alignment = TextAnchor.MiddleCenter;
        networkLabel.fontSize = 20;
        var devnetBtn = CreateButton(canvas.transform, "DevnetButton", "DevNet", 0);
        devnetBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        devnetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-170, 690);
        var mainnetBtn = CreateButton(canvas.transform, "MainnetButton", "MainNet", 0);
        mainnetBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        mainnetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(170, 690);

        var chainLabel = CreateLabel(canvas.transform, "ChainLabel", "Chain: not connected",
            new Vector2(0, 640), new Vector2(600, 30));
        chainLabel.alignment = TextAnchor.MiddleCenter;
        chainLabel.fontSize = 18;

        float y = 590;
        var signTxBtn = CreateButton(canvas.transform, "SignTxButton", "Sign Transaction", y); y -= BtnSpacing;
        var signAllBtn = CreateButton(canvas.transform, "SignAllButton", "Sign All Transactions", y); y -= BtnSpacing;
        var signMsgBtn = CreateButton(canvas.transform, "SignMsgButton", "Sign Message", y); y -= BtnSpacing;
        var signAndSendBtn = CreateButton(canvas.transform, "SignAndSendButton", "Sign & Send", y); y -= BtnSpacing;
        var getCapsBtn = CreateButton(canvas.transform, "GetCapsButton", "Get Capabilities", y);

        float statusTop = y - BtnHeight / 2 - 20;
        float statusH = 250;
        float statusCenter = statusTop - statusH / 2;
        var statusBg = CreateImage(canvas.transform, "StatusBg", new Vector2(0, statusCenter), new Vector2(920, statusH),
            new Color(0, 0, 0, 0.5f));
        statusBg.raycastTarget = false;
        var statusText = CreateLabel(canvas.transform, "StatusText", "",
            new Vector2(0, statusCenter), new Vector2(880, statusH - 20));
        statusText.fontSize = 20;
        statusText.alignment = TextAnchor.UpperCenter;

        var navBtn = CreateButton(canvas.transform, "NavButton", "← Auth", -850);
        navBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 70);

        var controllerGo = new GameObject("OperationsScreenController");
        controllerGo.transform.SetParent(canvas.transform, false);
        var controller = controllerGo.AddComponent<OperationsScreenController>();

        var so = new SerializedObject(controller);
        WireField(so, "statusText", statusText);
        WireField(so, "chainLabel", chainLabel);
        WireField(so, "signTxButton", signTxBtn);
        WireField(so, "signAllButton", signAllBtn);
        WireField(so, "signMsgButton", signMsgBtn);
        WireField(so, "signAndSendButton", signAndSendBtn);
        WireField(so, "getCapabilitiesButton", getCapsBtn);
        WireField(so, "devnetButton", devnetBtn);
        WireField(so, "mainnetButton", mainnetBtn);
        WireField(so, "networkLabel", networkLabel);
        WireField(so, "navButton", navBtn);
        so.ApplyModifiedPropertiesWithoutUndo();

        SaveScene("Assets/Scenes/Operations.unity");
    }

    private static void AddSingletons(Transform canvas)
    {
        var adapterGo = new GameObject("AdapterManager");
        adapterGo.transform.SetParent(canvas.transform, false);
        adapterGo.AddComponent<AdapterManager>();
    }

    private static GameObject CreateSceneBase()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.14f);
        }

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();

        return canvasGo;
    }

    private static void CreateTitle(Transform parent, string text)
    {
        var t = CreateLabel(parent, "Title", text, new Vector2(0, 780), new Vector2(900, 60));
        t.fontSize = TitleFontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
    }

    private static Button CreateButton(Transform parent, string name, string label, float yPos)
    {
        var btnGo = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnGo.name = name;
        btnGo.transform.SetParent(parent, false);
        var rt = btnGo.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(BtnWidth, BtnHeight);
        var text = btnGo.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = label;
            text.fontSize = BtnFontSize;
        }
        return btnGo.GetComponent<Button>();
    }

    private static Text CreateLabel(Transform parent, string name, string defaultText,
        Vector2 position, Vector2 size)
    {
        var textGo = DefaultControls.CreateText(new DefaultControls.Resources());
        textGo.name = name;
        textGo.transform.SetParent(parent, false);
        var rt = textGo.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var text = textGo.GetComponent<Text>();
        if (text != null)
        {
            text.text = defaultText;
            text.fontSize = StatusFontSize;
            text.alignment = TextAnchor.UpperCenter;
            text.color = Color.white;
        }
        return text;
    }

    private static Image CreateImage(Transform parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        var imgGo = DefaultControls.CreateImage(new DefaultControls.Resources());
        imgGo.name = name;
        imgGo.transform.SetParent(parent, false);
        var rt = imgGo.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var img = imgGo.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private static void WireField(SerializedObject so, string fieldName, Object value)
    {
        var prop = so.FindProperty(fieldName);
        if (prop != null)
            prop.objectReferenceValue = value;
        else
            Debug.LogWarning($"[SceneGenerator] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
    }

    private static void SaveScene(string path)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
    }
}
#endif
