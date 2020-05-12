using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Camera structure similar to a camera switch but is done in a linear fashion. 
//Often used with high gradations times for smooth results.
public class CameraTransition : MonoBehaviour
{
	//Fields//
	[Header("General Settings")]
	[SerializeField]
	private Vector3 orientation;
	[SerializeField]
	private Vector3 offset;
	[SerializeField]
	private float length;
	[SerializeField]
	protected float speedGradation;
	[SerializeField]
	protected float zoomGradation;
	[SerializeField]
	protected float linearMultiplierGradation;
	[SerializeField]
	protected float directionGradation;

	[Header("Blue Settings")]
	[SerializeField]
	private float blueSpeed;
	[SerializeField]
	private float blueZoom;
	[SerializeField]
	private float blueLinearMultiplier;
	[SerializeField]
	private Vector3 blueDirection;

	[Header("Red Settings")]
	[SerializeField]
	private float redSpeed;
	[SerializeField]
	private float redZoom;
	[SerializeField]
	private float redLinearMultiplier;
	[SerializeField]
	private Vector3 redDirection;
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Transition == null && GameInfo.CameraController.Effector == null && GameInfo.CameraController.Area == null)
				GameInfo.CameraController.Transition = this;

			if (GameInfo.CameraController.Transition == this)
				EffectSettings();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Transition == null && GameInfo.CameraController.Effector == null && GameInfo.CameraController.Area == null)
				GameInfo.CameraController.Transition = this;

			if (GameInfo.CameraController.Transition == this)
				EffectSettings();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Transition == this)
			{
				GameInfo.CameraController.Transition = null;

				//Red-blue comparison
				Vector3 center = transform.position + offset;
				Vector3 blue = center + orientation.normalized * (length / 2);
				Vector3 red = center - orientation.normalized * (length / 2);
				float blueDistance = Vector3.Distance(PlayerInfo.Player.transform.position, blue);
				float redDistance = Vector3.Distance(PlayerInfo.Player.transform.position, red);
				
				if (redDistance <= blueDistance)
				{
					//Exit closer to red point
					GameInfo.CameraController.TargetSpeed = redSpeed;
					GameInfo.CameraController.TargetZoom = redZoom;
					GameInfo.CameraController.TargetLinearMultiplier = redLinearMultiplier;
					GameInfo.CameraController.TargetDirection = redDirection;
				}
				else if (blueDistance < redDistance)
				{
					//Exit closer to blue point
					GameInfo.CameraController.TargetSpeed = blueSpeed;
					GameInfo.CameraController.TargetZoom = blueZoom;
					GameInfo.CameraController.TargetLinearMultiplier = blueLinearMultiplier;
					GameInfo.CameraController.TargetDirection = blueDirection;
				}

				GameInfo.CameraController.Transition = null;
			}
		}
	}

	private void EffectSettings()
	{
		//Red-blue percentage
		Vector3 center = transform.position + offset;
		Vector3 blue = (transform.position + offset) + orientation.normalized * (length / 2);
		Vector3 red = (transform.position + offset) - orientation.normalized * (length / 2);
		Vector3 axis = (red - blue).normalized;
		Vector3 playerOffset = PlayerInfo.Player.transform.position - blue;
		Vector3 projectedOffset = Matho.Project(playerOffset, axis);
		float s = Mathf.Clamp01(projectedOffset.magnitude / length);

		//Linear interpolations
		float speed = blueSpeed * s + redSpeed * (1 - s);
		float zoom = blueZoom * s + redZoom * (1 - s);
		float linearMultiplier = blueLinearMultiplier * s + redLinearMultiplier * (1 - s);
		float theta = Matho.AngleBetween(blueDirection, redDirection);
		Vector3 direction = Matho.RotateTowards(blueDirection, redDirection, theta * s);

		//Setting assignements
		GameInfo.CameraController.TargetSpeed = speed;
		GameInfo.CameraController.TargetZoom = zoom;
		GameInfo.CameraController.TargetLinearMultiplier = linearMultiplier;
		GameInfo.CameraController.TargetDirection = direction;

		GameInfo.CameraController.SpeedGradation = speedGradation;
		GameInfo.CameraController.ZoomGradation = zoomGradation;
		GameInfo.CameraController.LinearMultiplierGradation = linearMultiplierGradation;
		GameInfo.CameraController.DirectionGradation = directionGradation;
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 center = transform.position + offset;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(center, center + orientation.normalized * (length/2));
		Gizmos.DrawCube(center + orientation.normalized * (length/2), Vector3.one * 1);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(center, center - orientation.normalized * (length/2));
		Gizmos.DrawCube(center - orientation.normalized * (length/2), Vector3.one * 1);
	}
}
