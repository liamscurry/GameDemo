using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Buffer trigger part of the CameraArea structure. 
//Extends the exit of the standard trigger, so constant switching of settings can be reduced.
public class CameraAreaBufferTrigger : MonoBehaviour
{
	//Fields//
	#if UNITY_EDITOR
	[HideInInspector]
	[SerializeField]
	private ColliderDrawer colliderDrawer;
	#endif

	[SerializeField]
	private CameraAreaTrigger pair;

	private CameraArea area;
	private int activeTriggers;

	//Properties//
	public bool Active { get; protected set; }

	private void Start()
	{
		area = transform.parent.GetComponent<CameraArea>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (activeTriggers == 0)
				Active = true;

			activeTriggers += 1;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (activeTriggers == 1)
			{
				Active = false;
				if (!pair.Active && GameInfo.CameraController.Area == area)
					area.Exit();
			}

			activeTriggers -= 1;
		}
	}

	#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		colliderDrawer.Initialize(pair.gameObject);
		colliderDrawer.Draw();
	}
	#endif
}