using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Paired version of the CameraEffector structure, which draws its pair's triggers on selection.
public sealed class CameraEffectorPair : CameraEffector
{
	//Fields//
	[SerializeField]
	private GameObject pair;

	#if UNITY_EDITOR
	[HideInInspector]
	[SerializeField]
	private ColliderDrawer colliderDrawer;

	private void OnDrawGizmosSelected()
	{
		colliderDrawer.Initialize(pair);
		colliderDrawer.Draw();
	}
	#endif
}