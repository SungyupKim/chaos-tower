using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "ChaosTower/Wave Data")]
public class WaveData : ScriptableObject
{
    public EnemyGroup[] groups;
    public float groupDelay = 2f;
}

[System.Serializable]
public class EnemyGroup
{
    public EnemyData enemyData;
    public Element element;
    public int count = 5;
    public float spawnInterval = 1f;
}
