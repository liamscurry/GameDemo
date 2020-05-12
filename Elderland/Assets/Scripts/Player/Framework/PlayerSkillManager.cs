using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages skills earned throughout the game.

public class PlayerSkillManager 
{
    /* 
    //Fields
    private List<PlayerSkill> skills;
	
    //States
    public bool SkillInUse { get; set; }

    //Abilities
    public PlayerSkill Dash { get; private set; }
    public PlayerSkill Dodge { get; private set; }

    public PlayerSkillManager() 
    { 
        InitializeList();
        InitializePreferences();
    }

    //Updates each skill, called by PlayerManager.
	public void UpdateSkills() 
    {
        foreach (PlayerSkill s in skills)
		{
			s.UpdateSkill();
		}
	}

    public void FixedUpdateSkills() 
    {
        foreach (PlayerSkill s in skills)
		{
            if (s is PlayerFixedSkill)
			    ((PlayerFixedSkill) s).FixedUpdateSkill();
		}
	}

    //Initialize a skill
    public T Add<T>() where T : PlayerSkill
    {
        T t = PlayerInfo.Player.AddComponent<T>();
        t.Initialize(this);
        skills.Add(t);
        return t;
    }

	private void InitializeList()
	{
		skills = new List<PlayerSkill>();
	}

    //Initializes unlocked skills. Implementation is temporary, will load from file.
    private void InitializePreferences()
    {  
        Dash = Add<Dash>();
        Dodge = Add<Dodge>();
    }
    */
}
