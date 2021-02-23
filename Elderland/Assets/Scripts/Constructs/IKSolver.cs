using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Inverse kinematic solver used for a variety of applications including animation
* limiting based on surrounding geometry.
*/
public class IKSolver : MonoBehaviour
{
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
    * Solves a three point IK problem given the bone length and end points.
    * Edits input vectors via reference.
    */
    private static void IKSolveSimple(
        ref Vector2 start,
        ref Vector2 end,
        ref Vector2 middle,
        float l1,
        float l2)
    {
        if (l1 + l2 < (start - end).magnitude)
        {
            middle = start + (end - start).normalized * (l1);
            end =    start + (end - start).normalized * (l1 + l2);
        }
        else
        {
            float d = (start - end).magnitude;
            float theta = TriangleAngle(d, l1, l2);
            Vector2 solution = GeneratePoint(start, end, theta, l1);
            middle = solution;
        }
    }

    /*
    * Solves an n point IK problem given a set of points and lengths, where n > 3.
    * Edits input point array via reference. k is the number of iterations for each step.
    * ridgity is in the range 0.0 to 1.0. Increase ridgity if end bone is stretching.
    */
    private static void IKSolve(
        ref Vector2[] points,
        ref float[] lengths,
        float ridgity
    )
    {
        if (points.Length <= 3)
        {
            throw new System.Exception(
                "IKSolve must have n > 3 points. Consider using IKSolve simple.");
        }
        else
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

    public static void IKSolveSimpleTests(
        ref Vector2 start,
        ref Vector2 end,
        ref Vector2 middle,
        float l1,
        float l2)
    {
        IKSolveSimple(ref start, ref end, ref middle, l1, l2);
    }
}
