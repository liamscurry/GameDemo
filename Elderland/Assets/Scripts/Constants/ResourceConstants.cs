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

            public const string KnockbackPushIcon = "Player/Abilities/Icons/KnockbackPushIcon";
            public const string KnockbackPushWarpObject = "Player/Abilities/KnockbackPush/KnockbackWarpEffect";

            // Clips
            public const string SwordHoldClip = "Player/Abilities/Sword/SwordHoldClip";
        }

        public static class Art
        {
            public const string AnimatorController = "Player/PlayerAnimatorController";

            public const string Model = "Player/PlayerAnatomyRig";
            public const string Idle = "Armature|Idle";
            
            public const string Dodge = "Dodge";
            public const string Dash = "DashForward";

            public const string FireballLeftCharge = "FireballLeftCharge";
            public const string FireballLeftAct = "FireballLeftAct";
            public const string FireballRightSummon = "FireballRight";
            public const string FireballRightHold = "FireballRightHold";
            public const string FireballRightHold2 = "FireballRightHold2";
            public const string FireballRightHold3 = "FireballRightHold3";

            public const string Firewall = "Firewall";

            public const string LightSword1Charge = "LightSword1Charge";
            public const string LightSword1Act = "LightSword1Act";
            public const string LightSword1Hold = "LightSword1Hold";
            public const string LightSword1MirrorCharge = "LightSword1MirrorCharge";
            public const string LightSword1MirrorAct = "LightSword1MirrorAct";
            public const string LightSword1Hold_M = "LightSword1Hold_M";

            public const string LightSword2Charge = "LightSword2Charge";
            public const string LightSword2Act = "LightSword2Act";
            public const string LightSword2Hold = "LightSword2Hold";
            public const string LightSword2MirrorCharge = "LightSword2MirrorCharge";
            public const string LightSword2MirrorAct = "LightSword2MirrorAct";
            public const string LightSword2Hold_M = "LightSword2Hold_M";

            public const string LightSword3Charge = "LightSword3Charge";
            public const string LightSword3Act = "LightSword3Act";
            public const string LightSword3Hold = "LightSword3Hold";
            public const string LightSword3MirrorCharge = "LightSword3MirrorCharge";
            public const string LightSword3MirrorAct = "LightSword3MirrorAct";
            public const string LightSword3Hold_M = "LightSword3Hold_M";

            public const string FinisherCharge1 = "FinisherTakedownCharge1";
            public const string FinisherAct1 = "FinisherTakedownAct1";
            public const string FinisherCharge2 = "FinisherTakedownCharge2";
            public const string FinisherAct2 = "FinisherTakedownAct2";
            public const string FinisherCharge3 = "FinisherTakedownCharge3";
            public const string FinisherAct3 = "FinisherTakedownAct3";

            public const string KnockbackPushCharge = "KnockbackPushCharge";
            public const string KnockbackPushAct = "KnockbackPushAct";

            public const string KnockbackPushWarpAnimation = "Warp";

            public const string Block1 = "Block1";
            public const string Block2 = "Block2";
        }
    }
}
