using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StandardInteraction : MonoBehaviour 
{
	[SerializeField]
	protected AnimationClip animationClip;
	[SerializeField]
	protected GameObject ui;
	[SerializeField]
	protected bool target;
	[SerializeField]
	protected Vector3 targetPosition;
	[SerializeField]
	protected Vector3 positionWeight;
	[SerializeField]
	protected Vector3 targetRotation;
	[SerializeField]
	protected float rotationWeight;
	[SerializeField]
	protected bool reusable;
	[SerializeField]
	protected bool disableOnStart;
	[SerializeField]
	protected Type type;
	[Header("For hold until release or complete interactions")]
	[SerializeField]
	protected float holdDuration;
	[SerializeField]
	protected float minimumHoldDuration;
	[Header("For hold interactions")]
	[SerializeField]
	protected UnityEvent holdEvent;
	[SerializeField]
	protected UnityEvent holdStartEvent;
	[Header("For all interactions")]
	[SerializeField]
	protected UnityEvent endEvent;

	protected bool activated;

	public Vector3 ValidityDirection { get { return transform.forward; } }
	public Type InteractionType { get { return type; } }
	public float HoldNormalizedTime { get; set; }
	public float HoldDuration { get { return holdDuration; } }
	public float MinimumHoldDuration { get { return minimumHoldDuration; } }
	public bool Reusable { get { return reusable; } }

	private Vector3 GeneratedTargetPosition
	{
		get
		{
			Vector3 generatedTargetPosition =
				transform.position +
				targetPosition.z * transform.forward +
				targetPosition.y * transform.up +
				targetPosition.x * transform.right;
			
			return generatedTargetPosition;
		}
	}

	private Vector3 GeneratedTargetRotation
	{
		get
		{
			Vector3 normalizedTargetRotation = targetRotation.normalized;

			Vector3 generatedTargetRotation = 
				normalizedTargetRotation.z * transform.forward +
				normalizedTargetRotation.y * transform.up +
				normalizedTargetRotation.x * transform.right;
			
			return generatedTargetRotation;
		}
	}

	public enum Type { press, holdUntilRelease, holdUntilReleaseOrComplete }  

	protected virtual void Start()
	{
		if (disableOnStart)
		{
			Disable();
		}
	}

	public void Exit()
	{
		if (!activated)
		{
			activated = true;
			GameInfo.Manager.FreezeInput(this);
			GameInfo.CameraController.AllowZoom = false;
			PlayerInfo.AnimationManager.Interuptable = false;

			if (target)
			{
				Quaternion rotation = Quaternion.LookRotation(GeneratedTargetRotation, Vector3.up);
				var matchTarget = new PlayerAnimationManager.MatchTarget(GeneratedTargetPosition, rotation, AvatarTarget.Root, positionWeight, rotationWeight);
				PlayerInfo.AnimationManager.EnqueueTarget(matchTarget);
				PlayerInfo.Animator.SetBool("targetMatch", true);
			}
			else
			{
				PlayerInfo.Animator.SetBool("targetMatch", false);
			}
			
			PlayerInfo.Animator.SetTrigger("interacting");
			PlayerInfo.Animator.SetTrigger("generalInteracting");
			PlayerInfo.Animator.SetBool("instantaneous", type == Type.press);
			//PlayerInfo.AnimationManager.SetInteractionAnimation(animationClip);

			PlayerInfo.Manager.Interaction = this;

			StartCoroutine(UITimer());
			//StartCoroutine(EndTimer());
			OnExitBegin();
		}
	}

	protected IEnumerator UITimer()
	{
		yield return new WaitForSeconds(1);
		if (type == Type.press)
			ui.SetActive(false);
	}

	public void EndEvent()
	{
		endEvent.Invoke();
	}

	public void HoldEvent()
	{
		if (holdEvent != null)
			holdEvent.Invoke();
	}

	public void StartEvent()
	{
		if (holdStartEvent != null)
			holdStartEvent.Invoke();
	}

	//protected IEnumerator EndTimer()
	//{
		//yield return new WaitForSeconds(functionalityTime);
		//endEvent.Invoke();
	//}

	public virtual void Disable()
	{
		activated = true;
		ui.SetActive(false);
	}

	public virtual void Reset()
	{
		activated = false;
		ui.SetActive(true);
		HoldNormalizedTime = 0;
	}

	protected virtual void OnExitBegin() {}

	protected void OnDrawGizmosSelected()
	{
		if (target)
		{
			Vector3 generatedTargetPosition = GeneratedTargetPosition;
			Vector3 generatedTargetRotation = GeneratedTargetRotation;

			//Line
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(generatedTargetPosition, generatedTargetPosition + generatedTargetRotation);

			//Position
			Gizmos.color = Color.red;
			Gizmos.DrawCube(generatedTargetPosition, Vector3.one * 0.25f);

			//Direction
			Gizmos.color = Color.blue;
			Gizmos.DrawCube(generatedTargetPosition + generatedTargetRotation, Vector3.one * 0.125f);
		}
	}
}
