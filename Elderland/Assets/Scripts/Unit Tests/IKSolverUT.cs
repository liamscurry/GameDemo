using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolverUT : MonoBehaviour
{
    [Header("Generic IK Solver Test")]
    [SerializeField]
    private bool testGeneric;
    [SerializeField]
    private Vector2[] points;
    [SerializeField]
    private float[] lengths;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float ridgity;

    [Header("Transform Ik Solver Test")]
    [SerializeField]
    private Transform spaceTransform;
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Transform[] transforms;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float transformRidgity;
    [SerializeField]
    [Range(-90, 90)]
    private float transformPoleAngle;

    public static readonly float AngleThreshold = 2f;

    private float[] transformLengths;
    private Vector3 startTargetPosition;
    private Quaternion startRootRotation;
    private void Start()
    {
        StartCoroutine(TestCoroutine());
        
        IKSolver.InitializeTransformIKSolver(
                spaceTransform,
                targetTransform,
                transforms,
                ref transformLengths,
                ref startRootRotation,
                ref startTargetPosition);
    }

    private void LateUpdate()
    {
        IKSolver.TransformIKSolve(
            spaceTransform,
            targetTransform,
            transforms,
            transformLengths,
            transformRidgity,
            transformPoleAngle);

        if (Input.GetKeyDown(KeyCode.R))
        {
            IKSolver.ResetTransformIKSolver(
                spaceTransform,
                targetTransform,
                transforms,
                transformLengths,
                transformRidgity,
                transformPoleAngle,
                startRootRotation,
                startTargetPosition);
        }
    }

    private IEnumerator TestCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            IKSolver.TriangleAngleTests();
            IKSolver.GenerateOffsetTests();
            IKSolver.GeneratePointTests();
            IKSolver.DiscrepencyTests();

            Debug.Log("IKSolver: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("IKSolver: Failed. " + e.Message + " " + e.StackTrace);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (testGeneric)
        {
            Vector2[] pointsCopy = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                pointsCopy[i] = new Vector2(points[i].x, points[i].y);
            }
            float[] lengthsCopy = lengths;
            IKSolver.IKSolveTests(ref pointsCopy, ref lengthsCopy, ridgity);
            Gizmos.color = Color.cyan;
            for (int i = 0; i < lengths.Length; i++)
                Gizmos.DrawLine(pointsCopy[i], pointsCopy[i + 1]);
            for (int i = 0; i < points.Length; i++)
                Gizmos.DrawCube(pointsCopy[i], Vector3.one * 0.25f);
        }
    }
}