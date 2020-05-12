using UnityEngine;
using System.Collections.Generic;

//Common math operations
public static class Matho 
{
    //Constants//
    public const float Diagonal = 0.7071067812f;

    //Range methods//
    public static bool IsInRange(float one, float two, float leeway)
    {
        return one >= two - leeway && one <= two + leeway;
    }

    public static bool IsInRange(Vector2 one, Vector2 two, float leeway)
    {
        return IsInRange(one.x, two.x, leeway) && IsInRange(one.y, two.y, leeway);
    }

    public static bool IsInRange(Vector3 one, Vector3 two, float leeway)
    {
        return IsInRange(one.x, two.x, leeway) && IsInRange(one.y, two.y, leeway) && IsInRange(one.z, two.z, leeway);
    }

    //Returns a positive angle of a vector's components
    public static float Angle(Vector2 components)
    {
        float a = Mathf.Atan2(components.y, components.x) * Mathf.Rad2Deg;
        //Third quadrant
        if (components.x <= 0 && components.y < 0)
        {
            a += 360;
        }
        //Fourth quadrant
        else if (components.x >= 0 && components.y < 0)
        {
            a += 360;
        }
        return a;
    }

    //Determines the sign of a float. 1 when positive, 0 when zero and -1 when negative.
    public static float Sign(float f)
    {
        if (f > 0)
        {
            return 1;
        }
        else if (f < 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    //Vector relations//
    //Absolute angle between two vectors.
    public static float AngleBetween(Vector2 v1, Vector2 v2)
    {
        float f = Mathf.Clamp(((v1.x * v2.x) + (v1.y * v2.y)) / (v1.magnitude * v2.magnitude), -1, 1);
        return Mathf.Acos(f) * Mathf.Rad2Deg;
    }

    //Absolute angle between two vectors.
    public static float AngleBetween(Vector3 v1, Vector3 v2)
    {
        float f = Mathf.Clamp(((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)) / (v1.magnitude * v2.magnitude), -1, 1);
        return Mathf.Acos(f) * Mathf.Rad2Deg;
    }

    //Vector transformations//    
    //Reflects vector v1 over the line infinently spanned in the direction of v2.
    public static Vector2 Reflect(Vector2 v1, Vector2 v2)
    {
        float f = ((v1.x * v2.x) + (v1.y * v2.y)) / (v2.magnitude * v2.magnitude);
        return (2 * f) * v2 - v1;
    }

    public static Vector3 Reflect(Vector3 v1, Vector3 v2)
    {
        float f = ((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)) / (v2.magnitude * v2.magnitude);
        return (2 * f) * v2 - v1;
    }

    //Projects vector v1 onto the line infinently spanned in the direction of v2.
    public static Vector2 Project(Vector2 v1, Vector2 v2)
    {
        float f = ((v1.x * v2.x) + (v1.y * v2.y)) / (v2.magnitude * v2.magnitude);
        return f * v2;
    }

    public static Vector3 Project(Vector3 v1, Vector3 v2, bool print = false)
    {
        if (print)
        {
            //Debug.Log("v1: " + v1.magnitude);
            //Debug.Log("v2: " + v2.magnitude);
            //Debug.Log("numer: " + ((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)));
            //Debug.Log("denom: " + (v2.magnitude * v2.magnitude));
        }
        float f = ((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)) / (v2.magnitude * v2.magnitude);
        return f * v2;
    }

    public static float ProjectScalar(Vector2 v1, Vector2 v2)
    {
        float f = ((v1.x * v2.x) + (v1.y * v2.y)) / (v2.magnitude * v2.magnitude);
        return f;
    }

    public static float ProjectScalar(Vector3 v1, Vector3 v2)
    {
        float f = ((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)) / (v2.magnitude * v2.magnitude);
        return f;
    }

    //Rotates vector v theta degrees. Positive theta is clockwise, and negative theta is counter-clockwise.
    public static Vector2 Rotate(Vector2 v, float t)
    {
        float cos = Mathf.Cos(-t * Mathf.Deg2Rad);
        float sin = Mathf.Sin(-t * Mathf.Deg2Rad);
        return new Vector2((cos * v.x) - (sin * v.y), (sin * v.x) + (cos * v.y));
    }

    //Rotates vector v theta degrees about the normal n. Positive theta is clockwise, and negative theta is counter-clockwise.
    public static Vector3 Rotate(Vector3 v, Vector3 n, float t)
    {
        float m = v.magnitude;
        n = n.normalized;
        v = v.normalized;
        Vector3 i = Vector3.Cross(n, v).normalized;
        Vector3 j = Vector3.Cross(n, i).normalized;

        float x1 = (i.x * v.x) + (i.y * v.y) + (i.z * v.z);
        float y1 = (n.x * v.x) + (n.y * v.y) + (n.z * v.z);
        float z1 = (j.x * v.x) + (j.y * v.y) + (j.z * v.z);

        float x2 = (Mathf.Cos(t * Mathf.Deg2Rad) * x1) - (Mathf.Sin(t * Mathf.Deg2Rad) * z1);
        float y2 = y1;
        float z2 = (Mathf.Sin(t * Mathf.Deg2Rad) * x1) + (Mathf.Cos(t * Mathf.Deg2Rad) * z1);

        float x3 = (i.x * x2) + (n.x * y2) + (j.x * z2);
        float y3 = (i.y * x2) + (n.y * y2) + (j.y * z2);
        float z3 = (i.z * x2) + (n.z * y2) + (j.z * z2);

        return m * new Vector3(x3, y3, z3);
    }

    //Rotates vector v1 towards v2 in the shortest angular direction.
    public static Vector2 RotateTowards(Vector2 v1, Vector2 v2, float t)
    {
        Vector2 leftTarget = Rotate(v2, 90);
        Vector2 rightTarget = Rotate(v2, -90); 

        float leftTheta = AngleBetween(v1, leftTarget);
        float rightTheta = AngleBetween(v1, rightTarget);

        float speed = (leftTheta < rightTheta) ? -1 * t : t;
        float theta = AngleBetween(v1, v2);

        return (Mathf.Abs(speed) < theta) ? Rotate(v1, speed) : v2 * v1.magnitude;
    }

    //Rotates vector v1 towards v2 in the shortest angular direction.
    public static Vector3 RotateTowards(Vector3 v1, Vector3 v2, float t)
    {
        Vector3 normal = Vector3.Cross(v1, v2).normalized;

        Vector3 leftTarget = Rotate(v2, normal, 90);
        Vector3 rightTarget = Rotate(v2, normal, -90);

        float leftTheta = AngleBetween(v1, leftTarget);
        float rightTheta = AngleBetween(v1, rightTarget);

        float speed = (leftTheta < rightTheta) ? -1 * t : t;
        float theta = AngleBetween(v1, v2);

        return (Mathf.Abs(speed) < theta) ? Rotate(v1, normal, speed) : v2 * v1.magnitude;
    }

    //Projects v onto the x-z plane, converting to a Vector2 in the process.
    public static Vector2 StandardProjection2D(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 StandardProjection3D(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    //Calculates the directional derivative of a plane given its normal.
    public static Vector3 PlanarDirectionalDerivative(Vector2 direction, Vector3 normal)
    {
        direction.Normalize();
        float x = direction.x;
        float y = (normal.x / -normal.y) * direction.x + (normal.z / -normal.y) * direction.y;
        float z = direction.y;
        return new Vector3(x, y, z).normalized;
    }

    public static Vector2 PolarToCartesian(float r, float theta)
    {
        float x = r * Mathf.Cos(theta * Mathf.Deg2Rad);
        float y = r * Mathf.Sin(theta * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }

    public static Vector3 SphericalToCartesianX(float row, float theta, float phi)
    {
        float x = row * Mathf.Cos(theta * Mathf.Deg2Rad) * Mathf.Sin(phi * Mathf.Deg2Rad);
        float y = row * Mathf.Cos(phi * Mathf.Deg2Rad);
        float z = row * Mathf.Sin(theta * Mathf.Deg2Rad) * Mathf.Sin(phi * Mathf.Deg2Rad);
        return new Vector3(x, y, z);
    }

    public static Vector3 SphericalToCartesianZ(float row, float theta, float phi)
    {
        float x = row * Mathf.Sin(theta * Mathf.Deg2Rad) * Mathf.Sin(phi * Mathf.Deg2Rad);
        float y = row * Mathf.Cos(phi * Mathf.Deg2Rad);
        float z = row * Mathf.Cos(theta * Mathf.Deg2Rad) * Mathf.Sin(phi * Mathf.Deg2Rad);
        return new Vector3(x, y, z);
    }

    public static Vector3 CylindricalToCartesian(float r, float theta, float h)
    {
        float x = r * Mathf.Cos(theta * Mathf.Deg2Rad);
        float y = h;
        float z = r * Mathf.Sin(theta * Mathf.Deg2Rad);
        return new Vector3(x, y, z);
    }
    
    public static Vector2 DirectionVectorFromAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public static int Round(float f)
    {
		if (f < ((int) f) + 0.5)
			return (int) f;
		return ((int) f) + 1;
    }

    /*
    //Returns a positive angle given two components
    public static float Angle(float x, float y)
    {
        float a = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        //Third quadrant
        if (x <= 0 && y <= 0)
        {
            a += 360;
        }
        //Fourth quadrant
        else if (x >= 0 && y <= 0)
        {
            a += 360;
        }
        return a;
    }

    //Returns a positive angle based on two position vectors. 
    public static float AngleFromPositions(Vector2 start, Vector2 end)
    {
        Vector2 components = new Vector2(end.x - start.x,  end.y - start.y);
        return Angle(components);
    }
    
    //Returns the direction vector beginning at the start position, pointing towards the end position
    public static Vector2 DirectionVectorFromAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }
     
    public static Vector2 DirectionVectorFromPositions(Vector2 start, Vector2 end)
    {
        float angle = AngleFromPositions(start, end);
        return DirectionVectorFromAngle(angle);
    }

    //Used for finding smallest value in certain collections
    public static float SmallestValue(params float[] list)
    {
        float minValue = 0;
        foreach (float f in list)
        {
            if (f < minValue) minValue = f;
        }

        return minValue;
    }

    public static float SmallestValue(List<float> list)
    {
        float minValue = 0;
        foreach (float f in list)
        {
            if (f < minValue) minValue = f;
        }

        return minValue;
    }

    public static int SmallestIndexValue(List<float> list)
    {
        float minValue = (list.Count != 0) ? list[0] : 0;
        int minIndex = 0;

        for (int i = 0; i < list.Count; i++)
        {        
            if (list[i] < minValue) 
            { 
                minIndex = i; 
                minValue = list[i]; 
            }
        }

        return minIndex;
    }
    
    //Returns a standard rounded integer
    public static int Round(float f)
    {
		if (f < ((int) f) + 0.5)
			return (int) f;
		return ((int) f) + 1;
    }

    //Returns a rounded up integer
    public static int RoundUp(float f)
    {
        int i = (int) f;
        if (i == f)
        {
            return i;
        }
        else
        {
            return i + 1;
        }
    }
    */
}
