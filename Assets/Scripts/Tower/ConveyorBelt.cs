using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float baseRotationSpeed = 60f;
    [SerializeField] private float maxRotationSpeed = 180f;
    [SerializeField] private float acceleration = 120f;
    [SerializeField] private float deceleration = 200f;

    [Header("Visual")]
    [SerializeField] private LineRenderer beltVisual;
    [SerializeField] private int circleSegments = 64;

    private List<Tower> towers = new List<Tower>();
    private float currentAngle;
    private float currentSpeed;

    public float Radius => radius;

    private void Start()
    {
        DrawBeltCircle();
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

    private void DrawBeltCircle()
    {
        if (beltVisual == null) return;

        beltVisual.positionCount = circleSegments + 1;
        beltVisual.loop = true;
        beltVisual.useWorldSpace = false;

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (360f / circleSegments * i) * Mathf.Deg2Rad;
            beltVisual.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
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
