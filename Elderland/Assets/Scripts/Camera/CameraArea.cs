using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Camera structure designed for settings the camera's settings in a defined area. 
//These settings are reset to their previous values on exit.
//This structure has two areas: a standard trigger and a buffer trigger. 
//The buffer trigger extends the exit to itself, so constant switching of settings can be reduced.
public class CameraArea : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float speed;
	[SerializeField]
	private float zoom;
	[SerializeField]
	private float linearMultiplier;
	[SerializeField]
	private Vector3 direction;

	[SerializeField]
	private float speedGradation;
	[SerializeField]
	private float zoomGradation;
	[SerializeField]
	private float linearMultiplierGradation;
	[SerializeField]
	private float directionGradation;

	//Previous settings
	private float previousSpeed;
	private float previousZoom;
	private float previousLinearMultiplier;
	private Vector3 previousDirection;

	public void Enter()
	{
		CaptureSettings();
		EffectSettings();
		//Override
		GameInfo.CameraController.Transition = null;
		GameInfo.CameraController.Effector = null;
		GameInfo.CameraController.Area = this;
	}

	public void Exit()
	{
		ResetSettings();
		GameInfo.CameraController.Area = null;
	}

	private void CaptureSettings()
	{
		previousSpeed = GameInfo.CameraController.TargetSpeed;
		previousZoom = GameInfo.CameraController.TargetZoom;
		previousLinearMultiplier = GameInfo.CameraController.TargetLinearMultiplier;
		previousDirection = GameInfo.CameraController.TargetDirection;
	}

	private void EffectSettings()
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

	private void ResetSettings()
	{
		GameInfo.CameraController.TargetSpeed = previousSpeed;
		GameInfo.CameraController.TargetZoom = previousZoom;
		GameInfo.CameraController.TargetLinearMultiplier = previousLinearMultiplier;
		GameInfo.CameraController.TargetDirection = previousDirection;
	}	
}
