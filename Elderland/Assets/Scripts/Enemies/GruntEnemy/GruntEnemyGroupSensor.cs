using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sensor class for group arrangement. Purpose is for grunt enemies to know
// when there are nearby grunt enemies while in group following animation state.

public class GruntEnemyGroupSensor : MonoBehaviour
{
    //Properties//
	public List<GameObject> NearbyGrunts { get; private set; }

	private void Awake()
	{
		NearbyGrunts = new List<GameObject>();
	}

	private void OnDestroy()
	{
		RemoveFromNearby();
	}

	private void OnTriggerEnter(Collider other)
	{
		TryAdd(other);
	}

	private void OnTriggerExit(Collider other)
	{
		TryRemove(other);
	}

	public void RemoveDeadNearby(GameObject grunt)
	{
		NearbyGrunts.Remove(grunt);
	}

	private void TryAdd(Collider other)
	{
		if (other.tag == TagConstants.GruntGroupSensor &&
            !NearbyGrunts.Contains(other.transform.parent.gameObject))
        {
			NearbyGrunts.Add(other.transform.parent.gameObject);
        }
	}

	private void TryRemove(Collider other)
	{
		if (other.tag == TagConstants.GruntGroupSensor &&
            NearbyGrunts.Contains(other.transform.parent.gameObject))
        {
			NearbyGrunts.Remove(other.transform.parent.gameObject);
        }
	}

	private void RemoveFromNearby()
	{
		foreach (GameObject grunt in NearbyGrunts)
		{
			GruntEnemyManager gruntManager =
				grunt.GetComponent<GruntEnemyManager>();
			
			if (gruntManager.GroupSensor.NearbyGrunts.Contains(transform.parent.gameObject))
			{
				gruntManager.GroupSensor.RemoveDeadNearby(transform.parent.gameObject);
			}
		}
	}
}