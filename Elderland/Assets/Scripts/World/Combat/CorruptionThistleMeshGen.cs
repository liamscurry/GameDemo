using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Class responsible for editing the thistle's vertices when in contact with the player. This
simulates bending of the thistles (grass) in response to player collider.
*/
public class CorruptionThistleMeshGen : MonoBehaviour
{
    private MeshFilter meshFilter;

    private Mesh originalMesh;
    
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            UpdateMeshGeneration(null);
        }
    }

    /*
    Needed to regenerate the mesh given the set of input points in the current list.

    Inputs:
    List<Vector3> : obstructionPoints : Points that the mesh should bend away from.

    Outputs:
    None
    */
    private void UpdateMeshGeneration(List<Vector3> obstructionPoints)
    {
        Mesh newMesh = new Mesh();
        Vector3[] existingVertices = originalMesh.vertices;
        WarpVertices(existingVertices, obstructionPoints);
        newMesh.SetVertices(existingVertices);

        int submeshLength = originalMesh.subMeshCount;
        for (int i = 0; i < submeshLength; i++)
        {
            int[] existingTriangles = originalMesh.GetTriangles(i);
            newMesh.SetTriangles(existingTriangles, i);
        }

        // Need to recalculate the normals and tangents as vertices may have been altered.
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        Vector2[] existingUVs = originalMesh.uv;
        newMesh.SetUVs(0, existingUVs);

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
    private void WarpVertices(Vector3[] vertices, List<Vector3> obstructionPoints)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] + Vector3.up;
        }
    }
}
