using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Inverse kinematic solver used for a variety of applications including animation
* limiting based on surrounding geometry.
*/
public class IKSolver : MonoBehaviour
{
    public static readonly float IKRootRotationMin = 0.5f;

    /*
    * Generates a specific angle of a triangle with length d, l and h.
    * Specifically generates the angle between d and l segments.
    * Triangle is not neccesarily a right triangle.
    */
    private static float TriangleAngle(float d, float l, float h)
    {
        float numerator =
            Mathf.Pow(h, 2) - Mathf.Pow(l, 2) - Mathf.Pow(d, 2);
        float denominator = 
            -2 * l * d;
            
        float clampedRatio = 
            numerator / denominator;
        if (clampedRatio > 1)
        {
            return 0;
        }
        if (clampedRatio < -1)
        {
            return 180;
        }

        return Mathf.Acos(numerator / denominator) * Mathf.Rad2Deg;
    }

    /*
    * Generates offset from current point and vertical axis.
    * l is length of offset, down is rotated theta degrees counter-clockwise for positive theta.
    */
    private static Vector2 GenerateOffset(Vector2 down, float theta, float l)
    {
        return Matho.Rotate(down, -theta).normalized * l;
    }

    /*
    * Needed to generate next point from a given start and end vector defining the current tilted
    * vertical axis of the IK.
    * Rotates theta degrees counter-clockwise from the direction vector from start to end if 
    * theta is positive.
    */
    private static Vector2 GeneratePoint(Vector2 start, Vector2 end, float theta, float l)
    {
        return start + GenerateOffset(end - start, theta, l);
    }

    /*
    * Needed to determine convergence of advanced IK solver with a large number of joints.
    * end is the global end fixed point.
    * last, current and next are points considering the discrepency angle (last then current then
    * next)
    * Return value: true for segment bending against will, false for segment bending naturally
    */
    private static bool Discrepency(
        Vector2 end,
        Vector2 last,
        Vector2 current,
        Vector2 next)
    {
        Vector2 toLast = last - current;
        Vector2 toNext = next - current;
        float discrepencyAngle = 
            Matho.AngleBetween(toLast, toNext);

        // Need to make sure angle corresponds to outside angle.
        Vector2 projCurrentOffset = 
            Matho.Project(current - last, next - last);
        Vector2 projCurrent =
            last + projCurrentOffset;

        return (Vector2.Distance(end, current) < Vector2.Distance(end, projCurrent));
    }

    /*
    * Solves an n point IK problem given a set of points and lengths, where n > 3.
    * Edits input point array via reference. Increase ridgity if end bone is stretching.
    */
    private static void IKSolve(
        ref Vector2[] points,
        ref float[] lengths,
        float ridgity
    )
    {
        float lower = 0;
        float upper = 1;

        Vector2 end = points[points.Length - 1];

        float totalLength = 0;
        for (int i = 0; i < lengths.Length; i++)
            totalLength += lengths[i];
        float lengthUsed = 0;

        bool isFlat = false;
        Vector2 endDirection = (points[points.Length - 1] - points[0]).normalized;
        if ((points[0] - points[points.Length - 1]).magnitude > totalLength)
            isFlat = true;

        for (int i = 0; i < points.Length - 2; i++)
        {
            Vector2 currentPoint = Vector2.zero;
            float dI = (points[i] - points[points.Length - 1]).magnitude;
            lengthUsed += lengths[i];

            float stiffness = 
                (lower * (1 - ridgity) + upper * (ridgity));

            float theta = TriangleAngle(dI, lengths[i], totalLength - lengthUsed) * stiffness;
            currentPoint = GeneratePoint(points[i], end, theta, lengths[i]);

            float dINext = (currentPoint - points[points.Length - 1]).magnitude;
            float nextTheta =
                TriangleAngle(dINext, lengths[i + 1], totalLength - lengthUsed - lengths[i + 1]);
            Vector2 nextPoint =
                GeneratePoint(currentPoint, end, nextTheta, lengths[i + 1]);

            if (Discrepency(end, points[i], currentPoint, nextPoint))
            {
                // true, joint bending against will.
                upper = (lower + upper) / 2;
            }
            else
            {
                lower = (lower + upper) / 2;
            }

            lower = 0;
            upper = 1;
            points[i + 1] = currentPoint;
        }

        if (isFlat)
            points[points.Length - 1] = points[0] + endDirection * totalLength;
    }

    /*
    * Solves a 3D IK problem given a set of joint points as transforms and a 
    * pole angle that specifies how tilted the limb is and maps from 2D answer to 3D space.
    * In simple 
    */
    public static void TransformIKSolveAnimation(
        Transform spaceTransform,
        Transform targetTransform,
        Transform[] transforms,
        float[] lengths,
        float ridgity,
        bool adjustSpaceTransform = false)
    {
        // Solution from IKSolve maps to up and forward vectors of spaceTransform.
        // This is the offset from the first transform point.
        Vector2[] points = new Vector2[transforms.Length];
        Vector3 startOffset =
            (targetTransform.position - transforms[0].position);
        Vector3 forwardProjection =
            Matho.Project(startOffset, spaceTransform.forward);
        Vector3 upProjection =
            Matho.Project(startOffset, spaceTransform.up);
        int forwardSign =
            (Matho.AngleBetween(forwardProjection, spaceTransform.forward) < 90) ? 1 : -1;
        int upSign =
            (Matho.AngleBetween(upProjection, spaceTransform.up) < 90) ? 1 : -1;
        points[points.Length - 1] =
            new Vector2(
                forwardProjection.magnitude * forwardSign,
                upProjection.magnitude * upSign); // need signs for projections.

        IKSolve(ref points, ref lengths, ridgity);

        for (int i = 1; i < points.Length; i++)
        {
            transforms[i].position = 
                transforms[0].position + 
                points[i].y * spaceTransform.up +
                points[i].x * spaceTransform.forward;
        }

        if (adjustSpaceTransform)
        {
            Vector3 startDirection =
                transforms[1].position - transforms[0].position;

            spaceTransform.rotation =
                Quaternion.FromToRotation(spaceTransform.forward, startDirection) *
                spaceTransform.rotation;
        }
    }

    /*
    * Solves a 3D IK problem given a set of joint points as transforms and a 
    * pole angle that specifies how tilted the limb is and maps from 2D answer to 3D space.
    * In this raw case, the limb should only be moved/rotated from the target transform and the pole angle
    * and the parent transform of the whole IK rig subsystem (ex, parent of targetTransform)
    * You can call ResetTransformIKSolver to reset the targetTransform and space transform locally.
    * Call InitializeTransformIKSolver to initialize the rig subsystem.
    * The IK target should not be in a position locally which rotates the top bone past vertical,
    * ie, the targetTransform is high up locally along the y axis.
    */
    public static void TransformIKSolveRaw(
        Transform targetTransform,
        Transform[] transforms,
        float[] lengths,
        float ridgity,
        float poleAngle)
    {
        Transform spaceTransform = 
            transforms[0];

        Vector3 localRootPosition =
            targetTransform.parent.worldToLocalMatrix.MultiplyPoint(
                transforms[0].position);
        Vector3 targetDirection =
            targetTransform.parent.worldToLocalMatrix.MultiplyPoint(targetTransform.position) -
            localRootPosition;
        Vector2 projectedTarget = 
            Matho.StandardProjection2D(targetDirection);
        float maxTargetX = 1f;
        if (Mathf.Abs(targetDirection.x) > maxTargetX)
        {
            targetTransform.position = 
                targetTransform.parent.localToWorldMatrix.MultiplyPoint(
                    localRootPosition +
                    new Vector3(maxTargetX * Matho.Sign(targetDirection.x), targetDirection.y, targetDirection.z));

            targetDirection =
                targetTransform.parent.worldToLocalMatrix.MultiplyPoint(targetTransform.position) -
                localRootPosition;
            projectedTarget = 
                Matho.StandardProjection2D(targetDirection);
        }
        float targetAngle =
            Matho.AngleBetween(projectedTarget, new Vector2(0, 1));
        float signAngleV =
            Matho.AngleBetween(projectedTarget, new Vector2(1, 0));
        signAngleV = (signAngleV < 90) ? 1 : -1;
        poleAngle += targetAngle * signAngleV;

        Vector3 currentEulerAngles =
            transforms[0].localRotation.eulerAngles;
        spaceTransform.localRotation =   
            Quaternion.Euler(
                currentEulerAngles.x,
                -Matho.Angle(projectedTarget) + 90,
                currentEulerAngles.z);

        // Solution from IKSolve maps to up and forward vectors of spaceTransform.
        // This is the offset from the first transform point.
        Vector2[] points = new Vector2[transforms.Length];
        Vector3 startOffset =
            (targetTransform.position - transforms[0].position);
        Vector3 forwardProjection =
            Matho.Project(startOffset, spaceTransform.forward);
        Vector3 upProjection =
            Matho.Project(startOffset, spaceTransform.up);
        int forwardSign =
            (Matho.AngleBetween(forwardProjection, spaceTransform.forward) < 90) ? 1 : -1;
        int upSign =
            (Matho.AngleBetween(upProjection, spaceTransform.up) < 90) ? 1 : -1;
        points[points.Length - 1] =
            new Vector2(
                forwardProjection.magnitude * forwardSign,
                upProjection.magnitude * upSign);

        IKSolve(ref points, ref lengths, ridgity);

        for (int i = 1; i < points.Length; i++)
        {
            transforms[i].position = 
                transforms[0].position + 
                points[i].y * spaceTransform.up +
                points[i].x * spaceTransform.forward;
        }

        Vector3[] storedPositions = new Vector3[points.Length];
        Vector3 ikDirection = targetTransform.position - transforms[0].position;
        for (int i = 1; i < points.Length; i++)
        {
            storedPositions[i] = 
                transforms[0].position +
                Matho.Rotate(transforms[i].position - transforms[0].position, ikDirection, poleAngle);
        }

        Vector3 startDirection =
            transforms[1].position - transforms[0].position;

        spaceTransform.rotation =
            Quaternion.FromToRotation(spaceTransform.forward, startDirection) *
            spaceTransform.rotation;

        for (int i = 1; i < points.Length; i++)
            transforms[i].position = storedPositions[i];
    }

    /*
    * This method is needed in cases which you want to reset the state of the raw IK system via script
    * or editor button/key input.
    */
    public static void ResetTransformIKSolverRaw(
        Transform targetTransform,
        Transform[] transforms,
        float[] lengths,
        float ridgity,
        float poleAngle,
        Quaternion startRootRotation,
        Vector3 startTargetPosition)
    {
        transforms[0].localRotation =
            startRootRotation;
        targetTransform.position = 
            transforms[0].localToWorldMatrix.MultiplyPoint(startTargetPosition);
        TransformIKSolveRaw(
            targetTransform,
            transforms,
            lengths,
            ridgity,
            poleAngle);
    }

    /*
    * Needed to properly initialize raw IK system
    */
    public static void InitializeTransformIKSolverRaw(
        Transform targetTransform,
        Transform[] transforms,
        ref Quaternion startRootRotation,
        ref Vector3 startTargetPosition)
    {
        startTargetPosition = 
            transforms[0].worldToLocalMatrix.MultiplyPoint(targetTransform.position);
        startRootRotation = transforms[0].localRotation;
    }

    public static void TriangleAngleTests()
    {
        // Right test
        float d = 3;
        float l = 4;
        float h = 5;
        UT.CheckEquality<bool>(
            Matho.IsInRange(TriangleAngle(d, l, h), 90, UT.Threshold),
            true);  

        //Acute test
        float d2 = 10;
        float l2 = 1;
        float h2 = 9.02f;
        UT.CheckEquality<bool>(
            Matho.IsInRange(TriangleAngle(d2, l2, h2), 10, IKSolverUT.AngleThreshold),
            true);  
        
        // Obtuse test
        float d3 = 10;
        float l3 = 9;
        float h3 = 16.46f;
        UT.CheckEquality<bool>(
            Matho.IsInRange(TriangleAngle(d3, l3, h3), 120, IKSolverUT.AngleThreshold),
            true);  
    }

    public static void GenerateOffsetTests()
    {
        Vector2[] standardSolutions = 
        {
            new Vector2(1,0),
            new Vector2(2,0),
            new Vector2(Matho.Diagonal, -Matho.Diagonal),
            new Vector2(Matho.Diagonal, -Matho.Diagonal) * 2,
            new Vector2(0, -1),
            new Vector2(0, -2)
        };
        TestCurrentOffset(Vector2.down, standardSolutions);

        Vector2[] tiltedSolutions = 
        {
            new Vector2(Matho.Diagonal, -Matho.Diagonal),
            new Vector2(Matho.Diagonal, -Matho.Diagonal) * 2,
            new Vector2(0, -1),
            new Vector2(0, -1) * 2,
            new Vector2(-Matho.Diagonal, -Matho.Diagonal),
            new Vector2(-Matho.Diagonal, -Matho.Diagonal) * 2
        };
        TestCurrentOffset(new Vector2(-Matho.Diagonal, -Matho.Diagonal), tiltedSolutions);
    }

    private static void TestCurrentOffset(Vector2 down, Vector2[] solutions)
    {
        float theta1 = 90f;
        float length1 = 1;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta1, length1),
                solutions[0],
                UT.Threshold),
            true); 
        
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta1, length1 * 2),
                solutions[1],
                UT.Threshold),
            true); 

        float theta2 = 45f;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta2, length1),
                solutions[2],
                UT.Threshold),
            true); 
        
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta2, length1 * 2),
                solutions[3],
                UT.Threshold),
            true); 

        float theta3 = 0;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta3, length1),
                solutions[4],
                UT.Threshold),
            true); 
        
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GenerateOffset(down, theta3, length1 * 2),
                solutions[5],
                UT.Threshold),
            true);
    }

    public static void GeneratePointTests()
    {
        Vector2 start1 = Vector2.zero;
        Vector2 end1 = Vector2.down;
        float theta1 = 45;
        float l1 = 1;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GeneratePoint(start1, end1, theta1, l1),
                new Vector2(Matho.Diagonal, -Matho.Diagonal),
                UT.Threshold),
            true);
        
        Vector2 end2 = new Vector2(-Matho.Diagonal, -Matho.Diagonal);
        float l2 = 2;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GeneratePoint(start1, end2, theta1, l2),
                new Vector2(0, -2),
                UT.Threshold),
            true);

        Vector2 start3 = Vector2.zero + Vector2.right;
        Vector2 end3 = Vector2.down + Vector2.right;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GeneratePoint(start3, end3, theta1, l1),
                new Vector2(Matho.Diagonal, -Matho.Diagonal) + Vector2.right,
                UT.Threshold),
            true);
        
        Vector2 end4 = new Vector2(-Matho.Diagonal, -Matho.Diagonal) + Vector2.right;
        float l4 = 2;
        UT.CheckEquality<bool>(
            Matho.IsInRange(
                GeneratePoint(start3, end4, theta1, l4),
                new Vector2(0, -2) + Vector2.right,
                UT.Threshold),
            true);
    }

    public static void DiscrepencyTests()
    {
        Vector2 end = Vector2.zero;
        Vector2 last1 = new Vector2(0, 1);
        Vector2 current1 = new Vector2(0.2f, 0.2f);
        Vector2 next1 = new Vector2(1, 0.1f);
        UT.CheckEquality<bool>(Discrepency(end, last1, current1, next1), true);
        Vector2 current2 = new Vector2(1, 1);
        UT.CheckEquality<bool>(Discrepency(end, last1, current2, next1), false);

        UT.CheckEquality<bool>(Discrepency(-end, -last1, -current1, -next1), true);
        UT.CheckEquality<bool>(Discrepency(-end, -last1, -current2, -next1), false);
    }

    public static void IKSolveTests(
        ref Vector2[] points,
        ref float[] lengths,
        float ridgity
    )
    {
        IKSolve(ref points, ref lengths, ridgity);
    }
}
