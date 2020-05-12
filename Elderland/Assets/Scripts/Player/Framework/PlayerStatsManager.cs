using UnityEngine;

//Sub manager that deals with player stats and how they are modified from outside sources.
//Properties combine all altering factors of each stat, such as player level, to represent the final stat.

public class PlayerStatsManager 
{
	//Stats
	private float movespeed;
	private float jumpspeed;

	//Stat properties
	//Based on: movespeed modifier
	public float Movespeed { get { return movespeed * MovespeedModifier; } }
	public float Jumpspeed { get { return jumpspeed * JumpspeedModifier; }}

	//Modifiers for abilities/skills
	public float MovespeedModifier { get; set; }
	public float JumpspeedModifier { get; set; }

	public float BaseMovespeed { get { return movespeed; } }
	public float BaseJumpspeed { get { return jumpspeed; } }

	//Modifier editors
	public GameObject MovespeedEditor { get; set; }
	public GameObject JumpspeedEditor { get; set; }

	public PlayerStatsManager()
	{
		//Base values
		//7.25f
		movespeed = 1.59544f * 1.75f;
		jumpspeed = 7;

		//Modifiers
		MovespeedModifier = 1;
		JumpspeedModifier = 1;
	}
}