using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Most basic camera structure. Simply sets the camera's settings on entrance.
public class CameraEffector : MonoBehaviour
{
	//Fields//
	[Header("Settings")]
	[SerializeField]
	protected float speed;
	[SerializeField]
	protected float zoom;
	[SerializeField]
	protected float linearMultiplier;
	[SerializeField]
	protected Vector3 direction;

	[SerializeField]
	protected float speedGradation;
	[SerializeField]
	protected float zoomGradation;
	[SerializeField]
	protected float linearMultiplierGradation;
	[SerializeField]
	protected float directionGradation;

	protected void EffectSettings()
	{
		GameInfo.CameraController.TargetSpeed = speed;
		GameInfo.CameraController.TargetZoom = zoom;
		GameInfo.CameraController.TargetLinearMultiplier = linearMultiplier;
		GameInfo.CameraController.TargetDirection = direction;

		GameInfo.CameraController.SpeedGradation = speedGradation;
		GameInfo.CameraController.ZoomGradation = zoomGradation;
		GameInfo.CameraController.LinearMultiplierGradation = linearMultiplierGradation;
		GameInfo.CameraController.DirectionGradation = directionGradation;
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Effector == null && GameInfo.CameraController.Area == null)
			{
				//Override
				GameInfo.CameraController.Transition = null;

				EffectSettings();
				GameInfo.CameraController.Effector = this;
			}
		}
	}

	protected void OnTriggerStay(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Effector == null && GameInfo.CameraController.Area == null)
			{
				//Override
				GameInfo.CameraController.Transition = null;
				
				EffectSettings();
				GameInfo.CameraController.Effector = this;
			}
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if (other.tag == TagConstants.Player)
		{
			if (GameInfo.CameraController.Effector == this)
				GameInfo.CameraController.Effector = null;
		}
	}
}