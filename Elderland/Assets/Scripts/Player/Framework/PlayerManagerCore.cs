using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper class that is used to call event methods on objects in the core
// prefab that must be in the core prefab due to required references.
// Example: Player manager cannot call change health unless in core as it references
// UI in the core prefab.
public class PlayerManagerCore : MonoBehaviour
{
    public void MaxOutHealth()
    {
        PlayerInfo.Manager.ChangeHealth(PlayerInfo.Manager.MaxHealth);
        PlayerInfo.Manager.HealFresnel();
    }
}
