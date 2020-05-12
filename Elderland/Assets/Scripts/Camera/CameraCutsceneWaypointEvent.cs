using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CameraCutsceneWaypointEvent
{
	[SerializeField]
	public UnityEvent methods;
	[SerializeField]
	[Range(0f, 1f)]
	public float normalizedTime;

	public CameraCutsceneWaypointEvent(UnityEvent methods, float normalizedTime)
	{
		this.methods = methods;
		this.normalizedTime = Mathf.Clamp01(normalizedTime);
	}
}