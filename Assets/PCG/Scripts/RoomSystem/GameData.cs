[System.Serializable]
public class GameData
{
    public float playerHealth;
    public int playerAmmo;
    public string[] unlockedWeapons;
    public DungeonData dungeonData; // Add this to save dungeon state
}
