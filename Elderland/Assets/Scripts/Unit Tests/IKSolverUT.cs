using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolverUT : MonoBehaviour
{
    [Header("Simple IK Solver Test")]
    [SerializeField]
    private bool testSimple;
    [SerializeField]
    private Vector2 start;
    [SerializeField]
    private Vector2 end;
    [SerializeField]
    private float l1;
    [SerializeField]
    private float l2;

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
    private Transform targetTransform;
    [SerializeField]
    [HideInInspector]
    private Vector3 lastTargetDirection;
    [SerializeField]
    [HideInInspector]
    private Vector3 lastLocalTargetDirection;
    [SerializeField]
    [HideInInspector]
    private Quaternion lastRootRotation;
    [SerializeField]
    private Transform[] transforms;
    [SerializeField]
    private float[] transformLengths;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float transformRidgity;
    [SerializeField]
    [Range(-90, 90)]
    private float transformPoleAngle;

    public static readonly float AngleThreshold = 2f;

    private void Start()
    {
        StartCoroutine(TestCoroutine());

        IKSolver.InitializeTransformIKSolver(
            targetTransform,
            ref lastTargetDirection,
            transforms);
        
        IKSolver.TransformIKSolveRaw(
            targetTransform,
            ref lastTargetDirection,
            transforms,
            transformLengths,
            transformRidgity,
            0);
    }

    private void Update()
    {
        IKSolver.TransformIKSolveRaw(
            targetTransform,
            ref lastTargetDirection,
            transforms,
            transformLengths,
            transformRidgity,
            transformPoleAngle);
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
            /*
            Transform[] transforms = new Transform[2];
            transforms[0] = transform.parent;
            transforms[1] = transform;
            IKSolver.TransformIKSolve(transform, transforms);*/

            Debug.Log("IKSolver: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("IKSolver: Failed. " + e.Message + " " + e.StackTrace);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (testSimple)
        {
            Vector2 startCopy = start;
            Vector2 endCopy = end;
            Vector2 middle = Vector2.zero;
            IKSolver.IKSolveSimpleTests(ref startCopy, ref endCopy, ref middle, l1, l2);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startCopy, middle);
            Gizmos.DrawLine(middle, endCopy);
            Gizmos.DrawCube(startCopy, Vector3.one * 0.25f);
            Gizmos.DrawCube(middle, Vector3.one * 0.25f);
            Gizmos.DrawCube(endCopy, Vector3.one * 0.25f);
        }
        else if (testGeneric)
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