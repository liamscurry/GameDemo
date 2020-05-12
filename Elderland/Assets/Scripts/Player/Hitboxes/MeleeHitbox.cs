using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Non-ranged hitbox outline that works with all trigger types. 
//Enemies can only be hit once by the hitbox during a given action.

public class MeleeHitbox : MonoBehaviour 
{
	/*private List<GameObject> targetsHit;

	//public Action<enemy> HitEnemy { get; private set; }
	//public Action<enemy> HitStaticEnemy { get; private set; }

	//Hitbox functionality is defined by the ability using the hitbox.
	public void Initialize(Action<enemy> hitEnemy, Action<enemy> hitStaticEnemy)
    {
		HitEnemy = hitEnemy;
		HitStaticEnemy = hitStaticEnemy;
        targetsHit = new List<GameObject>();
    }

	//Called on action end.
    public void Clear()
    {
        targetsHit.Clear();
    }

	//Checks if the enemy touching the hitbox is behind a wall or is close enough to the hitbox's origin.
	private void HitCondition(Collider2D other)
    {
		enemy e = other.gameObject.transform.parent.GetComponent<enemy>();
        float distanceToEnemy = Vector2.Distance(PlayerInfo.CenterPosition, e.CenterPosition);

        if (distanceToEnemy < 0.5f || !Physics2D.Linecast(PlayerInfo.CenterPosition, e.CenterPosition, LayerConstants.GroundCollision))
        {
			if (other.tag == TagConstants.EnemyHitbox)
            	HitEnemy(e);

			if (other.tag == TagConstants.StaticEnemyHitbox)
            	HitStaticEnemy(e);

            targetsHit.Add(other.gameObject);
        }
	}

	//Invokes hit methods only when an enemy hasn't been affected during a given action.
	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((other.tag == TagConstants.EnemyHitbox || other.tag == TagConstants.StaticEnemyHitbox) && !targetsHit.Contains(other.gameObject))
			HitCondition(other);
	}*/

	/*
	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((other.tag == TagConstants.EnemyHitbox || other.tag == TagConstants.StaticEnemyHitbox) && !targetsHit.Contains(other.gameObject))
			HitCondition(other);
	}
	*/
}