using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sensor class for group arrangement. Purpose is for grunt enemies to know
// when there are nearby grunt enemies while in group following animation state.

public class GruntEnemyNearbySensor : MonoBehaviour
{
    //Properties//
	public List<IEnemyGroup> NearbyGrunts { get; private set; }
	public float Radius
	{
		get
		{
			return transform.localScale.x;
		}
	}

	private GruntEnemyManager manager;

	private void Awake()
	{ 
		NearbyGrunts = new List<IEnemyGroup>();
		manager = GetComponentInParent<GruntEnemyManager>();
	}

	private void OnDestroy()
	{
		RemoveFromNearby();
        EnemyGroup.Remove((IEnemyGroup) manager);
	}

	private void OnTriggerEnter(Collider other)
	{
		TryAdd(other);
	}

	private void OnTriggerExit(Collider other)
	{
		TryRemove(other);
	}

	public void RemoveDeadNearby(IEnemyGroup grunt)
	{
		NearbyGrunts.Remove(grunt);
	}

	private void TryAdd(Collider other)
	{
		if (other.tag == TagConstants.GruntNearbySensor)
        {
			GruntEnemyManager enemyManager =
				other.GetComponentInParent<GruntEnemyManager>();
			if (!NearbyGrunts.Contains((IEnemyGroup) enemyManager))
				NearbyGrunts.Add((IEnemyGroup) enemyManager);
        }
	}

	private void TryRemove(Collider other)
	{
		if (other.tag == TagConstants.GruntNearbySensor)
        {
			GruntEnemyManager enemyManager =
				other.GetComponentInParent<GruntEnemyManager>();
			if (NearbyGrunts.Contains((IEnemyGroup) enemyManager))
				NearbyGrunts.Remove((IEnemyGroup) enemyManager);
        }
	}

	private void RemoveFromNearby()
	{
		foreach (IEnemyGroup grunt in NearbyGrunts)
		{
			GruntEnemyManager gruntManager =
				(GruntEnemyManager) grunt;
			
			if (gruntManager.NearbySensor.NearbyGrunts.Contains((IEnemyGroup) manager))
			{
				gruntManager.NearbySensor.RemoveDeadNearby((IEnemyGroup) manager);
			}
		}

        NearbyGrunts.Clear();
	}
}