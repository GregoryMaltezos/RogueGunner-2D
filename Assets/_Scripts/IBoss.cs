using UnityEngine;

public interface IBoss
{
    void Die();
    void SetCurrentHealth(int health);
    void FlashRed();
}
