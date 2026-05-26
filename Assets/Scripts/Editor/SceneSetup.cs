using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using TMPro;

public class SceneSetup : EditorWindow
{
    [MenuItem("ChaosTower/Setup Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("Chaos Tower Setup",
            "현재 씬에 모든 게임 오브젝트를 자동 생성합니다.\n기존 오브젝트가 있으면 중복될 수 있습니다.\n\n진행할까요?",
            "생성", "취소"))
            return;

        CreateBase();
        CreateConveyorBelt();
        GameObject towerPrefab = CreateTowerPrefab();
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject itemPrefab = CreateItemPrefab();
        CreateGameManager(enemyPrefab, itemPrefab);
        CreateTowerInstances(towerPrefab, 3);
        CreateUI();
        SetupCamera();

        Debug.Log("[ChaosTower] Scene setup complete!");
    }

    private static GameObject CreateBase()
    {
        GameObject baseObj = new GameObject("Base");
        baseObj.transform.position = Vector3.zero;

        SpriteRenderer sr = baseObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = Color.white;
        sr.sortingOrder = 5;
        baseObj.transform.localScale = Vector3.one * 1f;

        CircleCollider2D col = baseObj.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        return baseObj;
    }

    private static GameObject CreateConveyorBelt()
    {
        GameObject belt = new GameObject("ConveyorBelt");
        belt.transform.position = Vector3.zero;

        ConveyorBelt cb = belt.AddComponent<ConveyorBelt>();

        LineRenderer lr = belt.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.startColor = Color.cyan;
        lr.endColor = Color.cyan;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 1;

        SerializedObject so = new SerializedObject(cb);
        so.FindProperty("beltVisual").objectReferenceValue = lr;
        so.ApplyModifiedProperties();

        return belt;
    }

    private static GameObject CreateTowerPrefab()
    {
        GameObject tower = new GameObject("Tower");

        SpriteRenderer sr = tower.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = Color.white;
        sr.sortingOrder = 8;
        tower.transform.localScale = Vector3.one * 0.5f;

        CircleCollider2D col = tower.AddComponent<CircleCollider2D>();
        col.radius = 0.8f;
        col.isTrigger = true;

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(tower.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);

        // TowerData ScriptableObject
        TowerData towerData = CreateDefaultTowerData();

        Tower t = tower.AddComponent<Tower>();
        SerializedObject so = new SerializedObject(t);
        so.FindProperty("towerData").objectReferenceValue = towerData;
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/Tower.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tower, path);
        Object.DestroyImmediate(tower);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite();
        sr.color = Color.red;
        sr.sortingOrder = 7;
        enemy.transform.localScale = Vector3.one * 0.6f;

        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Health bar
        GameObject healthBar = new GameObject("HealthBar");
        healthBar.transform.SetParent(enemy.transform);
        healthBar.transform.localPosition = new Vector3(0, 1f, 0);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(healthBar.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(1.5f, 0.15f, 1f);
        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateSquareSprite();
        bgSr.color = Color.black;
        bgSr.sortingOrder = 9;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBar.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(1.5f, 0.15f, 1f);
        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = CreateSquareSprite();
        fillSr.color = Color.green;
        fillSr.sortingOrder = 10;

        Enemy e = enemy.AddComponent<Enemy>();
        SerializedObject so = new SerializedObject(e);
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.FindProperty("healthBarFill").objectReferenceValue = fillSr;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static GameObject CreateItemPrefab()
    {
        GameObject item = new GameObject("FloatingItem");

        SpriteRenderer sr = item.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDiamondSprite();
        sr.color = Color.magenta;
        sr.sortingOrder = 9;
        item.transform.localScale = Vector3.one * 0.4f;

        CircleCollider2D col = item.AddComponent<CircleCollider2D>();
        col.radius = 0.75f;
        col.isTrigger = true;

        FloatingItem fi = item.AddComponent<FloatingItem>();
        SerializedObject so = new SerializedObject(fi);
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/FloatingItem.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(item, path);
        Object.DestroyImmediate(item);
        return prefab;
    }

    private static void CreateGameManager(GameObject enemyPrefab, GameObject itemPrefab)
    {
        GameObject gm = new GameObject("GameManager");
        gm.transform.position = Vector3.zero;

        GameManager manager = gm.AddComponent<GameManager>();
        SerializedObject gmSo = new SerializedObject(manager);
        GameObject baseObj = GameObject.Find("Base");
        if (baseObj != null)
            gmSo.FindProperty("baseTransform").objectReferenceValue = baseObj.transform;
        gmSo.FindProperty("maxBaseHealth").intValue = 20;
        gmSo.ApplyModifiedProperties();

        EnemySpawner spawner = gm.AddComponent<EnemySpawner>();
        SerializedObject spSo = new SerializedObject(spawner);
        spSo.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        spSo.ApplyModifiedProperties();

        WaveManager wave = gm.AddComponent<WaveManager>();
        SerializedObject wmSo = new SerializedObject(wave);
        wmSo.FindProperty("enemySpawner").objectReferenceValue = spawner;
        wmSo.ApplyModifiedProperties();

        ItemSpawner itemSpawner = gm.AddComponent<ItemSpawner>();
        SerializedObject isSo = new SerializedObject(itemSpawner);
        isSo.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
        isSo.ApplyModifiedProperties();
    }

    private static void CreateTowerInstances(GameObject towerPrefab, int count)
    {
        ConveyorBelt belt = Object.FindObjectOfType<ConveyorBelt>();
        if (belt == null) return;

        SerializedObject beltSo = new SerializedObject(belt);
        float radius = beltSo.FindProperty("radius").floatValue;

        Element[] elements = { Element.Fire, Element.Water, Element.Lightning };

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count * i) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            GameObject tower = (GameObject)PrefabUtility.InstantiatePrefab(towerPrefab);
            tower.transform.position = pos;
            tower.name = $"Tower_{elements[i]}";

            Tower t = tower.GetComponent<Tower>();
            SerializedObject so = new SerializedObject(t);
            so.FindProperty("element").enumValueIndex = (int)elements[i];
            so.ApplyModifiedProperties();
        }
    }

    private static void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // HUD Panel
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel");

        GameObject waveText = CreateTMPText(hudPanel.transform, "WaveText", "Wave 1",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 50));

        GameObject scoreText = CreateTMPText(hudPanel.transform, "ScoreText", "Score: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -70), new Vector2(300, 50));

        // Health Slider
        GameObject healthSliderObj = CreateHealthSlider(hudPanel.transform);

        GameObject healthText = CreateTMPText(hudPanel.transform, "HealthText", "20/20",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(200, 40));

        // Start Panel
        GameObject startPanel = CreatePanel(canvasObj.transform, "StartPanel");
        GameObject startTitle = CreateTMPText(startPanel.transform, "Title", "CHAOS TOWER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(600, 100), 48);
        GameObject startBtn = CreateButton(startPanel.transform, "StartButton", "START",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(250, 60));

        // Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel");
        CreateTMPText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(500, 80), 42);
        GameObject finalScoreText = CreateTMPText(gameOverPanel.transform, "FinalScoreText", "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(400, 50));
        GameObject finalWaveText = CreateTMPText(gameOverPanel.transform, "FinalWaveText", "Wave: 1",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(400, 50));
        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "RESTART",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(250, 60));
        gameOverPanel.SetActive(false);

        // Wave Complete Panel
        GameObject waveCompletePanel = CreatePanel(canvasObj.transform, "WaveCompletePanel");
        CreateTMPText(waveCompletePanel.transform, "WaveClearText", "WAVE CLEAR!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 80), 42);
        waveCompletePanel.SetActive(false);

        // Upgrade Panel
        GameObject upgradePanel = CreatePanel(canvasObj.transform, "UpgradePanel");
        CreateTMPText(upgradePanel.transform, "UpgradeTitle", "WAVE CLEAR!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(400, 60), 36);

        GameObject addTowerBtn = CreateButton(upgradePanel.transform, "AddTowerBtn", "Add Tower",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(250, 60));
        upgradePanel.SetActive(false);

        // GameUI component
        GameUI gameUI = canvasObj.AddComponent<GameUI>();
        SerializedObject guiSo = new SerializedObject(gameUI);
        guiSo.FindProperty("waveText").objectReferenceValue = waveText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("healthSlider").objectReferenceValue = healthSliderObj.GetComponent<Slider>();
        guiSo.FindProperty("healthText").objectReferenceValue = healthText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("hudPanel").objectReferenceValue = hudPanel;
        guiSo.FindProperty("startPanel").objectReferenceValue = startPanel;
        guiSo.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        guiSo.FindProperty("waveCompletePanel").objectReferenceValue = waveCompletePanel;
        guiSo.FindProperty("finalScoreText").objectReferenceValue = finalScoreText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("finalWaveText").objectReferenceValue = finalWaveText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
        guiSo.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        guiSo.ApplyModifiedProperties();

        // UpgradeUI component
        UpgradeUI upgradeUI = canvasObj.AddComponent<UpgradeUI>();
        ConveyorBelt belt = Object.FindObjectOfType<ConveyorBelt>();
        string towerPrefabPath = "Assets/Prefabs/Tower.prefab";
        GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);

        SerializedObject uuSo = new SerializedObject(upgradeUI);
        uuSo.FindProperty("upgradePanel").objectReferenceValue = upgradePanel;
        uuSo.FindProperty("addTowerBtn").objectReferenceValue = addTowerBtn.GetComponent<Button>();
        uuSo.FindProperty("conveyorBelt").objectReferenceValue = belt;
        if (towerPrefab != null)
            uuSo.FindProperty("towerPrefab").objectReferenceValue = towerPrefab;
        uuSo.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 8;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.18f);

        // URP: set background type via UniversalAdditionalCameraData if available
        var urpCamData = cam.GetComponent("UniversalAdditionalCameraData");
        if (urpCamData != null)
        {
            SerializedObject so = new SerializedObject((Component)urpCamData);
            var bgType = so.FindProperty("m_BackgroundType");
            if (bgType != null)
            {
                bgType.intValue = 0; // 0 = Solid Color
                so.ApplyModifiedProperties();
            }
        }
    }

    // --- Helper Methods ---

    private static TowerData CreateDefaultTowerData()
    {
        string path = "Assets/ScriptableObjects/DefaultTowerData.asset";
        EnsureDirectory("Assets/ScriptableObjects");

        TowerData existing = AssetDatabase.LoadAssetAtPath<TowerData>(path);
        if (existing != null) return existing;

        TowerData data = ScriptableObject.CreateInstance<TowerData>();
        data.towerName = "Default Tower";
        data.baseDamage = 10f;
        data.fireRate = 1f;
        data.range = 5f;
        data.projectileSpeed = 8f;

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        return data;
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return panel;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize = 24)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f);
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }

    private static void SetButtonColor(GameObject btnObj, Color color)
    {
        Image img = btnObj.GetComponent<Image>();
        if (img != null)
        {
            Color dimmed = color * 0.6f;
            dimmed.a = 0.9f;
            img.color = dimmed;
        }
    }

    private static GameObject CreateHealthSlider(Transform parent)
    {
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(400, 30);

        // Background
        Image bgImg = sliderObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-10, -6);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.minValue = 0;
        slider.maxValue = 20;
        slider.value = 20;
        slider.interactable = false;

        return sliderObj;
    }

    // --- Sprite Generation (saved as PNG assets) ---

    private static Sprite CreateCircleSprite()
    {
        return GetOrCreateSprite("circle", 64, (x, y, size) =>
        {
            float center = size / 2f;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            return dist <= center - 1 ? Color.white : Color.clear;
        });
    }

    private static Sprite CreateSquareSprite()
    {
        return GetOrCreateSprite("square", 32, (x, y, size) => Color.white);
    }

    private static Sprite CreateTriangleSprite()
    {
        Vector2 top = new Vector2(32, 62);
        Vector2 bl = new Vector2(2, 2);
        Vector2 br = new Vector2(62, 2);
        return GetOrCreateSprite("triangle", 64, (x, y, size) =>
            PointInTriangle(new Vector2(x, y), top, bl, br) ? Color.white : Color.clear);
    }

    private static Sprite CreateDiamondSprite()
    {
        return GetOrCreateSprite("diamond", 64, (x, y, size) =>
        {
            float center = size / 2f;
            float half = center - 2;
            float dx = Mathf.Abs(x - center);
            float dy = Mathf.Abs(y - center);
            return (dx / half + dy / half) <= 1f ? Color.white : Color.clear;
        });
    }

    private delegate Color PixelFunc(int x, int y, int size);

    private static Sprite GetOrCreateSprite(string name, int size, PixelFunc pixelFunc)
    {
        string dir = "Assets/Sprites";
        EnsureDirectory(dir);
        string path = $"{dir}/{name}.png";

        // Reuse existing
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        // Generate texture and save as PNG
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, pixelFunc(x, y, size));
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        System.IO.File.WriteAllBytes(path, png);

        AssetDatabase.ImportAsset(path);

        // Configure as sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
