using UnityEngine;

[CreateAssetMenu(fileName = "NewTower", menuName = "ChaosTower/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public float baseDamage = 10f;
    public float fireRate = 1f;
    public float range = 5f;
    public float projectileSpeed = 8f;
    public Sprite sprite;
}
