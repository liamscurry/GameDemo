using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Class responsible for updating the CorruptionThistleMeshCollider structure it holds and adding 
and removing it from CorruptionThistleMeshGen objects.
*/
public class CorruptionThistleMeshEffector : MonoBehaviour
{
    [SerializeField]
    private float radius;
    // Moves the center of the position from the center of transition relative
    // to local coordinates.
    [SerializeField]
    private Vector3 offset; 

    public CorruptionThistleMeshCollider MeshCollider { get; private set;}

    private const float strengthPerRadius = 1.5f;

    private Vector3 localOffset 
    { 
        get 
        {
            return 
                transform.forward * offset.x +
                transform.up * offset.y +
                transform.right * offset.x;
        }
    }

    private void Start()
    {
        MeshCollider =
            new CorruptionThistleMeshCollider(radius, strengthPerRadius * radius, Vector3.zero);
    }

    private void Update()
    {
        MeshCollider.Position = transform.position + localOffset;
    }

    // Need to use trigger methods to add and remove self mesh collider when touching thistle mesh gen
    // objects.
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.ThistleGen)
        {
            var meshGen = other.GetComponent<CorruptionThistleMeshGen>();
            meshGen.Effectors.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TagConstants.ThistleGen)
        {
            var meshGen = other.GetComponent<CorruptionThistleMeshGen>();
            meshGen.Effectors.Remove(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + localOffset);
        Gizmos.DrawCube(transform.position + localOffset, Vector3.one * 0.25f);
    }
}