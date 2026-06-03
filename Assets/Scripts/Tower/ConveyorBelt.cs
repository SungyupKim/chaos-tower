using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    public static ConveyorBelt Instance { get; private set; }

    [Header("Belt Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float baseRotationSpeed = 60f;
    [SerializeField] private float maxRotationSpeed = 180f;
    [SerializeField] private float acceleration = 120f;
    [SerializeField] private float deceleration = 200f;

    [Header("Visual")]
    [SerializeField] private int   circleSegments = 80;
    [SerializeField] private float railOffset     = 0.12f;   // half-distance between the two rail centers
    [SerializeField] private float railWidth      = 0.055f;  // visual thickness of each rail
    [SerializeField] private int   numTies        = 80;      // sleepers around the circle

    private List<Tower> towers = new List<Tower>();
    private float currentAngle;
    private float currentSpeed;

    public float Radius           => radius;
    public float AngularSpeedDeg  => baseRotationSpeed + currentSpeed;

    private void Awake() => Instance = this;
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Start()
    {
        DrawTrack();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleInput();
        RotateTowers();
    }

    private void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float input = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
            input = 1f;
        else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
            input = -1f;

        if (input != 0f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, input * maxRotationSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
    }

    private void RotateTowers()
    {
        float effectiveSpeed = baseRotationSpeed + currentSpeed;
        currentAngle += effectiveSpeed * Time.deltaTime;

        for (int i = 0; i < towers.Count; i++)
        {
            float towerAngle = currentAngle + (360f / towers.Count * i);
            float rad = towerAngle * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            towers[i].transform.position = pos;
        }
    }

    public void RegisterTower(Tower tower)
    {
        if (!towers.Contains(tower))
            towers.Add(tower);
    }

    public void UnregisterTower(Tower tower)
    {
        towers.Remove(tower);
    }

    private void DrawTrack()
    {
        // Destroy any previously generated track children (safe to re-run)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "Rail_Outer" || child.name == "Rail_Inner" || child.name.StartsWith("Tie_"))
                Destroy(child.gameObject);
        }

        CreateRail("Rail_Outer", radius + railOffset);
        CreateRail("Rail_Inner", radius - railOffset);
        CreateTies();
    }

    private void CreateRail(string objName, float r)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount  = circleSegments + 1;
        lr.loop           = false;          // last point manually set equal to first
        lr.startWidth     = railWidth;
        lr.endWidth       = railWidth;
        lr.startColor     = new Color(0.72f, 0.72f, 0.76f);
        lr.endColor       = new Color(0.72f, 0.72f, 0.76f);
        lr.material       = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder   = 3;
        lr.useWorldSpace  = false;

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / circleSegments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f));
        }
    }

    private void CreateTies()
    {
        // Minimal white texture used as the tie sprite
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[]   px  = { Color.white, Color.white, Color.white, Color.white };
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        Sprite tieSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);

        // Ties extend slightly beyond both rails
        float tieLength  = (railOffset + 0.09f) * 2f;
        Color tieColor   = new Color(0.24f, 0.17f, 0.10f);   // dark wood/concrete

        for (int j = 0; j < numTies; j++)
        {
            float deg = 360f * j / numTies;
            float rad = deg  * Mathf.Deg2Rad;

            GameObject tie = new GameObject($"Tie_{j}");
            tie.transform.SetParent(transform);
            // Center of tie sits on the track circle radius
            tie.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            // Rotate so the tie's Y-axis points radially outward
            tie.transform.localRotation = Quaternion.Euler(0f, 0f, deg - 90f);
            // x = tangential width, y = radial length
            tie.transform.localScale = new Vector3(0.08f, tieLength, 1f);

            SpriteRenderer sr = tie.AddComponent<SpriteRenderer>();
            sr.sprite       = tieSprite;
            sr.color        = tieColor;
            sr.sortingOrder = 2;   // behind rails (order 3)
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        int segments = 32;
        Vector3 prev = transform.position + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (360f / segments * i) * Mathf.Deg2Rad;
            Vector3 next = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
