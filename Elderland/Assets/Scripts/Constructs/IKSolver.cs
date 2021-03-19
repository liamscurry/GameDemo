using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Inverse kinematic solver used for a variety of applications including animation
* limiting based on surrounding geometry.
*/
public class IKSolver : MonoBehaviour
{
    public class IKPackage 
    {
        public Transform[] Transforms { get; set; }
        public float[] Lengths { get; set; }
        public Transform TargetTransform { get; set; }
        public Transform PoleTransform { get; set; }
        public Transform FootEndTransform { get; set; }
        
        public float Ridgity { get; set; }
        public float BasePoleAngle { get; set; }
        public bool FlipFoot { get; set; }
        public bool ReverseRight { get; set; }
        public bool IgnoreNormalFootRotation { get; set; }

        public float CurrentFootPercent { get; set; }
        public Vector3 LastNormal { get; set; }
        public Vector3 SpaceForward { get; set; }
        public Vector3 SpaceUp { get; set; }
        public Vector3 SpaceRight { get; set; }

        public IKPackage(int numberOfBones)
        {
            Lengths = new float[numberOfBones];
        }
    }

    public static readonly float IKRootRotationMin = 0.5f;
    private static readonly float footSmoothSpeed = 3;
    private static readonly float poleMagMin = 0.01f;

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
        float[] lengths,
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
    * The IK target should not be in a position locally which rotates the top bone past vertical,
    * ie, the targetTransform is high up locally along the y axis.
    * See IKSystem.cs for an example.
    */
    public static void TransformIKSolve(IKPackage p)
    {
        // Need to obscure target position to geometry so limbs don't go through geometry.
        Vector3 limitedTargetPosition;
        Vector3 limitedTargetDirection;
        RaycastHit hit;
        bool hitLimit =
            FindTransformIKLimit(
                p,
                out limitedTargetPosition,
                out limitedTargetDirection,
                out hit);

        // 2D initialization for IKSolve
        Vector2[] points = new Vector2[p.Transforms.Length];
        Matrix4x4 spaceMatrix = 
            Matrix4x4.Rotate(Quaternion.Inverse(Quaternion.LookRotation(p.SpaceForward, p.SpaceUp)));
        Vector3 targetSpacePos = 
            spaceMatrix.MultiplyPoint(limitedTargetPosition - p.Transforms[0].position);

        /*
        Debug space:
        Debug.DrawLine(p.Transforms[0].position, limitedTargetPosition);
        Debug.DrawLine(p.Transforms[0].position, p.Transforms[0].position + p.SpaceForward, Color.blue);
        Debug.DrawLine(p.Transforms[0].position, p.Transforms[0].position + p.SpaceUp, Color.yellow);
        Debug.DrawLine(p.Transforms[0].position, p.Transforms[0].position + p.SpaceRight, Color.red);
        */

        points[points.Length - 1] =
            new Vector2(
                targetSpacePos.z,
                targetSpacePos.x);

        IKSolve(ref points, p.Lengths, p.Ridgity);

        // Map 2D solution to 3D space based on space in the IKPackage.
        for (int i = 1; i < points.Length; i++)
        {
            p.Transforms[i].position = 
                p.Transforms[0].position + 
                points[i].x * p.SpaceForward +
                points[i].y * p.SpaceRight;
        }

        // Need to rotate limb by basePoleAngle for animation calibration.
        Vector3[] storedPositions = new Vector3[points.Length];

        for (int i = 1; i < points.Length; i++)
        {
            storedPositions[i] = 
                p.Transforms[0].position +
                Matho.Rotate(
                    p.Transforms[i].position - p.Transforms[0].position,
                    limitedTargetDirection,
                    p.BasePoleAngle);
        }

        for (int i = 1; i < points.Length; i++)
            p.Transforms[i].position = storedPositions[i];

        if (hitLimit)
            p.LastNormal = hit.normal;

        Vector3 rotatedSpaceUp = 
            Matho.Rotate(p.SpaceUp, limitedTargetDirection, p.BasePoleAngle);

        DirectTransformIK(p, limitedTargetDirection, rotatedSpaceUp);
    }

    /*
    * Helper method for TransformIKSolve
    */
    private static bool FindTransformIKLimit(
        IKPackage p,
        out Vector3 limitedTargetPosition,
        out Vector3 limitedTargetDirection,
        out RaycastHit hit)
    {
        limitedTargetPosition = p.TargetTransform.position;
        RaycastHit limitTarget;
        bool hitLimit = Physics.Raycast(
            p.Transforms[0].position,
            (p.TargetTransform.position - p.Transforms[0].position).normalized,
            out limitTarget,
            (p.TargetTransform.position - p.Transforms[0].position).magnitude,
            LayerConstants.GroundCollision);

        if (hitLimit)
            limitedTargetPosition = limitTarget.point;
        limitedTargetDirection =
            (limitedTargetPosition - p.Transforms[0].position).normalized;

        if (hitLimit)
        {
            p.CurrentFootPercent =
                Mathf.MoveTowards(p.CurrentFootPercent, 1, footSmoothSpeed * Time.deltaTime);
        }
        else
        {
            p.CurrentFootPercent =
                Mathf.MoveTowards(p.CurrentFootPercent, 0, footSmoothSpeed * Time.deltaTime);
        }

        hit = limitTarget;
        return hitLimit;
    }

    /*
    * Helper method for TransformIKSolve.
    * Needed to specify rotation for animator and skinned mesh renderer to know which way the
    * armatures bones are rotated.
    */
    private static void DirectTransformIK(
        IKPackage p,
        Vector3 limitedTargetDirection,
        Vector3 spaceUp)
    {
        Vector3[] storedPositions = new Vector3[p.Transforms.Length];
        for (int i = 0; i < p.Transforms.Length; i++)
        {
            storedPositions[i] = p.Transforms[i].position;
        }

        Vector3 targetDirection = limitedTargetDirection;
        for (int i = 0; i < p.Transforms.Length - 1; i++)
        {
            Vector3 lookDirection =
                storedPositions[i + 1] - storedPositions[i];
            Vector3 up = Vector3.Cross(lookDirection, spaceUp);
            p.Transforms[i].rotation =
                Quaternion.LookRotation(
                    up,
                    lookDirection);  
        }

        for (int i = 0; i < p.Transforms.Length; i++)
        {
            p.Transforms[i].position = storedPositions[i];
        }

        if (!p.IgnoreNormalFootRotation)
            DirectNormalFootRotation(p);
    }

    /*
    * Helper method for DirectTransformIK
    */
    private static void DirectNormalFootRotation(IKPackage p)
    {
        p.Transforms[p.Transforms.Length - 1].rotation = p.TargetTransform.rotation;
        
        Vector3 footPosition = p.Transforms[p.Transforms.Length - 1].position;
        Vector3 footEndPosition = p.FootEndTransform.position;
        RaycastHit limitTargetEnd;
        bool hitLimitEnd = Physics.Raycast(
            p.Transforms[0].position,
            (footEndPosition - p.Transforms[0].position).normalized,
            out limitTargetEnd,
            (footEndPosition - p.Transforms[0].position).magnitude,
            LayerConstants.GroundCollision);
        if (hitLimitEnd)
            footEndPosition = limitTargetEnd.point;
        Vector3 limitedTargetEndDirection = 
            (footEndPosition - footPosition).normalized;

        int footSign = (p.FlipFoot) ? 1 : -1;

        Matrix4x4 footMatrix =
            Matrix4x4.Rotate(Quaternion.FromToRotation(limitedTargetEndDirection, p.TargetTransform.up) *
            p.TargetTransform.rotation);
        Vector3 footUp = -footMatrix.MultiplyPoint(Vector3.forward);

        Quaternion limitedRotation = 
            Quaternion.LookRotation(-footUp, limitedTargetEndDirection);

        // Normal rotation
        Quaternion normalRotation = 
            Quaternion.LookRotation(-footSign * p.LastNormal, limitedTargetEndDirection);

        p.Transforms[p.Transforms.Length - 1].rotation = 
                Quaternion.Lerp(limitedRotation, normalRotation, p.CurrentFootPercent);
    }

    /*
    * Needed to properly initialize IK system
    */
    public static void InitializeTransformIKSolver(IKPackage p)
    {
        p.Lengths = new float[p.Transforms.Length - 1];
        for (int i = 0; i < p.Transforms.Length - 1; i++)
        {
            p.Lengths[i] = Vector3.Distance(p.Transforms[i].position, p.Transforms[i + 1].position);
        }
       
        p.CurrentFootPercent = 0;
        p.LastNormal = Vector3.forward;
    }

    /*
    * Needed for systems that use a pole transform from Blender.
    */ 
    public static void CalculatePoleSpace(IKPackage p)
    {
        Vector3 planeDirection = p.TargetTransform.position - p.Transforms[0].position;
        Vector3 poleNormDir = 
            (p.PoleTransform.position - p.TargetTransform.position) - 
            Matho.Project(p.PoleTransform.position - p.TargetTransform.position, planeDirection);
        poleNormDir.Normalize();
        planeDirection.Normalize();

        if (poleNormDir.magnitude >= poleMagMin)
        {
            p.SpaceForward = planeDirection;
            p.SpaceUp = poleNormDir;
            if (!p.ReverseRight)
            {
                p.SpaceRight = Vector3.Cross(planeDirection, poleNormDir);
            }
            else
            {
                p.SpaceRight = Vector3.Cross(poleNormDir, planeDirection);
            }
        }
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
        IKSolve(ref points, lengths, ridgity);
    }
}

/*
Auto pole IK setup: Not supported anymore
TransformIKSolve:
if (autoPole)
{
    Vector2 projectedTarget = 
        Matho.StandardProjection2D(targetDirection);
    if (Mathf.Abs(targetDirection.x) > maxX)
    {
        targetTransform.position = 
            parentTransform.localToWorldMatrix.MultiplyPoint(
                localRootPosition +
                new Vector3(maxX * Matho.Sign(targetDirection.x), targetDirection.y, targetDirection.z));

        targetDirection =
            parentTransform.worldToLocalMatrix.MultiplyPoint(targetTransform.position) -
            localRootPosition;
        projectedTarget = 
            Matho.StandardProjection2D(targetDirection);
    }
    float targetAngle =
        Matho.AngleBetween(projectedTarget, new Vector2(0, 1));
    float signAngleV =
        Matho.AngleBetween(projectedTarget, new Vector2(1, 0));
    signAngleV = (signAngleV < 90) ? 1 : -1;
    poleAngle += basePoleAngle;
    poleAngle += targetAngle * signAngleV;
}
*/