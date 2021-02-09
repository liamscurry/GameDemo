//Stores the paths of resources to be loaded. Hierarchy is alphabetic.

public static class ResourceConstants 
{
   public static class Enemy
    {
        public static class Enemies
        {
            public const string LightEnemy = "Enemies/Light Enemy";
            public const string HeavyEnemy = "Enemies/Heavy Enemy";
            public const string RangedEnemy = "Enemies/RangedEnemy/RangedEnemy";
            public const string GruntEnemy = "Enemies/GruntEnemy/GruntEnemy";
            public const string TurretEnemy = "Enemies/TurretEnemy/TurretEnemy";
        }

        public static class Projectiles
        {
            public const string RangedEnemyProjectile = "Enemies/RangedEnemy/RangedEnemyProjectile";
        }

        public static class UI
        {
            public const string FinisherIndicator = "Enemies/UI/FinisherIndicator";
        }
    }

    public static class Misc
    {
        public const string Particle = "Misc/Particle";
    }

    public static class Pickups
    {
        public const string HealthPickup = "Pickups/HealthPickup";
    }

    public static class Player
    {
        public static class Hitboxes
        {
            public const string SphericalHitbox = "Player/Hitboxes/SphericalHitbox";
            public const string RectangularSingleHitbox = "Player/Hitboxes/RectangularSingleHitbox";
            public const string RectangularMultiHitbox = "Player/Hitboxes/RectangularMultiHitbox";
            public const string FireChargeSegment = "Player/Hitboxes/FireChargeSegment";
            public const string BurningFireChargeSegment = "Player/Hitboxes/BurningFireChargeSegment";
            public const string BurningFireChargeDebuffHitbox = "Player/Hitboxes/BurningFireChargeDebuffHitbox";

            public const string SwordParticles = "Player/Hitboxes/PlayerSwordParticles";
            public const string DashParticles = "Player/Hitboxes/PlayerDashParticles";
            public const string BlockParticles = "Player/Hitboxes/PlayerBlockParticles";
        }

        public static class Pickups
        {
            public const string Health = "Player/Pickups/Health";
        }

        public static class Projectiles
        {
            public const string Fireball = "Player/Projectiles/PlayerFireball";
            public const string HomingFireball = "Player/Projectiles/PlayerHomingFireball";
            public const string MultishotChild = "Player/Projectiles/PlayerMultishotChild";
            public const string MultishotParent = "Player/Projectiles/PlayerMultishotParent";
            public const string Nullify = "Player/Projectiles/PlayerNullify";
        }

        public static class Abilities
        {
            public const string ContinuousUI = "Player/Abilities/ContinuousUI";
            public const string CooldownUI = "Player/Abilities/CooldownUI";
            public const string CooldownStaminaUI = "Player/Abilities/CooldownStaminaUI";
            public const string CooldownHalfStaminaUI = "Player/Abilities/CooldownHalfStaminaUI";
            public const string CooldownLevelUI = "Player/Abilities/CooldownLevelUI";

            public const string DashTier1Icon = "Player/Abilities/Icons/DashTier1Icon";
            public const string DashTier2Icon = "Player/Abilities/Icons/DashTier2Icon";
            public const string DashTier3Icon = "Player/Abilities/Icons/DashTier3Icon";
            public const string FirechargeTier1Icon = "Player/Abilities/Icons/FirechargeTier1Icon";
            public const string FirechargeTier2Icon = "Player/Abilities/Icons/FirechargeTier2Icon";
            public const string FirechargeTier3Icon = "Player/Abilities/Icons/FirechargeTier3Icon";
            public const string FireballTier1Icon = "Player/Abilities/Icons/FireballTier1Icon";
            public const string FireballTier2Icon = "Player/Abilities/Icons/FireballTier2Icon";
            public const string FireballTier3Icon = "Player/Abilities/Icons/FireballTier3Icon";
            public const string HoldBar = "Player/Abilities/HoldBar";

            // Clips
            public const string SwordHoldClip = "Player/Abilities/Sword/SwordHoldClip";
        }
    }
}
