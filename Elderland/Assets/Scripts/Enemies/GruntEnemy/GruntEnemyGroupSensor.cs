using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sensor class for group arrangement. Purpose is for grunt enemies to know
// when there are nearby grunt enemies while in group following animation state.

public class GruntEnemyGroupSensor : MonoBehaviour
{
    //Properties//
	private List<Collider> nearbyGrunts;
	private GruntEnemyManager manager;

	private void Awake()
	{
		nearbyGrunts = new List<Collider>();
		manager = GetComponentInParent<GruntEnemyManager>();
	}

	private void OnTriggerStay(Collider other)
	{
		TryAdd(other);
	}

	public void Reset()
	{
		nearbyGrunts.Clear();
	}

	private void TryAdd(Collider other)
	{
		if (other.tag == TagConstants.GruntGroupSensor &&
			manager.InGroupState && !nearbyGrunts.Contains(other))
        {
			GruntEnemyManager enemyManager =
				other.GetComponentInParent<GruntEnemyManager>();
			if (enemyManager.InGroupState)
			{
				nearbyGrunts.Add(other);
				EnemyGroup.Add((IEnemyGroup) enemyManager, (IEnemyGroup) manager);
			}
        }
	}
}
