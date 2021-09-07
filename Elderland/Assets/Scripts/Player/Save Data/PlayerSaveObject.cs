using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Save component responsible for player:
Location,
Rotation,
Health,
Stamina
*/
public class PlayerSaveObject : BaseSaveObject
{
    private class PlayerSaveData
    {
        public float health;
        public float stamina;
        public Vector3 position;
        public Quaternion rotation;

        public PlayerSaveData(
            float health,
            float stamina,
            Vector3 position,
            Quaternion rotation)
        {
            this.health = health;
            this.stamina = stamina;
            this.position = position;
            this.rotation = rotation;
        }
    }

    // Format for Json in files is [ID jsonString\n] without braces. One object per line.
    public override string Save(SaveManager saveManager, bool resetSave = false)
    {
        CheckID(saveManager, resetSave);

        PlayerSaveData saveInfo =
            new PlayerSaveData(
                PlayerInfo.Manager.Health,
                PlayerInfo.AbilityManager.Stamina,
                PlayerInfo.Player.transform.position,
                PlayerInfo.Player.transform.rotation);
        string saveInfoJson = JsonUtility.ToJson(saveInfo);
        return ID + " " + saveInfoJson;
    }
    
    public override void Load(string jsonString)
    {
        PlayerSaveData saveInfo =
            JsonUtility.FromJson<PlayerSaveData>(jsonString);
        PlayerInfo.Manager.MaxOutHealth();
        PlayerInfo.Manager.ChangeHealth(-1 * (PlayerInfo.Manager.MaxHealth - saveInfo.health), false, false);
        PlayerInfo.Manager.MaxOutStamina();
        PlayerInfo.Manager.ChangeStamina(-1 * (PlayerAbilityManager.MaxStamina - saveInfo.stamina));

        PlayerInfo.Player.transform.position = saveInfo.position;
        PlayerInfo.Player.transform.rotation = saveInfo.rotation;

        PlayerInfo.MovementManager.TargetDirection =
            Matho.StdProj2D(PlayerInfo.Player.transform.forward);
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        PlayerInfo.MovementManager.SnapSpeed();

        GameInfo.CameraController.SetDirection(PlayerInfo.Player.transform);
    }
}