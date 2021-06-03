using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeArranger
{
    //true is avaliable, false is taken
    public Vector2 Center { get; set; }
    public readonly int n;
    public float nodeStartAngle;
    public readonly EnemyManager[] nodes;
    public readonly float nodeSpacing;
    public readonly float radius;

    public MeleeArranger(Vector2 center, float radius, int n, float nodeStartAngle = 0)
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
            nodes = new EnemyManager[n];
            for (int index = 0; index < n; index++)
            {
                nodes[index] = null;
            }
            nodeSpacing = 360f / n;
        }
    }

    public void ClearNodes()
    {
        for (int index = 0; index < n; index++)
        {
            if (nodes[index] != null)
            {
                nodes[index] = null;
            }
        }
    }

    public void ClearNode(int index)
    {
        if (index < 0 || index >= n)
        {
            throw new System.ArgumentException("Not proper index");
        }
        else
        {
            if (nodes[index] != null)
            {
                nodes[index] = null;
            }
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
            throw new System.ArgumentException("Not proper index: " + n);
        }
        else
        {
            return Center + radius * Matho.DirectionVectorFromAngle(GetAngle(node));
        }
    }

    public int GetIndex(Vector2 position)
    {
        float globalAngle = Matho.Angle(position - Center);
        float localAngle = globalAngle - nodeStartAngle;
        float correctedLocalAngle = (globalAngle >= nodeStartAngle) ? localAngle: localAngle + 360;
        return Matho.Round(correctedLocalAngle / nodeSpacing) % n;
    }

    public int GetValidIndex(Vector2 position)
    {
        float generalIndex = GetGeneralIndex(position);
        float exactIndex = generalIndex % n;
        int index = Matho.Round(exactIndex) % n;

        if (CheckValidity(index))
        {
            //Case: center index viable
            return index;
        }
        else
        {
            //Case: search surrounding nodes for viable
            if (exactIndex > index)
            {
                return LeftSearch(index, false);
            }
            else
            {
                return RightSearch(index, false);
            }
        }
    }

    public float GetExactIndex(Vector2 position)
    {
        return GetGeneralIndex(position) % n;
    }

    private float GetGeneralIndex(Vector2 position)
    {
        float globalAngle = Matho.Angle(position - Center);
        float localAngle = globalAngle - nodeStartAngle;
        float correctedLocalAngle = (globalAngle >= nodeStartAngle) ? localAngle: localAngle + 360;
        return (correctedLocalAngle / nodeSpacing);
    }

    public bool OverrideNode(EnemyManager manager)
    {
        int index = GetIndex(Matho.StdProj2D(manager.transform.position));
        if (index != manager.ArrangementNode)
        {
            if (nodes[index] == null)
            {
                if (CheckValidity(index))
                {
                    ClearNode(manager.ArrangementNode);
                    nodes[index] = manager;
                    manager.ArrangementNode = index;
                }
            }
            else
            {              
                EnemyManager other = nodes[index];
                if (other.ArrangementNode == -1)
                {

                }
                Vector2 nodePosition = GetPosition(other.ArrangementNode);
                nodePosition = Vector2.MoveTowards(nodePosition, Matho.StdProj2D(PlayerInfo.Player.transform.position), 1.5f);
                float distance = (Matho.StdProj2D(manager.transform.position) - nodePosition).magnitude;
                float otherDistance = (Matho.StdProj2D(other.transform.position) - nodePosition).magnitude;
                if (distance + 0.5f < otherDistance)
                {
                    ClearNode(manager.ArrangementNode);
                    nodes[index] = manager;
                    manager.ArrangementNode = index;
                    ClaimNode(other);
                    other.CalculateAgentPath();
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public void ClaimNode(EnemyManager manager)
    {
        Vector2 position = Matho.StdProj2D(manager.transform.position);
        float generalIndex = GetGeneralIndex(position);
        float exactIndex = generalIndex % n;
        int index = Matho.Round(exactIndex) % n;

        if (nodes[index] == null && CheckValidity(index))
        {
            //Case: center index viable
            nodes[index] = manager;
            manager.ArrangementNode = index;
        }
        else
        {
            //Case: search surrounding nodes for viable
            int searchIndex = (exactIndex > index) ? LeftSearch(index, true) : RightSearch(index, true);

            if (searchIndex != -1)
            {
                nodes[searchIndex] = manager;
                manager.ArrangementNode = searchIndex;
            }
            else
            {
                if (manager.ArrangementNode != -1)
                {
                    ClearNode(manager.ArrangementNode);
                    manager.ArrangementNode = -1;
                }
            }
        }
    }

    private int RightSearch(int index, bool prohibitTaken)
    {
        int specificCount = 0;
        int pairCount = 1;
        while (true)
        {
            //Right check
            int pairRight = (index - pairCount) % n;
            if (pairRight < 0)
                pairRight += n;
            if ((!prohibitTaken && CheckValidity(pairRight)) || 
                (prohibitTaken && (CheckValidity(pairRight) && nodes[pairRight] == null)))
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
            if ((!prohibitTaken && CheckValidity(pairLeft)) || 
                (prohibitTaken && (CheckValidity(pairLeft) && nodes[pairLeft] == null)))
            {
                return pairLeft;
            }
            specificCount++;

            if (specificCount == n - 1)
            {
                return -1;
            }

            pairCount++;
        }
    }

    private int LeftSearch(int index, bool prohibitTaken)
    {
        int specificCount = 0;
        int pairCount = 1;
        while (true)
        {
            //Left check
            int pairLeft = (index + pairCount) % n;
            if ((!prohibitTaken && CheckValidity(pairLeft)) || 
                (prohibitTaken && (CheckValidity(pairLeft) && nodes[pairLeft] == null)))
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
            if ((!prohibitTaken && CheckValidity(pairRight)) || 
                (prohibitTaken && (CheckValidity(pairRight) && nodes[pairRight] == null)))
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

    public bool CheckValidity(int index)
    {
        Vector2 position = EnemyInfo.MeleeArranger.GetPosition(index);
        Vector3 positionNav = GameInfo.CurrentLevel.NavCast(position);
        Vector2 playerPosition = Matho.StdProj2D(PlayerInfo.Player.transform.position);
        Vector3 playerPositionNav = GameInfo.CurrentLevel.NavCast(playerPosition);
        
        NavMeshHit hitInfo;
        return !NavMesh.Raycast(playerPositionNav, positionNav, out hitInfo, NavMesh.AllAreas);
    }
}