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

        public PlayerSaveData(float health, float stamina)
        {
            this.health = health;
            this.stamina = stamina;
        }
    }

    // Format for Json in files is [ID jsonString\n] without braces. One object per line.
    public override string Save(SaveManager saveManager, bool resetSave = false)
    {
        CheckID(saveManager, resetSave);

        PlayerSaveData saveInfo =
            new PlayerSaveData(PlayerInfo.Manager.Health, PlayerInfo.AbilityManager.Stamina);
        string saveInfoJson = JsonUtility.ToJson(saveInfo);
        return ID + " " + saveInfoJson;
    }
    
    public override void Load(string jsonString)
    {
        PlayerSaveData saveInfo =
            JsonUtility.FromJson<PlayerSaveData>(jsonString);
        PlayerInfo.Manager.MaxOutHealth();
        PlayerInfo.Manager.ChangeHealth(-1 * (PlayerInfo.Manager.MaxHealth - saveInfo.health));
        PlayerInfo.Manager.MaxOutStamina();
        PlayerInfo.Manager.ChangeStamina(-1 * (PlayerAbilityManager.MaxStamina - saveInfo.stamina));
    }
}