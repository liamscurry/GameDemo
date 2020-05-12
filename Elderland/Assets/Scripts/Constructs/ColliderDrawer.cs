using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[SerializeField]
[System.Serializable]
public class ColliderDrawer
{
	[SerializeField]
	private List<Collider> colliders;
	[SerializeField]
	private List<BoxCollider> boxColliders;
	[SerializeField]
	private List<SphereCollider> sphereColliders;
	[SerializeField]
	private List<CapsuleCollider> capsuleColliders;

	public void Initialize(GameObject target)
	{
		Collider[] colliders = target.GetComponentsInChildren<Collider>();

		if (this.colliders == null || this.colliders.Count != colliders.Length)
		{
			//Clear old
			this.colliders = new List<Collider>(colliders);
			boxColliders = new List<BoxCollider>();
			sphereColliders = new List<SphereCollider>();
			capsuleColliders = new List<CapsuleCollider>();

			//Generate new
			foreach (Collider collider in colliders)
			{
				if (collider as BoxCollider)
					boxColliders.Add(collider as BoxCollider);
				if (collider as SphereCollider)
					sphereColliders.Add(collider as SphereCollider);
				if (collider as CapsuleCollider)
					capsuleColliders.Add(collider as CapsuleCollider);
			}
		}
	}

	public void Draw()
	{
		foreach (BoxCollider boxCollider in boxColliders)
			boxCollider.Draw(Color.blue);
		foreach (SphereCollider sphereCollider in sphereColliders)
			sphereCollider.Draw(Color.blue);
		foreach (CapsuleCollider capsuleCollider in capsuleColliders)
			capsuleCollider.Draw(Color.blue);
	}
}
#endif