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
            "This will auto-generate all game objects in the current scene.\nExisting objects may be duplicated.\n\nProceed?",
            "Create", "Cancel"))
            return;

        CreateBase();
        CreateConveyorBelt();
        GameObject towerPrefab = CreateTowerPrefab();
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject itemPrefab = CreateItemPrefab();
        CreateGameManager(enemyPrefab, itemPrefab);
        CreateTowerInstances(towerPrefab, 2);
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
        belt.AddComponent<ConveyorBelt>();  // rails & ties are drawn in Start()
        return belt;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tower Prefab — stone fortress with single detailed body sprite + turret
    // ─────────────────────────────────────────────────────────────────────────
    private static GameObject CreateTowerPrefab()
    {
        GameObject tower = new GameObject("Tower");
        tower.transform.localScale = Vector3.one * 0.7f;

        CircleCollider2D col = tower.AddComponent<CircleCollider2D>();
        col.radius = 0.9f;
        col.isTrigger = true;

        // ── Visual hierarchy ──────────────────────────────────────────────────
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(tower.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;

        // Fortress body  (tower_body 128×320, PPU=320 → base 0.4×1.0)
        // At localScale (2,2,1): display 0.8×2.0 in Visual units
        // With root ×0.7: world 0.56×1.40; center at y=0, top +1.0, bottom -1.0
        GameObject towerBody = new GameObject("TowerBody");
        towerBody.transform.SetParent(visual.transform);
        towerBody.transform.localPosition = Vector3.zero;
        towerBody.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        SpriteRenderer bodySr = towerBody.AddComponent<SpriteRenderer>();
        bodySr.sprite = CreateTowerBodySprite();
        bodySr.color = Color.white;   // tinted per element by TowerVisual
        bodySr.sortingOrder = 6;

        // ── TurretPivot in the battlement zone ───────────────────────────────
        // y=0.85 in Visual → sprite yf = (0.85+1.0)/2.0 = 0.925, inside crenels (yf>0.89)
        GameObject turretPivot = new GameObject("TurretPivot");
        turretPivot.transform.SetParent(visual.transform);
        turretPivot.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        turretPivot.transform.localScale = Vector3.one;

        // Turret dome (dome_shaded 128×128, PPU=128, scale 0.55)
        GameObject turret = new GameObject("Turret");
        turret.transform.SetParent(turretPivot.transform);
        turret.transform.localPosition = Vector3.zero;
        turret.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        SpriteRenderer turretSr = turret.AddComponent<SpriteRenderer>();
        turretSr.sprite = CreateDomeSprite();
        turretSr.color = Color.white;
        turretSr.sortingOrder = 8;

        // Gun barrel (barrel_shaded 32×80, PPU=80 → base 0.4×1.0)
        // scale (0.36, 0.55) → display 0.144×0.55; center = turret_r+barrel_half = 0.275+0.275=0.55
        GameObject barrel = new GameObject("GunBarrel");
        barrel.transform.SetParent(turretPivot.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        barrel.transform.localScale = new Vector3(0.36f, 0.55f, 1f);
        SpriteRenderer barrelSr = barrel.AddComponent<SpriteRenderer>();
        barrelSr.sprite = CreateGunBarrelSprite();
        barrelSr.color = Color.white;
        barrelSr.sortingOrder = 8;

        // Core glow (glow_soft 64×64, PPU=64, scale 0.32)
        GameObject coreGlow = new GameObject("CoreGlow");
        coreGlow.transform.SetParent(turretPivot.transform);
        coreGlow.transform.localPosition = Vector3.zero;
        coreGlow.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
        SpriteRenderer coreGlowSr = coreGlow.AddComponent<SpriteRenderer>();
        coreGlowSr.sprite = CreateGlowSprite();
        coreGlowSr.color = new Color(1f, 1f, 1f, 0.7f);
        coreGlowSr.sortingOrder = 9;

        // FirePoint at barrel tip: turret_r + barrel_height = 0.275 + 0.55 = 0.825
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(turretPivot.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.825f, 0f);

        // Wheels — sit on the two rails at the tower base
        // Visual-local x = ±0.17 → world ±0.119 ≈ railOffset 0.12 (when tower is horizontal)
        // Visual-local y = -0.90 (base of tower body)
        Sprite wheelSpr = CreateWheelSprite();
        GameObject wLeft  = CreateWheel("WheelLeft",  visual.transform, new Vector3(-0.17f, -0.90f, 0f), wheelSpr);
        GameObject wRight = CreateWheel("WheelRight", visual.transform, new Vector3( 0.17f, -0.90f, 0f), wheelSpr);

        // TowerVisual animation / tracking component
        TowerVisual tv = visual.AddComponent<TowerVisual>();
        SerializedObject tvSo = new SerializedObject(tv);
        tvSo.FindProperty("bodyRenderer").objectReferenceValue       = bodySr;
        tvSo.FindProperty("turretRenderer").objectReferenceValue    = turretSr;
        tvSo.FindProperty("coreGlowRenderer").objectReferenceValue = coreGlowSr;
        tvSo.FindProperty("gunBarrelRenderer").objectReferenceValue = barrelSr;
        tvSo.FindProperty("turretPivot").objectReferenceValue       = turretPivot.transform;
        tvSo.FindProperty("wheelLeft").objectReferenceValue         = wLeft.transform;
        tvSo.FindProperty("wheelRight").objectReferenceValue        = wRight.transform;
        tvSo.ApplyModifiedProperties();

        // ── HP Bar ────────────────────────────────────────────────────────────
        // Tower body bottom: y=-1.0 in Visual/Root local → world -0.7; bars further below
        GameObject hpBar = new GameObject("HPBar");
        hpBar.transform.SetParent(tower.transform);
        hpBar.transform.localPosition = new Vector3(0f, -1.15f, 0f);

        GameObject hpBg = new GameObject("Background");
        hpBg.transform.SetParent(hpBar.transform);
        hpBg.transform.localPosition = Vector3.zero;
        hpBg.transform.localScale = new Vector3(1.5f, 0.13f, 1f);
        SpriteRenderer hpBgSr = hpBg.AddComponent<SpriteRenderer>();
        hpBgSr.sprite = CreateSquareSprite();
        hpBgSr.color = new Color(0.10f, 0.04f, 0.04f);
        hpBgSr.sortingOrder = 10;

        GameObject hpFill = new GameObject("HPFill");
        hpFill.transform.SetParent(hpBar.transform);
        hpFill.transform.localPosition = Vector3.zero;
        hpFill.transform.localScale = new Vector3(1.5f, 0.13f, 1f);
        SpriteRenderer hpFillSr = hpFill.AddComponent<SpriteRenderer>();
        hpFillSr.sprite = CreateSquareSprite();
        hpFillSr.color = Color.green;
        hpFillSr.sortingOrder = 11;

        // ── XP Bar ────────────────────────────────────────────────────────────
        GameObject xpBar = new GameObject("XPBar");
        xpBar.transform.SetParent(tower.transform);
        xpBar.transform.localPosition = new Vector3(0f, -1.30f, 0f);

        GameObject xpBg = new GameObject("Background");
        xpBg.transform.SetParent(xpBar.transform);
        xpBg.transform.localPosition = Vector3.zero;
        xpBg.transform.localScale = new Vector3(1.5f, 0.09f, 1f);
        SpriteRenderer xpBgSr = xpBg.AddComponent<SpriteRenderer>();
        xpBgSr.sprite = CreateSquareSprite();
        xpBgSr.color = new Color(0.05f, 0.05f, 0.14f);
        xpBgSr.sortingOrder = 10;

        GameObject xpFill = new GameObject("XPFill");
        xpFill.transform.SetParent(xpBar.transform);
        xpFill.transform.localPosition = Vector3.zero;
        xpFill.transform.localScale = new Vector3(1.5f, 0.09f, 1f);
        SpriteRenderer xpFillSr = xpFill.AddComponent<SpriteRenderer>();
        xpFillSr.sprite = CreateSquareSprite();
        xpFillSr.color = new Color(0.2f, 0.6f, 1f);
        xpFillSr.sortingOrder = 11;

        // ── Tower component ───────────────────────────────────────────────────
        TowerData towerData = CreateDefaultTowerData();
        Tower t = tower.AddComponent<Tower>();
        SerializedObject so = new SerializedObject(t);
        so.FindProperty("towerData").objectReferenceValue   = towerData;
        so.FindProperty("towerVisual").objectReferenceValue = tv;
        so.FindProperty("firePoint").objectReferenceValue   = firePoint.transform;
        so.FindProperty("hpBarFill").objectReferenceValue   = hpFillSr;
        so.FindProperty("xpBarFill").objectReferenceValue   = xpFillSr;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/Tower.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tower, path);
        Object.DestroyImmediate(tower);
        return prefab;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Enemy Prefab — 3D shaded airship with lit hull, gondola, and soft glow
    // ─────────────────────────────────────────────────────────────────────────
    private static GameObject CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");

        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ── Visual child — bobs without affecting physics ─────────────────────
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(enemy.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

        // Hull  (airship_hull 192×80, PPU=192 → base 1×0.417)
        // uniform scale 1.4 → display 1.4 × 0.584
        GameObject hull = new GameObject("Hull");
        hull.transform.SetParent(visual.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        SpriteRenderer hullSr = hull.AddComponent<SpriteRenderer>();
        hullSr.sprite = CreateAirshipHullSprite();
        hullSr.color = Color.white;   // tinted per element in AirshipVisual
        hullSr.sortingOrder = 7;

        // Gondola  (gondola_shaded 128×32, PPU=128 → base 1×0.25)
        // uniform scale 0.55 → display 0.55 × 0.1375
        GameObject gondola = new GameObject("Gondola");
        gondola.transform.SetParent(visual.transform);
        gondola.transform.localPosition = new Vector3(0f, -0.38f, 0f);
        gondola.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        SpriteRenderer gondolaSr = gondola.AddComponent<SpriteRenderer>();
        gondolaSr.sprite = CreateGondolaSprite();
        gondolaSr.color = Color.white;
        gondolaSr.sortingOrder = 6;

        // Tail fins (swept rectangles, keep simple)
        GameObject finTop = new GameObject("FinTop");
        finTop.transform.SetParent(visual.transform);
        finTop.transform.localPosition = new Vector3(-0.3f, 0.30f, 0f);
        finTop.transform.localScale = new Vector3(0.38f, 0.09f, 1f);
        finTop.transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
        SpriteRenderer finTopSr = finTop.AddComponent<SpriteRenderer>();
        finTopSr.sprite = CreateSquareSprite();
        finTopSr.color = Color.white;
        finTopSr.sortingOrder = 6;

        GameObject finBottom = new GameObject("FinBottom");
        finBottom.transform.SetParent(visual.transform);
        finBottom.transform.localPosition = new Vector3(-0.3f, -0.30f, 0f);
        finBottom.transform.localScale = new Vector3(0.38f, 0.09f, 1f);
        finBottom.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
        SpriteRenderer finBottomSr = finBottom.AddComponent<SpriteRenderer>();
        finBottomSr.sprite = CreateSquareSprite();
        finBottomSr.color = Color.white;
        finBottomSr.sortingOrder = 6;

        GameObject tailFin = new GameObject("TailFin");
        tailFin.transform.SetParent(visual.transform);
        tailFin.transform.localPosition = new Vector3(-0.52f, 0f, 0f);
        tailFin.transform.localScale = new Vector3(0.13f, 0.40f, 1f);
        SpriteRenderer tailFinSr = tailFin.AddComponent<SpriteRenderer>();
        tailFinSr.sprite = CreateSquareSprite();
        tailFinSr.color = Color.white;
        tailFinSr.sortingOrder = 6;

        // Propeller anchor (spins via AirshipVisual)
        GameObject propAnchor = new GameObject("PropellerAnchor");
        propAnchor.transform.SetParent(visual.transform);
        propAnchor.transform.localPosition = new Vector3(-0.76f, 0f, 0f);
        propAnchor.transform.localScale = Vector3.one;

        GameObject propHub = new GameObject("PropHub");
        propHub.transform.SetParent(propAnchor.transform);
        propHub.transform.localPosition = Vector3.zero;
        propHub.transform.localScale = new Vector3(0.10f, 0.10f, 1f);
        SpriteRenderer propHubSr = propHub.AddComponent<SpriteRenderer>();
        propHubSr.sprite = CreateCircleSprite();
        propHubSr.color = new Color(0.25f, 0.25f, 0.30f, 0.9f);
        propHubSr.sortingOrder = 8;

        CreatePropBlade(propAnchor.transform, 0f);
        CreatePropBlade(propAnchor.transform, 90f);

        // Engine glow  (glow_soft 64×64, PPU=64 → base 1×1, scale 0.28)
        GameObject engineGlow = new GameObject("EngineGlow");
        engineGlow.transform.SetParent(visual.transform);
        engineGlow.transform.localPosition = new Vector3(0.70f, 0f, 0f);
        engineGlow.transform.localScale = new Vector3(0.30f, 0.30f, 1f);
        SpriteRenderer engineGlowSr = engineGlow.AddComponent<SpriteRenderer>();
        engineGlowSr.sprite = CreateGlowSprite();
        engineGlowSr.color = new Color(1f, 0.92f, 0.4f, 0.85f);
        engineGlowSr.sortingOrder = 9;

        // AirshipVisual animation component
        AirshipVisual av = visual.AddComponent<AirshipVisual>();
        SerializedObject avSo = new SerializedObject(av);
        avSo.FindProperty("propellerAnchor").objectReferenceValue   = propAnchor.transform;
        avSo.FindProperty("engineGlowRenderer").objectReferenceValue = engineGlowSr;
        avSo.ApplyModifiedProperties();

        // ── Health bar (stays on root so it doesn't bob) ──────────────────────
        GameObject healthBar = new GameObject("HealthBar");
        healthBar.transform.SetParent(enemy.transform);
        healthBar.transform.localPosition = new Vector3(0f, 0.55f, 0f);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(healthBar.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(0.80f, 0.08f, 1f);
        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateSquareSprite();
        bgSr.color = new Color(0.10f, 0.04f, 0.04f);
        bgSr.sortingOrder = 11;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBar.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(0.80f, 0.08f, 1f);
        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite = CreateSquareSprite();
        fillSr.color = Color.green;
        fillSr.sortingOrder = 12;

        // ── Enemy component ───────────────────────────────────────────────────
        Enemy e = enemy.AddComponent<Enemy>();
        SerializedObject so = new SerializedObject(e);
        so.FindProperty("spriteRenderer").objectReferenceValue  = hullSr;
        so.FindProperty("healthBarFill").objectReferenceValue   = fillSr;
        so.FindProperty("airshipVisual").objectReferenceValue   = av;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static GameObject CreateWheel(string name, Transform parent, Vector3 localPos, Sprite spr)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.color = new Color(0.38f, 0.38f, 0.42f);
        sr.sortingOrder = 5;   // behind tower body (6)
        return obj;
    }

    private static void CreatePropBlade(Transform parent, float rotZ)
    {
        // prop_blade 16×64, PPU=64 → base 0.25×1
        // scale (0.24, 0.58) → display 0.06 × 0.58
        GameObject blade = new GameObject($"PropBlade_{(int)rotZ}");
        blade.transform.SetParent(parent);
        blade.transform.localPosition = Vector3.zero;
        blade.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        blade.transform.localScale = new Vector3(0.24f, 0.58f, 1f);
        SpriteRenderer sr = blade.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePropBladeSprite();
        sr.color = new Color(0.8f, 0.8f, 0.85f, 0.75f);
        sr.sortingOrder = 8;
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

        ConveyorBelt belt = Object.FindObjectOfType<ConveyorBelt>();
        string towerPrefabPath = "Assets/Prefabs/Tower.prefab";
        GameObject towerPref = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);

        RewardManager rm = gm.AddComponent<RewardManager>();
        SerializedObject rmSo = new SerializedObject(rm);
        if (belt != null)      rmSo.FindProperty("conveyorBelt").objectReferenceValue  = belt;
        if (towerPref != null) rmSo.FindProperty("towerPrefab").objectReferenceValue   = towerPref;
        rmSo.ApplyModifiedProperties();
    }

    private static void CreateTowerInstances(GameObject towerPrefab, int count)
    {
        ConveyorBelt belt = Object.FindObjectOfType<ConveyorBelt>();
        if (belt == null) return;

        SerializedObject beltSo = new SerializedObject(belt);
        float radius = beltSo.FindProperty("radius").floatValue;

        Element[] elements = { Element.Fire, Element.Water };

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
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;   // balance width & height matching for mobile

        // ── HUD Panel ──────────────────────────────────────────────────────────
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel");

        GameObject timeText = CreateTMPText(hudPanel.transform, "TimeText", "00:00",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(240f, 54f));

        GameObject scoreText = CreateTMPText(hudPanel.transform, "ScoreText", "Score: 0",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -84f), new Vector2(300f, 54f));

        GameObject healthSliderObj = CreateHealthSlider(hudPanel.transform);

        GameObject healthText = CreateTMPText(hudPanel.transform, "HealthText", "20/20",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -75f), new Vector2(220f, 40f));

        GameObject levelUpText = CreateTMPText(hudPanel.transform, "LevelUpText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(800f, 52f), 26);

        // ── Start Panel ────────────────────────────────────────────────────────
        GameObject startPanel = CreatePanel(canvasObj.transform, "StartPanel");
        CreateTMPText(startPanel.transform, "Title", "CHAOS TOWER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(600f, 100f), 48);
        GameObject startBtn = CreateButton(startPanel.transform, "StartButton", "START",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(250f, 60f));

        // ── Game Over Panel ────────────────────────────────────────────────────
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel");
        CreateTMPText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(500f, 80f), 42);
        GameObject finalScoreText = CreateTMPText(gameOverPanel.transform, "FinalScoreText", "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(400f, 50f));
        GameObject finalTimeText = CreateTMPText(gameOverPanel.transform, "FinalTimeText", "Survived: 00:00",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(400f, 50f));
        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "RESTART",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(250f, 60f));
        gameOverPanel.SetActive(false);

        // ── Reward Queue Panel ─────────────────────────────────────────────────
        GameObject rewardPanel = new GameObject("RewardQueuePanel");
        rewardPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rewardRt = rewardPanel.AddComponent<RectTransform>();
        rewardRt.anchorMin = new Vector2(0f, 0f);
        rewardRt.anchorMax = new Vector2(1f, 0f);
        rewardRt.pivot     = new Vector2(0.5f, 0f);
        rewardRt.anchoredPosition = Vector2.zero;
        rewardRt.sizeDelta = new Vector2(0f, 150f);
        Image rewardBg = rewardPanel.AddComponent<Image>();
        rewardBg.color = new Color(0.07f, 0.07f, 0.13f, 0.92f);

        CreateTMPText(rewardPanel.transform, "RewardTitle", "Level-Up Reward (↑↓ or tap)",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(900f, 28f), 16);

        string[] btnLabels = { "DMG", "SPEED", "RANGE", "ATTACK", "REPAIR", "+TOWER" };
        float btnW = 155f, btnH = 58f, gap = 8f;
        float rowStart = -(btnLabels.Length * (btnW + gap) - gap) / 2f + btnW / 2f;
        GameObject[] rewardBtns = new GameObject[6];
        for (int i = 0; i < 6; i++)
        {
            float x = rowStart + i * (btnW + gap);
            rewardBtns[i] = CreateButton(rewardPanel.transform, $"RewardBtn_{i}", btnLabels[i],
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -46f), new Vector2(btnW, btnH));
        }

        GameObject selectedLabel = CreateTMPText(rewardPanel.transform, "SelectedLabel", "Select: Damage +20%",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(900f, 28f), 17);

        // ── GameUI component ───────────────────────────────────────────────────
        GameUI gameUI = canvasObj.AddComponent<GameUI>();
        SerializedObject guiSo = new SerializedObject(gameUI);
        guiSo.FindProperty("timeText").objectReferenceValue       = timeText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("scoreText").objectReferenceValue      = scoreText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("healthSlider").objectReferenceValue   = healthSliderObj.GetComponent<Slider>();
        guiSo.FindProperty("healthText").objectReferenceValue     = healthText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("hudPanel").objectReferenceValue       = hudPanel;
        guiSo.FindProperty("startPanel").objectReferenceValue     = startPanel;
        guiSo.FindProperty("gameOverPanel").objectReferenceValue  = gameOverPanel;
        guiSo.FindProperty("finalScoreText").objectReferenceValue = finalScoreText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("finalTimeText").objectReferenceValue  = finalTimeText.GetComponent<TextMeshProUGUI>();
        guiSo.FindProperty("startButton").objectReferenceValue    = startBtn.GetComponent<Button>();
        guiSo.FindProperty("restartButton").objectReferenceValue  = restartBtn.GetComponent<Button>();
        guiSo.FindProperty("levelUpText").objectReferenceValue    = levelUpText.GetComponent<TextMeshProUGUI>();
        guiSo.ApplyModifiedProperties();

        // ── UpgradeUI component ────────────────────────────────────────────────
        UpgradeUI upgradeUI = canvasObj.AddComponent<UpgradeUI>();
        SerializedObject uuSo = new SerializedObject(upgradeUI);
        uuSo.FindProperty("btnDamage").objectReferenceValue    = rewardBtns[0].GetComponent<Button>();
        uuSo.FindProperty("btnFireRate").objectReferenceValue  = rewardBtns[1].GetComponent<Button>();
        uuSo.FindProperty("btnRange").objectReferenceValue     = rewardBtns[2].GetComponent<Button>();
        uuSo.FindProperty("btnAttack").objectReferenceValue    = rewardBtns[3].GetComponent<Button>();
        uuSo.FindProperty("btnRepair").objectReferenceValue    = rewardBtns[4].GetComponent<Button>();
        uuSo.FindProperty("btnAddTower").objectReferenceValue  = rewardBtns[5].GetComponent<Button>();
        uuSo.FindProperty("selectedLabel").objectReferenceValue = selectedLabel.GetComponent<TextMeshProUGUI>();
        uuSo.ApplyModifiedProperties();

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
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.14f);

        var urpCamData = cam.GetComponent("UniversalAdditionalCameraData");
        if (urpCamData != null)
        {
            SerializedObject so = new SerializedObject((Component)urpCamData);
            var bgType = so.FindProperty("m_BackgroundType");
            if (bgType != null) { bgType.intValue = 0; so.ApplyModifiedProperties(); }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI helpers
    // ─────────────────────────────────────────────────────────────────────────

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
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; tmp.raycastTarget = false;
        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;

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
        textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one; textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return btnObj;
    }

    private static GameObject CreateHealthSlider(Transform parent)
    {
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30f, -30f);
        rt.sizeDelta = new Vector2(400f, 34f);

        Image bgImg = sliderObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-10, -6);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one; fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.minValue = 0; slider.maxValue = 20; slider.value = 20;
        slider.interactable = false;
        return sliderObj;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Sprite generators — produce grayscale lit textures; SpriteRenderer.color
    // supplies element tint at runtime. PPU = max(width, height) for all new
    // sprites so one world-unit = the sprite's longer dimension at scale (1,1,1).
    // ─────────────────────────────────────────────────────────────────────────

    // Sphere-shaded dome — turret head (128×128, PPU=128, base 1×1)
    private static Sprite CreateDomeSprite()
    {
        return GetOrCreateSprite("dome_shaded", 128, 128, (x, y, w, h) =>
        {
            float cx = w * 0.5f, cy = h * 0.5f, r = w * 0.5f - 1f;
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist > r) return Color.clear;

            float nx = dx / r, ny = dy / r;
            float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx * nx - ny * ny));

            // Light from top-left-front (normalized)
            const float lx = -0.277f, ly = 0.555f, lz = 0.784f;
            float diff = Mathf.Max(0f, nx * lx + ny * ly + nz * lz);

            // Blinn-Phong specular (H = halfway between L and view=(0,0,1))
            float hx = lx, hy = ly, hz = lz + 1f;
            float hl = Mathf.Sqrt(hx * hx + hy * hy + hz * hz);
            hx /= hl; hy /= hl; hz /= hl;
            float spec = Mathf.Pow(Mathf.Max(0f, nx * hx + ny * hy + nz * hz), 32f);

            float b = Mathf.Clamp01(0.18f + diff * 0.68f + spec * 0.70f);
            return new Color(b, b, b, 1f);
        });
    }

    // Soft circular glow — core glow, engine exhaust (64×64, PPU=64, base 1×1)
    private static Sprite CreateGlowSprite()
    {
        return GetOrCreateSprite("glow_soft", 64, 64, (x, y, w, h) =>
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            float t = 1f - Mathf.Clamp01(dist / cx);
            return new Color(1f, 1f, 1f, t * t);
        });
    }

    // Railroad wheel with 4 spokes and iron rim (64×64, PPU=64, base 1×1)
    private static Sprite CreateWheelSprite()
    {
        return GetOrCreateSprite("wheel", 64, (x, y, size) =>
        {
            float cx = size * 0.5f, cy = size * 0.5f;
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float r    = size * 0.5f - 1f;   // outer radius ≈ 31px

            if (dist > r) return Color.clear;

            // Outer iron flange / rim
            if (dist > r * 0.76f) return new Color(0.18f, 0.18f, 0.20f, 1f);

            // Central hub
            if (dist < r * 0.18f) return new Color(0.62f, 0.62f, 0.66f, 1f);

            // 4-spoke pattern — every 90°, ±12° wide
            float angle    = Mathf.Atan2(dy, dx);
            float normAng  = (angle + 2f * Mathf.PI) % (2f * Mathf.PI);   // 0..2π
            float spokeAng = normAng % (Mathf.PI * 0.5f);                  // 0..π/2
            bool  isSpoke  = spokeAng < 0.21f || spokeAng > Mathf.PI * 0.5f - 0.21f;

            float b = isSpoke ? 0.62f : 0.24f;
            return new Color(b, b, b, 1f);
        });
    }

    // Vertical cylinder — tower column body (128×200, PPU=200, base 0.64×1)
    private static Sprite CreateCylinderBodySprite()
    {
        return GetOrCreateSprite("cylinder_body", 128, 200, (x, y, w, h) =>
        {
            float t = (x - w * 0.5f) / (w * 0.5f); // -1..1 across width
            float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - t * t)); // cylinder front-face normal

            float diff = nz * 0.82f;
            float spec = Mathf.Pow(nz, 16f) * 0.38f;
            float b = Mathf.Clamp01(0.16f + diff + spec);

            // Horizontal armor-plate seams (every 50px)
            int seam = y % 50;
            if (seam < 3) b *= 0.50f;

            // Thin edge shadow on left/right
            if (x < 3 || x > w - 4) b = Mathf.Clamp01(b * 0.50f);

            return new Color(b, b, b, 1f);
        });
    }

    // Narrow cylinder — gun barrel (32×80, PPU=80, base 0.4×1)
    private static Sprite CreateGunBarrelSprite()
    {
        return GetOrCreateSprite("barrel_shaded", 32, 80, (x, y, w, h) =>
        {
            float t = (x - w * 0.5f) / (w * 0.5f);
            float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - t * t));
            float diff = nz * 0.80f;
            float spec = Mathf.Pow(nz, 12f) * 0.50f;

            // Bright muzzle band at top
            float yf = (float)y / h;
            float muzzle = yf > 0.88f ? 1.3f : 1f;

            float b = Mathf.Clamp01((0.18f + diff + spec) * muzzle);
            return new Color(b, b, b, 1f);
        });
    }

    // Tech platform — wide base plate (256×32, PPU=256, base 1×0.125)
    private static Sprite CreateTechPlatformSprite()
    {
        return GetOrCreateSprite("tech_platform", 256, 32, (x, y, w, h) =>
        {
            float xf = (float)x / w, yf = (float)y / h;
            float b = 0.36f;

            // Top bright highlight strip
            if (yf > 0.80f) b = 0.68f;
            // Bottom shadow
            else if (yf < 0.14f) b = 0.16f;

            // Hex grid lines
            float gx = xf * 14f, gy = yf * 3.5f;
            if (gx - Mathf.Floor(gx) < 0.04f || gy - Mathf.Floor(gy) < 0.07f) b *= 0.62f;

            // Rivet bolts evenly spaced
            for (int i = 1; i <= 5; i++)
            {
                float bx = (float)i / 6f, by = 0.50f;
                float bd = Mathf.Sqrt((xf - bx) * (xf - bx) * 6f + (yf - by) * (yf - by));
                if (bd < 0.032f) b = 0.88f;
            }

            return new Color(Mathf.Clamp01(b), Mathf.Clamp01(b), Mathf.Clamp01(b), 1f);
        });
    }

    // Stone fortress tower — single 128×320 sprite with battlements, brick, windows
    // PPU=320 → base 0.4×1.0; at scale (2,2,1) world 0.8×2.0 inside root ×0.7
    private static Sprite CreateTowerBodySprite()
    {
        return GetOrCreateSprite("tower_body", 128, 320, (x, y, w, h) =>
        {
            float xf = (float)x / w;   // 0..1 left→right
            float yf = (float)y / h;   // 0..1 bottom→top

            // Silhouette half-width: base flare → taper → corbel overhang → battlements
            float hw;
            if      (yf < 0.08f) hw = Mathf.Lerp(0.490f, 0.430f, yf / 0.08f);
            else if (yf < 0.82f) hw = Mathf.Lerp(0.430f, 0.370f, (yf - 0.08f) / 0.74f);
            else if (yf < 0.89f) hw = Mathf.Lerp(0.370f, 0.450f, (yf - 0.82f) / 0.07f);
            else                 hw = 0.450f;

            float left = 0.5f - hw, right = 0.5f + hw;
            if (xf < left || xf > right) return Color.clear;

            float relX = (xf - 0.5f) / hw;   // -1..1 within silhouette
            float absX = Mathf.Abs(relX);

            // Crenellated battlements (5 merlons, 4 crenels)
            if (yf > 0.89f)
            {
                float bx    = (xf - left) / (hw * 2f);
                int   seg   = Mathf.Clamp(Mathf.FloorToInt(bx * 9f), 0, 8);
                float bFrac = (yf - 0.89f) / 0.11f;
                if ((seg % 2 == 1) && bFrac > 0.38f) return Color.clear;
            }

            // Box lighting: upper-left-front source — left edge dark, center-left bright
            float sideLight;
            if      (relX < -0.78f) sideLight = Mathf.Lerp(0.18f, 0.52f, (relX + 1f)    / 0.22f);
            else if (relX <  0.05f) sideLight = Mathf.Lerp(0.52f, 0.78f, (relX + 0.78f) / 0.83f);
            else if (relX <  0.72f) sideLight = Mathf.Lerp(0.78f, 0.54f, (relX - 0.05f) / 0.67f);
            else                    sideLight = Mathf.Lerp(0.54f, 0.28f, (relX - 0.72f)  / 0.28f);

            float b = sideLight * (0.88f + yf * 0.12f);

            // Brick texture: 18 rows with horizontal mortar joints + staggered verticals
            float brickRow = yf * 18f;
            int   row      = Mathf.FloorToInt(brickRow);
            float rowFrac  = brickRow - row;

            if (rowFrac < 0.07f)
            {
                b *= 0.55f;   // horizontal mortar
            }
            else
            {
                float brickX  = xf * 7f + (row % 2 == 0 ? 0f : 0.5f);
                float colFrac = brickX - Mathf.Floor(brickX);
                if (colFrac < 0.06f || colFrac > 0.94f) b *= 0.62f;   // vertical mortar
            }

            // Quoin corner stones every 3rd row (slightly brighter)
            if (absX > 0.82f && row % 3 == 0 && rowFrac >= 0.08f)
                b = Mathf.Min(b * 1.18f, 0.92f);

            // Arrow slit windows — 4 embrasures in 2×2 grid
            float slitW = 0.055f, slitH = 0.055f;
            float[] wx = { 0.35f, 0.65f, 0.35f, 0.65f };
            float[] wy = { 0.40f, 0.40f, 0.60f, 0.60f };
            for (int i = 0; i < 4; i++)
            {
                float ddx = Mathf.Abs(xf - wx[i]), ddy = Mathf.Abs(yf - wy[i]);
                if (ddx < slitW && ddy < slitH)
                {
                    if (ddx < slitW * 0.42f && ddy < slitH * 0.42f)
                        return new Color(0.04f, 0.04f, 0.09f, 1f);   // dark interior
                    b = Mathf.Max(b, 0.80f);                          // bright stone frame
                    break;
                }
            }

            return new Color(Mathf.Clamp01(b), Mathf.Clamp01(b), Mathf.Clamp01(b), 1f);
        });
    }

    // 3D blimp hull with Phong shading and porthole windows (192×80, PPU=192, base 1×0.417)
    private static Sprite CreateAirshipHullSprite()
    {
        return GetOrCreateSprite("airship_hull", 192, 80, (x, y, w, h) =>
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float rx = w * 0.5f - 1f, ry = h * 0.5f - 1f;
            float ex = (x - cx) / rx, ey = (y - cy) / ry;
            if (ex * ex + ey * ey > 1f) return Color.clear;

            // Approximate ellipsoid surface normal
            float nx = ex / (rx * rx), ny = ey / (ry * ry);
            float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - ex * ex - ey * ey));
            float nl = Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
            if (nl > 0f) { nx /= nl; ny /= nl; nz /= nl; }

            // Light from above-front
            const float lx2 = 0f, ly2 = 0.625f, lz2 = 0.781f;
            float diff = Mathf.Max(0f, nx * lx2 + ny * ly2 + nz * lz2);
            float hx = lx2, hy = ly2, hz = lz2 + 1f;
            float hl = Mathf.Sqrt(hx * hx + hy * hy + hz * hz);
            hx /= hl; hy /= hl; hz /= hl;
            float spec = Mathf.Pow(Mathf.Max(0f, nx * hx + ny * hy + nz * hz), 20f);

            float b = Mathf.Clamp01(0.17f + diff * 0.72f + spec * 0.60f);

            // Porthole windows in a row near the gondola line
            float yf = (float)y / h, xf = (float)x / w;
            if (yf > 0.30f && yf < 0.48f)
            {
                for (int wi = 1; wi <= 5; wi++)
                {
                    float wx = (float)wi / 6f;
                    float wd = Mathf.Sqrt((xf - wx) * (xf - wx) * 5f + (yf - 0.38f) * (yf - 0.38f));
                    if (wd < 0.022f)       b *= 0.20f; // dark window interior
                    else if (wd < 0.028f)  b  = 0.85f; // bright window rim
                }
            }

            return new Color(b, b, b, 1f);
        });
    }

    // Metallic gondola with window slits (128×32, PPU=128, base 1×0.25)
    private static Sprite CreateGondolaSprite()
    {
        return GetOrCreateSprite("gondola_shaded", 128, 32, (x, y, w, h) =>
        {
            float xf = (float)x / w, yf = (float)y / h;

            // Cylinder-style top-lighting (lit from above)
            float t = (yf - 0.5f) * 2f; // -1..1 top to bottom in texture (y=0 is bottom)
            float b = 0.22f + (t + 1f) * 0.5f * 0.42f;

            // Small rectangular windows
            for (int wi = 1; wi <= 4; wi++)
            {
                float wx = (float)wi / 5f;
                bool inWindow = Mathf.Abs(xf - wx) < 0.055f && yf > 0.30f && yf < 0.72f;
                if (inWindow)
                    b = Mathf.Abs(xf - wx) < 0.030f ? 0.08f : 0.62f; // interior / frame
            }

            return new Color(Mathf.Clamp01(b), Mathf.Clamp01(b), Mathf.Clamp01(b), 1f);
        });
    }

    // Propeller blade with gradient brightness (16×64, PPU=64, base 0.25×1)
    private static Sprite CreatePropBladeSprite()
    {
        return GetOrCreateSprite("prop_blade", 16, 64, (x, y, w, h) =>
        {
            float xf = (float)x / w, yf = (float)y / h;
            float t = (xf - 0.5f) * 2f; // -1..1 across blade width
            float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - t * t));
            float b = (0.18f + nz * 0.60f) * (0.55f + yf * 0.70f);
            return new Color(Mathf.Clamp01(b), Mathf.Clamp01(b), Mathf.Clamp01(b), 0.82f);
        });
    }

    // ── Legacy helpers kept for other uses (bars, base, item) ────────────────

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
        Vector2 top = new Vector2(32, 62), bl = new Vector2(2, 2), br = new Vector2(62, 2);
        return GetOrCreateSprite("triangle", 64, (x, y, size) =>
            PointInTriangle(new Vector2(x, y), top, bl, br) ? Color.white : Color.clear);
    }

    private static Sprite CreateDiamondSprite()
    {
        return GetOrCreateSprite("diamond", 64, (x, y, size) =>
        {
            float center = size / 2f, half = center - 2;
            float dx = Mathf.Abs(x - center), dy = Mathf.Abs(y - center);
            return (dx / half + dy / half) <= 1f ? Color.white : Color.clear;
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetOrCreateSprite helpers
    // ─────────────────────────────────────────────────────────────────────────

    private delegate Color PixelFunc(int x, int y, int size);
    private delegate Color PixelFuncRect(int x, int y, int w, int h);

    // Square sprites (existing convention: PPU=size, base 1×1)
    private static Sprite GetOrCreateSprite(string name, int size, PixelFunc pixelFunc)
    {
        string dir = "Assets/Sprites";
        EnsureDirectory(dir);
        string path = $"{dir}/{name}.png";

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, pixelFunc(x, y, size));
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);

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

    // Rectangular sprites — PPU = max(width, height) so the long side = 1 unit at scale (1,1,1)
    private static Sprite GetOrCreateSprite(string name, int width, int height, PixelFuncRect pixelFunc)
    {
        string dir = "Assets/Sprites";
        EnsureDirectory(dir);
        string path = $"{dir}/{name}.png";

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, pixelFunc(x, y, width, height));
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = Mathf.Max(width, height);
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

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
