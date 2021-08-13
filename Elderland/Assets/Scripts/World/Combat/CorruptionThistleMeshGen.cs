using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Class responsible for editing the thistle's vertices when in contact with the player. This
simulates bending of the thistles (grass) in response to player collider.

Assumes static position after initialization.
*/
public class CorruptionThistleMeshGen : MonoBehaviour
{
    [SerializeField]
    private float stompHeight;

    private MeshFilter meshFilter;

    private Mesh originalMesh;

    private Vector3[] worldSpaceVertices;
    private Vector3[] worldSpaceNormals;
    private Vector3[] worldSpaceTangents;

    public List<CorruptionThistleMeshEffector> Effectors;
    
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
        StoreWorldSpaceVertices();
        StoreWorldSpaceNormals();
        StoreWorldSpaceTangents();
        Effectors = new List<CorruptionThistleMeshEffector>();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        {
            /*
            var obstructionPoints =
                new List<CorruptionThistleMeshCollider>();
            Vector3 worldPosition =
                transform.parent.position + Vector3.up * occlusionHeight;
            Vector3 worldPosition2 =
                transform.parent.position + Vector3.up * occlusionHeight + Vector3.right * 1.5f;    

            obstructionPoints.Add(new CorruptionThistleMeshCollider(1f, 2f, worldPosition));
            obstructionPoints.Add(new CorruptionThistleMeshCollider(1f, 2f, worldPosition2));
            */

            UpdateMeshGeneration(Effectors);
        }
    }

    /*
    Needed to regenerate the mesh given the set of input points in the current list.

    Inputs:
    List<Vector3> : obstructionPoints : Points that the mesh should bend away from.

    Outputs:
    None
    */
    private void UpdateMeshGeneration(List<CorruptionThistleMeshEffector> obstructionPoints)
    {
        Mesh newMesh = new Mesh();

        Vector2[] existingUVs = originalMesh.uv;
        
        Vector3[] existingVertices = originalMesh.vertices;
        WarpVertices(existingVertices, existingUVs, obstructionPoints);
        newMesh.SetVertices(existingVertices);

        newMesh.SetUVs(0, existingUVs);

        int submeshLength = originalMesh.subMeshCount;
        for (int i = 0; i < submeshLength; i++)
        {
            int[] existingTriangles = originalMesh.GetTriangles(i);
            newMesh.SetTriangles(existingTriangles, i);
        }

        // Need to recalculate the normals and tangents as vertices may have been altered.
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        newMesh.RecalculateBounds();

        meshFilter.mesh = newMesh;
    }

    /*
    Helper function for UpdateMeshGeneration that edits the copied vertices of the original mesh to
    the new mesh, based on the occlusion list passed in.

    Inputs:
    Vector3[] : vertices : List of existing vertices copied from the original mesh.
    List<Vector3> : obstructionPoints : List of points passed into UpdateMeshGeneration.

    Outputs:
    None
    */
    private void WarpVertices(
        Vector3[] vertices,
        Vector2[] uvs,
        List<CorruptionThistleMeshEffector> obstructionPoints)
    {
        for (int j = 0; j < vertices.Length; j++)
        {
            // Only consider top vertices
            if (uvs[j].y > 0.25f)
            {
                for (int i = 0; i < obstructionPoints.Count; i++)
                {
                    var meshCollider = obstructionPoints[i].MeshCollider;

                    Vector2 projectedObsDir = 
                        Matho.StdProj2D(worldSpaceVertices[j] - meshCollider.Position);
                    Vector2 projectedNormal = 
                        Matho.StdProj2D(worldSpaceNormals[j]);
                    bool flipNormal = false;
                    if (Matho.AngleBetween(projectedNormal, projectedObsDir) > 90f)
                    {
                        projectedNormal *= -1;
                        flipNormal = true;
                    }
                    Vector2 projectedTangent = 
                        Matho.StdProj2D(worldSpaceTangents[j]);
                    float normalObsDis = 
                        Mathf.Abs(Matho.ProjectScalar(projectedObsDir, projectedNormal));
                    float tangentObsDis = 
                        Mathf.Abs(Matho.ProjectScalar(projectedObsDir, projectedTangent));

                    if (projectedObsDir.magnitude != 0 &&
                        normalObsDis < meshCollider.Radius &&
                        tangentObsDis < meshCollider.Radius)
                    {
                        Vector3 obstructionDir = 
                            worldSpaceVertices[j] - meshCollider.Position;
                        if (obstructionDir.y > 0)
                            obstructionDir.y = 0;
                        Vector3 direction = worldSpaceNormals[j];
                        if (flipNormal)
                            direction *= -1;
                        direction.y = obstructionDir.y;
                        direction.Normalize();

                        float tangentPercentage =
                            1 - tangentObsDis / meshCollider.Radius;

                        float movePercentage = normalObsDis / meshCollider.Radius;
                        float verticalPercentage = movePercentage;
                        movePercentage *= 2f;
                        movePercentage -= 1f;
                        movePercentage = -Mathf.Pow(movePercentage, 2) + 1;
                        movePercentage *= tangentPercentage;

                        float obstructionPerc = meshCollider.Strength * meshCollider.Weight;
                        Vector3 newWorldVertex = 
                            worldSpaceVertices[j] + 
                            direction * movePercentage * uvs[j].y * obstructionPerc +
                            Vector3.down * (1 - verticalPercentage) * tangentPercentage * stompHeight;
                        vertices[j] =
                            transform.worldToLocalMatrix.MultiplyPoint3x4(newWorldVertex);
                    }
                }
            }
        }
    }

    /*
    Precomputes world space points of vertices for mesh generation to speed up runtime.

    Inputs:
    None

    Outputs:
    None
    */
    private void StoreWorldSpaceVertices()
    {
        worldSpaceVertices = originalMesh.vertices;
        for (int i = 0; i < worldSpaceVertices.Length; i++)
        {
            worldSpaceVertices[i] =
                transform.localToWorldMatrix.MultiplyPoint3x4(originalMesh.vertices[i]);
        }
    }

    /*
    Precomputes world space normals of vertices for mesh generation to speed up runtime.
    Stores normals of original mesh.

    Inputs:
    None

    Outputs:
    None
    */
    private void StoreWorldSpaceNormals()
    {
        worldSpaceNormals = new Vector3[originalMesh.normals.Length];
        for (int i = 0; i < worldSpaceNormals.Length; i++)
        {
            Vector3 start = 
                transform.localToWorldMatrix.MultiplyPoint3x4(originalMesh.vertices[i]);
            
            Vector3 end = 
                transform.localToWorldMatrix.MultiplyPoint3x4(originalMesh.vertices[i] + originalMesh.normals[i]);

            worldSpaceNormals[i] =
                (end - start).normalized;
        }
    }

    /*
    Precomputes world space tangents of vertices for mesh generation to speed up runtime.
    Stores tangents of original mesh.

    Inputs:
    None

    Outputs:
    None
    */
    private void StoreWorldSpaceTangents()
    {
        worldSpaceTangents = new Vector3[originalMesh.tangents.Length];
        for (int i = 0; i < worldSpaceTangents.Length; i++)
        {
            Vector3 start = 
                transform.localToWorldMatrix.MultiplyPoint3x4(originalMesh.vertices[i]);
            
            Vector3 threeTupleTangent =
                new Vector3(
                    originalMesh.tangents[i].x,
                    originalMesh.tangents[i].y,
                    originalMesh.tangents[i].z);
            Vector3 end = 
                transform.localToWorldMatrix.MultiplyPoint3x4(originalMesh.vertices[i] + threeTupleTangent);

            worldSpaceTangents[i] =
                (end - start).normalized;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.parent.position,
            transform.parent.position + Vector3.down * stompHeight);
        Gizmos.DrawCube(transform.parent.position + Vector3.down * stompHeight, Vector3.one * 0.25f);
    }
}
