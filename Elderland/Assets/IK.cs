using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK : MonoBehaviour 
{
	Animator animator;
	public float radius;

	private float currentPercentage;
	private float targetPercentage;
	private float dampTracker;
	public float dampSpeed;

	void Start()
	{
		animator = GetComponent<Animator>();
		currentPercentage = 0;
		targetPercentage = 0;
	}

	void OnAnimatorIK(int layerIndex)
	{
		Transform leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
		Transform rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
		Transform leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
		Transform rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
		Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
		Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
		Transform leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
		Transform rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);

		Vector3 rightFootPosition = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
		float maxDistance = rightUpperLeg.position.y - rightFootPosition.y + (rightFoot.position.y - transform.position.y);

		//Foot capsule
		Collider[] footColliders = Physics.OverlapCapsule(rightFoot.position, rightToes.position, radius, LayerConstants.GroundCollision);
		if (footColliders.Length > 0)
		{
			RaycastHit rightFootRay;
			Vector3 rightDirection = (rightFoot.position - rightLowerLeg.position).normalized;
			float rightDistance = (rightFoot.position - rightLowerLeg.position).magnitude * 2;
			Physics.Raycast(rightLowerLeg.position, rightDirection, out rightFootRay, rightDistance, LayerConstants.GroundCollision);

			//targetPosition = rightFootRay.point;
			//position
			//animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootRay.point);
			//animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);	

			//rotation
			//Vector3 n = rightFootRay.normal;
			//Quaternion tilt = Quaternion.FromToRotation(Vector3.up, n);
			//Quaternion orientation = Quaternion.FromToRotation(Vector3.forward, new Vector3(-rightLowerLeg.up.x, 0, -rightLowerLeg.up.z).normalized);
			//animator.SetIKRotation(AvatarIKGoal.RightFoot, tilt * orientation);
			//targetPercentage = 1;
		}
		else
		{
			//targetPercentage = 0;
		}

		//currentPercentage = Mathf.SmoothDamp(currentPercentage, targetPercentage, ref dampTracker, dampSpeed, 100);
		//animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, currentPercentage);

		//print(currentPercentage);

		/*else
		{
			targetPosition = rightFoot.position;
		}

		currentTargetPosition = Vector3.SmoothDamp(currentTargetPosition, targetPosition, ref positionDamp, 0.75f, 100);

		animator.SetIKPosition(AvatarIKGoal.RightFoot, currentTargetPosition);
		animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
		*/

		/* 
		//Right foot
		RaycastHit rightFootRay;
		Vector3 rightDirection = (rightFoot.position - rightLowerLeg.position).normalized;
		float rightDistance = (rightFoot.position - rightLowerLeg.position).magnitude;
		if (Physics.Raycast(rightLowerLeg.position, rightDirection, out rightFootRay, rightDistance, LayerConstants.GroundCollision))
		{
			print("touching");
			//position
			animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootRay.point + (Vector3.up * (rightFoot.position.y - transform.position.y)));
			animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);	

			//rotation
			Vector3 n = rightFootRay.normal;
			Quaternion tilt = Quaternion.FromToRotation(Vector3.up, n);
			Quaternion orientation = Quaternion.FromToRotation(Vector3.forward, new Vector3(-rightLowerLeg.up.x, 0, -rightLowerLeg.up.z).normalized);
			animator.SetIKRotation(AvatarIKGoal.RightFoot, tilt * orientation);
			animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
		}	
		*/
		/*
		//Left foot
		RaycastHit leftFootRay;
		Vector3 leftDirection = (leftFoot.position - leftLowerLeg.position).normalized;
		float leftDistance = (leftFoot.position - leftLowerLeg.position).magnitude;
		if (Physics.Raycast(leftLowerLeg.position, leftDirection, out leftFootRay, leftDistance, LayerConstants.GroundCollision))
		{
			//position
			animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootRay.point + (Vector3.up * (leftFoot.position.y - transform.position.y)));
			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
		
			//rotation
			Vector3 n = leftFootRay.normal;
			Quaternion tilt = Quaternion.FromToRotation(Vector3.up, n);
			Quaternion orientation = Quaternion.FromToRotation(Vector3.forward, new Vector3(-leftLowerLeg.up.x, 0, -leftLowerLeg.up.z).normalized);
			animator.SetIKRotation(AvatarIKGoal.LeftFoot, tilt * orientation);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
		}
		*/
	}

	void OnDrawGizmos()
	{
		Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
		Transform rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);

		Gizmos.color = Color.black;
		Gizmos.DrawSphere(rightFoot.position, radius);
		Gizmos.DrawSphere(rightToes.position, radius);
	}
}
