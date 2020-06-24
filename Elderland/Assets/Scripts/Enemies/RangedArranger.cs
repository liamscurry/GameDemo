using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedArranger
{  
    //true is open area, false is in wall
    public Vector2 Center { get; set; }
    public readonly int n;
    public float nodeStartAngle;
    public readonly bool[] nodesCalculated;
    public readonly bool[] nodesAvailability;
    public readonly float nodeSpacing;
    public readonly float radius;
    private bool clearedThisFrame;

    public RangedArranger(Vector2 center, float radius, int n, float nodeStartAngle = 0)
    {
        if (radius <= 0)
        {
            throw new System.ArgumentException("Radius of nodes must be greater than zero");
        }
        else if (n <= 0)
        {
            throw new System.ArgumentException("Size of group nodes must be at least 1");
        }
        else
        {
            this.Center = center;
            this.radius = radius;
            this.n = n;
            float reducedStartAngle = nodeStartAngle % 360;
            this.nodeStartAngle = (reducedStartAngle >= 0) ? reducedStartAngle : reducedStartAngle + 360;
            nodesCalculated = new bool[n];
            ClearNodesCalculated();
            nodesAvailability = new bool[n];
            nodeSpacing = 360f / n;
        }
    }

    public void LateUpdateArranger()
    {
        ClearNodesCalculated();
    }

    public void GetValidIndex(Vector3 position, int direction, int ignoreIndex, ref int returnIndex)
    {
        float generalIndex = GetGeneralIndex(position);
        float exactIndex = generalIndex % n;
        int index = Matho.Round(exactIndex) % n;

        if (direction == 1)
        {
            index = Mathf.CeilToInt(exactIndex) % n;
        }
        else
        {
            index = Mathf.FloorToInt(exactIndex) % n;
        }

        if (index != ignoreIndex && GetValidity(index))
        {
            //Case: center index viable
            returnIndex = index;
        }
        else
        {
            //Case: search surrounding nodes for viable
            if (direction == 1)
            {
                returnIndex = LeftSearch(index, ignoreIndex);
            }
            else
            {
                returnIndex = RightSearch(index, ignoreIndex);
            }
        }
    }

    private int RightSearch(int index, int ignoreIndex)
    {
        int specificCount = 0;
        int pairCount = 1;
        while (true)
        {
            //Right check
            int pairRight = (index - pairCount) % n;
            if (pairRight < 0)
                pairRight += n;
            if (pairRight != ignoreIndex && GetValidity(pairRight))
            {

                return pairRight;
            }
            specificCount++;

            if (specificCount == n - 1)
            {

                return -1;
            }

            //Left check
            int pairLeft = (index + pairCount) % n;
            if (pairLeft != ignoreIndex && GetValidity(pairLeft))
            {

                return pairLeft;
            }
            specificCount++;

            if (specificCount == n - 1)
            {

                return - 1;
            }

            pairCount++;
        }
    }

    private int LeftSearch(int index, int ignoreIndex)
    {
        int specificCount = 0;
        int pairCount = 1;
        while (true)
        {
            //Left check
            int pairLeft = (index + pairCount) % n;
            if (pairLeft != ignoreIndex && GetValidity(pairLeft))
            {
                return pairLeft;
            }
            specificCount++;

            if (specificCount == n - 1)
            {
                return -1;
            }

            //Right check
            int pairRight = (index - pairCount) % n;
            if (pairRight < 0)
                pairRight += n;
            if (pairRight != ignoreIndex && GetValidity(pairRight))
            {
                return pairRight;
            }
            specificCount++;

            if (specificCount == n - 1)
            {
                return -1;
            }

            pairCount++;
        }
    }

    private int GetIndex(Vector3 position)
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(position);
        float globalAngle = Matho.Angle(projectedPosition - Center);
        float localAngle = globalAngle - nodeStartAngle;
        float correctedLocalAngle = (globalAngle >= nodeStartAngle) ? localAngle: localAngle + 360;
        return Matho.Round(correctedLocalAngle / nodeSpacing) % n;
    }

    private float GetGeneralIndex(Vector3 position)
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(position);
        float globalAngle = Matho.Angle(projectedPosition - Center);
        float localAngle = globalAngle - nodeStartAngle;
        float correctedLocalAngle = (globalAngle >= nodeStartAngle) ? localAngle: localAngle + 360;
        return (correctedLocalAngle / nodeSpacing);
    }

    private void ClearNodesCalculated()
    {
        for (int index = 0; index < n; index++)
        {
            nodesCalculated[index] = false;
        }
    }

    private float GetAngle(float node)
    {
        if (node < 0 || node >= n)
        {
            throw new System.ArgumentException("Not proper index");
        }
        else
        {
            return ((node * nodeSpacing) + nodeStartAngle) % 360;
        }
    }

    public Vector2 GetPosition(float node)
    {
        if (node < 0 || node >= n)
        {
            throw new System.ArgumentException("Not proper index: " + node);
        }
        else
        {
            return Center + radius * Matho.DirectionVectorFromAngle(GetAngle(node));
        }
    }

    public Vector2 GetPosition(float node, Vector2 center)
    {
        if (node < 0 || node >= n)
        {
            throw new System.ArgumentException("Not proper index: " + node);
        }
        else
        {
            return center + radius * Matho.DirectionVectorFromAngle(GetAngle(node));
        }
    }
    
    public bool GetValidity(int index)
    {
        if (nodesCalculated[index])
        {
            return nodesAvailability[index];
        }
        else
        {
            return CheckValidity(index);
        }
    }

    public bool GetValidity(int index, Vector2 center)
    {
        return CheckValidity(index, center);
    }

    private bool CheckValidity(int index)
    {
        Vector2 position = GetPosition(index);
        Vector3 positionNav = GameInfo.CurrentLevel.NavCast(position);
        Vector3 centerNav = GameInfo.CurrentLevel.NavCast(Center);
        
        NavMeshHit hitInfo;
        bool validity = !NavMesh.Raycast(centerNav, positionNav, out hitInfo, NavMesh.AllAreas);
        nodesAvailability[index] = validity;
        nodesCalculated[index] = true;
        return validity;
    }

    private bool CheckValidity(int index, Vector2 center)
    {
        Vector2 position = GetPosition(index);
        Vector3 positionNav = GameInfo.CurrentLevel.NavCast(position);
        Vector3 centerNav = GameInfo.CurrentLevel.NavCast(center);
        
        NavMeshHit hitInfo;
        bool validity = !NavMesh.Raycast(centerNav, positionNav, out hitInfo, NavMesh.AllAreas);
        nodesAvailability[index] = validity;
        nodesCalculated[index] = true;
        return validity;
    }
}
