using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "VR Archery/Level Configuration")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;
    
    [Header("Gameplay Settings")]
    public int maxArrows = 10;
    public int targetCount = 5;
    public float timeLimit = 0f; // 0 = no time limit
}