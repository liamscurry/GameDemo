using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleDirection { Up, Right, Down, Left }
public enum PuzzleVertexType 
{ 
   Center, 
   Top, Right, Bottom, Left,
   TopLeftCorner, TopRightCorner, BottomRightCorner, BottomLeftCorner 
}

public class PuzzleVertex : MonoBehaviour
{
    [SerializeField]
    private PuzzleVertex solutionVertex;
    [SerializeField]
    private PuzzleDirection solutionDirection;
    [SerializeField]
    private PuzzleVertexType vertexType;
    
    public PuzzleVertex SolutionVertex { get { return solutionVertex; } }
    public PuzzleDirection SolutionDirection { get { return solutionDirection; } }
    public PuzzleVertexType VertexType { get { return vertexType; } }

    private static readonly float[][] alphas = new float[9][]
    {
        new float[4] {1, 1, 1, 1}, // center
        new float[4] {0, 1, 1, 1}, // top
        new float[4] {1, 0, 1, 1}, // right
        new float[4] {1, 1, 0, 1}, // bottom
        new float[4] {1, 1, 1, 0}, // left
        new float[4] {0, 1, 1, 0}, // top left
        new float[4] {0, 0, 1, 1}, // top right
        new float[4] {1, 0, 0, 1}, // bottom right
        new float[4] {1, 1, 0, 0}  // bottom left
    };

    public float[] GetAlphas()
    {
        return alphas[(int) vertexType];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float percentageBetween1 = 0.7f;
        float percentageBetween2 = 0.75f;
        Vector3 scale = Vector3.one * 0.2f;
        if (solutionVertex != null)
        {
            Gizmos.DrawLine(transform.position, solutionVertex.transform.position);
            
            Vector3 directionPoint1 =
                transform.position * (1 - percentageBetween1) +
                solutionVertex.transform.position * (percentageBetween1);
            Vector3 directionPoint2 =
                transform.position * (1 - percentageBetween2) +
                solutionVertex.transform.position * (percentageBetween2);
                
            Gizmos.DrawCube(directionPoint1, scale / 2);
            Gizmos.DrawCube(directionPoint2, scale / 4);
        }

        Gizmos.DrawCube(transform.position, scale);
    }
}
