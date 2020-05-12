using UnityEngine;
using UnityEditor;

//Commonly used processes for the collider components such as drawing.
public static class ColliderExtensions
{
	//Draw methods//
	#if UNITY_EDITOR
	public static void Draw(this Collider collider, Color color)
	{
		if (collider is BoxCollider)
		{
			BoxCollider boxPairTrigger = collider as BoxCollider;
			boxPairTrigger.Draw(Color.blue);
		}
		else if (collider is SphereCollider)
		{
			SphereCollider spherePairTrigger = collider as SphereCollider;
			spherePairTrigger.Draw(Color.blue);
		}
		else if (collider is CapsuleCollider)
		{
			CapsuleCollider capsulePairTrigger = collider as CapsuleCollider;
			capsulePairTrigger.Draw(Color.blue);
		}
		else
		{
			throw new System.Exception("Must be a box, sphere or capsule collider");
		}
	}

	//Primative shortcuts
	public static void Draw(this BoxCollider boxCollider, Color color)
	{
		Gizmos.color = color;
		Handles.color = color;
		SetDrawMatrix(boxCollider);
		Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
		ResetDrawMatrix();
	}

	public static void Draw(this SphereCollider sphereCollider, Color color)
	{
		Gizmos.color = color;
		Handles.color = color;
		SetDrawMatrix(sphereCollider);
		Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
		ResetDrawMatrix();
	}

	public static void Draw(this CapsuleCollider capsuleCollider, Color color)
	{
		Gizmos.color = color;
		Handles.color = color;
		SetDrawMatrix(capsuleCollider);

		float h = (capsuleCollider.height / 2 - capsuleCollider.radius);

		//Middle section
		Vector3 c0 = capsuleCollider.center;
		Vector3 c1 = c0 + capsuleCollider.radius * new Vector3(1, 0, 0);
		Vector3 c2 = c0 + capsuleCollider.radius * new Vector3(-1, 0, 0);
		Vector3 c3 = c0 + capsuleCollider.radius * new Vector3(0, 0, 1);
		Vector3 c4 = c0 + capsuleCollider.radius * new Vector3(0, 0, -1);
		Handles.DrawLine(c1 + h * Vector3.up, c1 + h * Vector3.down);
		Handles.DrawLine(c2 + h * Vector3.up, c2 + h * Vector3.down);
		Handles.DrawLine(c3 + h * Vector3.up, c3 + h * Vector3.down);
		Handles.DrawLine(c4 + h * Vector3.up, c4 + h * Vector3.down);

		//Top section
		Handles.DrawWireArc(c0 + h * Vector3.up, new Vector3(0, 0, 1), new Vector3(1, 0, 0), 180, capsuleCollider.radius);
		Handles.DrawWireArc(c0 + h * Vector3.up, new Vector3(1, 0, 0), new Vector3(0, 0, -1), 180, capsuleCollider.radius);
		Handles.DrawWireDisc(c0 + h * Vector3.up, Vector3.up, capsuleCollider.radius);

		//Bottom section
		Handles.DrawWireArc(c0 + h * Vector3.down, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), 180, capsuleCollider.radius);
		Handles.DrawWireArc(c0 + h * Vector3.down, new Vector3(1, 0, 0), new Vector3(0, 0, 1), 180, capsuleCollider.radius);
		Handles.DrawWireDisc(c0 + h * Vector3.down, Vector3.down, capsuleCollider.radius);

		ResetDrawMatrix();
	}

	//Matrix operations
	private static void SetDrawMatrix(Collider c)
	{
		Vector3 center = c.transform.position;
		Quaternion rotation = c.transform.rotation;
		Vector3 scale = c.transform.lossyScale;

		Matrix4x4 customBasis = Matrix4x4.TRS(center, rotation, scale);

		Gizmos.matrix = customBasis;
		Handles.matrix = customBasis;
	}

	private static void ResetDrawMatrix()
	{
		Matrix4x4 standardBasis = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

		Gizmos.matrix = standardBasis;
		Handles.matrix = standardBasis;
	}

	#endif

	//Position methods//
    public static Vector3 TopSpherePosition(this CapsuleCollider capsule)
    {
        return capsule.bounds.center + capsule.TopSphereOffset();
    }

    public static Vector3 BottomSpherePosition(this CapsuleCollider capsule)
    {
        return capsule.bounds.center + capsule.BottomSphereOffset();
    }

    public static Vector3 TopSphereOffset(this CapsuleCollider capsule)
    {
        Matrix4x4 customBasis = Matrix4x4.TRS(Vector3.zero, capsule.transform.rotation, Vector3.one);
        Vector3 topOffset = customBasis.MultiplyPoint((capsule.height / 2 - capsule.radius) * capsule.transform.lossyScale.y * Vector3.up);
        return topOffset;
    }

    public static Vector3 BottomSphereOffset(this CapsuleCollider capsule)
    {
        Matrix4x4 customBasis = Matrix4x4.TRS(Vector3.zero, capsule.transform.rotation, Vector3.one);
        Vector3 bottomOffset = customBasis.MultiplyPoint((capsule.height / 2 - capsule.radius) * capsule.transform.lossyScale.y * Vector3.down);
        return bottomOffset;
    }

	//Contact methods//
	/*public static bool EnclosesLocalBounds(this Collider collider, Collider other)
	{
		Vector3 center = BoundsCenter(collider);
		Vector3 size = BoundSize(collider);

		Vector3[] boundVertices = BoundVertices(other);
		Vector3[] relativeBoundVertices = LocalToLocal(boundVertices, other, collider);

		foreach (Vector3 v in relativeBoundVertices)
			if (!Contains(center, size, v))
				return false;

		return true;
	}

	private static bool Contains(Vector3 offset, Vector3 size, Vector3 vertex)
	{	
		if (!(vertex.x <= offset.x + size.x && vertex.x >= offset.x - size.x))
			return false;
		if (!(vertex.y <= offset.y + size.y && vertex.y >= offset.y - size.y))
			return false;
		if (!(vertex.z <= offset.z + size.z && vertex.z >= offset.z - size.z))
			return false;
		
		return true;
	}

	private static Vector3[] BoundVertices(Collider collider)
	{
		Vector3 offset = BoundsCenter(collider);
		Vector3 size = BoundSize(collider); 

		Vector3[] vertices = new Vector3[8];
		
		//Top
		vertices[0] = offset + new Vector3(size.x, size.y, size.z);
		vertices[1] = offset + new Vector3(size.x, size.y, -size.z);
		vertices[2] = offset + new Vector3(-size.x, size.y, size.z);
		vertices[3] = offset + new Vector3(-size.x, size.y, -size.z);

		//Bottom
		vertices[4] = offset + new Vector3(size.x, -size.y, size.z);
		vertices[5] = offset + new Vector3(size.x, -size.y, -size.z);
		vertices[6] = offset + new Vector3(-size.x, -size.y, size.z);
		vertices[7] = offset + new Vector3(-size.x, -size.y, -size.z);

		return vertices;
	}

	private static Vector3 BoundsCenter(Collider collider)
	{
		if (collider as BoxCollider)
		{
			return ((BoxCollider) collider).center;
		}
		else if (collider as SphereCollider)
		{
			return ((SphereCollider) collider).center;
		}
		else if (collider as CapsuleCollider)
		{
			return ((CapsuleCollider) collider).center;
		}
		else
		{
			throw new System.NotSupportedException("Only Box Collider, Sphere Collider and Capule Collider supported");
		}
	}

	private static Vector3 BoundSize(Collider collider)
	{
		if (collider as BoxCollider)
		{
			return ((BoxCollider) collider).size / 2;
		}
		else if (collider as SphereCollider)
		{
			SphereCollider sphereCollider = (SphereCollider) collider;

			float maxScale = sphereCollider.transform.lossyScale.x;
			if (sphereCollider.transform.lossyScale.y > maxScale)
				maxScale = sphereCollider.transform.lossyScale.y;
			if (sphereCollider.transform.lossyScale.z > maxScale)
				maxScale = sphereCollider.transform.lossyScale.z;

			return maxScale * Vector3.one;
		}
		else if (collider as CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = (CapsuleCollider) collider;

			float r = capsuleCollider.radius;
			float h = capsuleCollider.height / 2;

			return new Vector3(r, h, r);
		}
		else
		{
			throw new System.NotSupportedException("Only Box Collider, Sphere Collider and Capule Collider supported");
		}
	}

	private static Vector3[] LocalToLocal(Vector3[] vertices, Collider start, Collider end)
	{
		Matrix4x4 basis = end.transform.worldToLocalMatrix * start.transform.localToWorldMatrix;

		Vector3[] convertedVertices = new Vector3[vertices.Length];
		for (int i = 0; i < convertedVertices.Length; i++)
			convertedVertices[i] = basis.MultiplyPoint(vertices[i]);

		return convertedVertices;
	}

	//Overlap methods//
	//AABB overlap
	public static bool OverlapsBounds(this Collider collider0, Collider collider1)
	{
		Vector3 c0 = collider0.bounds.center;
		Vector3 c1 = collider1.bounds.center;
		Vector3 s0 = collider0.bounds.size / 2;
		Vector3 s1 = collider1.bounds.size / 2;

		//x
		if (!((c1.x >= c0.x && c0.x + s0.x >= c1.x - s1.x) || (c1.x <= c0.x && c1.x + s1.x >= c0.x - s0.x)))
			return false;
		
		if (!((c1.y >= c0.y && c0.y + s0.y >= c1.y - s1.y) || (c1.y <= c0.y && c1.y + s1.y >= c0.y - s0.y)))
			return false;

		if (!((c1.z >= c0.z && c0.z + s0.z >= c1.z - s1.z) || (c1.z <= c0.z && c1.z + s1.z >= c0.z - s0.z)))
			return false;

		return true;
	}

	//Raycast overlap
	public static bool Overlaps(this BoxCollider boxCollider, Collider collider)
	{
		Collider[] colliders = Physics.OverlapBox(boxCollider.bounds.center, boxCollider.size / 2, boxCollider.transform.rotation, LayerConstants.Bounds);
		return colliders.Contains(collider);
	}

	public static bool Overlaps(this SphereCollider sphereCollider, Collider collider)
	{
		Collider[] colliders = Physics.OverlapSphere(sphereCollider.bounds.center, sphereCollider.radius, LayerConstants.Bounds);
		return colliders.Contains(collider);
	}

	public static bool Overlaps(this CapsuleCollider capsuleCollider, Collider collider)
	{
		Vector3 top = capsuleCollider.TopSpherePosition();
		Vector3 bottom = capsuleCollider.BottomSpherePosition();

		Collider[] colliders = Physics.OverlapCapsule(top, bottom, capsuleCollider.radius, LayerConstants.Bounds);
		return colliders.Contains(collider);
	}*/
}