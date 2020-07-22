using UnityEngine;

//Stores LayerMask references for the game's layers, which are treated as "constants" at runtime.

public static class LayerConstants
{
    //Static Fields
    public static readonly LayerMask Bounds;
    public static readonly LayerMask Destructable;
    public static readonly LayerMask Interactable;
    public static readonly LayerMask Enemy;
    public static readonly LayerMask EnemyHitbox;
    public static readonly LayerMask Folliage;
    public static readonly LayerMask Hitbox;
    public static readonly LayerMask EnemyProjectile;
    public static readonly LayerMask Floor;
    public static readonly LayerMask GroundCollision;
    public static readonly LayerMask GroundRange;
    public static readonly LayerMask Pickup;
    public static readonly LayerMask Player;
    public static readonly LayerMask PlayerProjectile;

    static LayerConstants()
    {
        Bounds = 1 << LayerMask.NameToLayer("Bounds");
        Destructable = 1 << LayerMask.NameToLayer("Destructable");
        Interactable = 1 << LayerMask.NameToLayer("Interactable");
        Enemy = 1 << LayerMask.NameToLayer("Enemy");
        EnemyHitbox = 1 << LayerMask.NameToLayer("EnemyHitbox");
        Folliage = 1 << LayerMask.NameToLayer("Folliage");
        Hitbox = 1 << LayerMask.NameToLayer("Hitbox");
        EnemyProjectile = 1 << LayerMask.NameToLayer("EnemyProjectile");
        Floor = 1 << LayerMask.NameToLayer("Floor");
        GroundCollision = 1 << LayerMask.NameToLayer("GroundCollision");
        GroundRange = 1 << LayerMask.NameToLayer("GroundRange");
        Pickup = 1 << LayerMask.NameToLayer("Pickup");
        Player = 1 << LayerMask.NameToLayer("Player");
        PlayerProjectile = 1 << LayerMask.NameToLayer("PlayerProjectile");
    }
}
