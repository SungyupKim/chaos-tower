using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ChaosTower/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth = 50f;
    public float moveSpeed = 2f;
    public int damage = 1;
    public int scoreValue = 10;
    public Sprite sprite;
}
