using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : IComparable<EnemyGroup>
{
    private List<IEnemyGroup> enemies;
    private const float offsetThreshold = 0.005f;

    public int EnemyCount 
    { 
        get 
        {
            if (enemies == null)
            {
                return 0;
            } 
            else
            {
                return enemies.Count;
            }
        }
    }

    private EnemyGroup()
    {
        enemies = new List<IEnemyGroup>();
    }

    public int CompareTo(EnemyGroup other)
    {
        if (this == other)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
    
    // Calculates weighted center of enemies for movement methods.
    public Vector3 CalculateCenter()
    {
        if (enemies.Count == 0)
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 posSum = Vector3.zero;

            foreach (IEnemyGroup enemy in enemies)
            {
                posSum += enemy.Position;
            }

            return posSum * (1.0f / enemies.Count);
        }
    }

    private void Move(Vector3 center, Vector3 target, float speed)
    {
        Vector3 velocity = 
            (target - center);
        velocity = Matho.StandardProjection3D(velocity);

        if (velocity.magnitude < offsetThreshold)
        {
            return;
        }

        velocity = velocity.normalized * speed;
        
        foreach (IEnemyGroup enemy in enemies)
        {
            enemy.Velocity += velocity;
        }
    }

    private static float AbsoluteAngleBetween(Vector2 v, Vector2 w)
    {
        float percentageBetween = 
            Matho.AngleBetween(v, w) / 180.0f;
        
        if (percentageBetween <= 0.5f)
        {
            percentageBetween *= 2;
        }
        else
        {
            percentageBetween = -2 * (percentageBetween - 1);
        }

        return percentageBetween;
    }

    private float CalculateRotationConstant(Vector3 center, Vector3 target)
    {
        if (enemies.Count == 0)
        {
            return 1;
        }
        else
        {
            float sumAbsAngle = 0;

            Vector3 targetDirection3D = 
                (target - center);
            
            Vector2 targetDirection =
                Matho.StandardProjection2D(targetDirection3D);

            if (targetDirection.magnitude < offsetThreshold)
            {
                return 1;
            }

            targetDirection.Normalize();

            foreach (IEnemyGroup enemy in enemies)
            {
                Vector3 centerDirection3D =
                    (center - enemy.Position);
                Vector2 centerDirection =
                    Matho.StandardProjection2D(centerDirection3D).normalized;

                sumAbsAngle += AbsoluteAngleBetween(targetDirection, centerDirection);
            }

            return sumAbsAngle / enemies.Count;
        }
    }

    private void Rotate(Vector3 center, float speed)
    {
        foreach (IEnemyGroup enemy in enemies)
        {
            Vector3 centerDirection = 
                (center - enemy.Position);
            centerDirection =
                Matho.StandardProjection3D(centerDirection).normalized;
            Vector3 tangentDirection =
                Matho.Rotate(centerDirection, Vector3.up, 90f);
            enemy.Velocity += tangentDirection * speed;
        }
    }

    private void Expand(float speed)
    {
        foreach (IEnemyGroup enemy in enemies)
        {
            if (enemy.NearbyEnemies.Count > 0)
            {
                foreach (IEnemyGroup nearbyEnemy in enemy.NearbyEnemies)
                {
                    Vector3 nearbyDirection =
                        nearbyEnemy.Position - enemy.Position;
                    nearbyDirection = Matho.StandardProjection3D(nearbyDirection);
                    if (nearbyDirection.magnitude > offsetThreshold)
                    {
                        nearbyDirection.Normalize();
                        nearbyEnemy.Velocity += nearbyDirection * speed * 0.5f;
                        enemy.Velocity += -nearbyDirection * speed * 0.5f;
                    }
                }
            }
        }
    }

    public static bool Contains(EnemyGroup group, IEnemyGroup enemy)
    {
        return group.enemies.Contains(enemy);
    }

    // Automatically combines groups and creates groups when
    // two enemies try to group up for autonomous grouping.
    public static void Add(IEnemyGroup e1, IEnemyGroup e2)
    {
        if (e1.Group == null && e2.Group != null)
        {
            e1.Group = e2.Group;
            e2.Group.enemies.Add(e1);
        }
        else if (e2.Group == null && e1.Group != null)
        {
            e2.Group = e1.Group;
            e1.Group.enemies.Add(e2);
        }
        else if (e1.Group == null && e2.Group == null)
        {
            // No Groups
            EnemyGroup newGroup = 
                new EnemyGroup();
            e1.Group = newGroup;
            e2.Group = newGroup;
            newGroup.enemies.Add(e1);
            newGroup.enemies.Add(e2);
        }
        else
        {
            // Two Groups
            EnemyGroup temp = 
                e1.Group;

            foreach (IEnemyGroup enemy in temp.enemies)
            {
                enemy.Group = e2.Group;
                e2.Group.enemies.Add(enemy);
            }

            temp.enemies.Clear();
        }
    }

    // Removes an enemy from its group, deleting group if empty afterwards for automation.
    public static void Remove(IEnemyGroup e)
    {
        EnemyGroup temp = e.Group;
        e.Group = null;
        temp.enemies.Remove(e);
    }

    // Unit Tests
    public static void AddTest()
    {
        // Both no group
        var e1 = new EnemyGroupUTDummy();
        var e2 = new EnemyGroupUTDummy();

        EnemyGroup.Add(e1, e2);
        UT.CheckEquality<EnemyGroup>(e1.Group, e2.Group);
        UT.CheckDifference<EnemyGroup>(e1.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 2);

        // First null
        var e3 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e3, e1);
        UT.CheckEquality<EnemyGroup>(e3.Group, e1.Group);
        UT.CheckDifference<EnemyGroup>(e3.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 3);

        // Second null
        var e4 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e1, e4);
        UT.CheckEquality<EnemyGroup>(e1.Group, e4.Group);
        UT.CheckDifference<EnemyGroup>(e4.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 4);

        // Both have group
        var e5 = new EnemyGroupUTDummy();
        var e6 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e5, e6);
        EnemyGroup.Add(e5, e1);
        UT.CheckEquality<EnemyGroup>(e1.Group, e5.Group);
        UT.CheckDifference<EnemyGroup>(e5.Group, null);   
        UT.CheckEquality<EnemyGroup>(e5.Group, e6.Group);
        UT.CheckDifference<EnemyGroup>(e6.Group, null);   
        UT.CheckEquality<int>(e1.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e2.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e5.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e6.Group.EnemyCount, 6);
    }

    public static void RemoveTest()
    {
        var e1 = new EnemyGroupUTDummy();
        var e2 = new EnemyGroupUTDummy();
        var e3 = new EnemyGroupUTDummy();

        EnemyGroup.Add(e1, e2);
        EnemyGroup.Add(e1, e3);

        UT.CheckEquality<int>(e1.Group.EnemyCount, 3);
        EnemyGroup.Remove(e2);
        UT.CheckEquality<int>(e1.Group.EnemyCount, 2);
        UT.CheckDifference<IEnemyGroup>(e2, null);
        UT.CheckEquality<EnemyGroup>(e2.Group, null);
        UT.CheckEquality<bool>(EnemyGroup.Contains(e1.Group, e2), false);
    }
    
    public static void CalculateCenterTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        UT.CheckEquality<bool>(e1.Group.CalculateCenter() == new Vector3(2, 0, 0), true);

        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, -3));
        var e4 = new EnemyGroupUTDummy(new Vector3(2, 1, 0));
        var e5 = new EnemyGroupUTDummy(new Vector3(3, 2, 0));
        EnemyGroup.Add(e3, e4);
        EnemyGroup.Add(e3, e5);
        UT.CheckEquality<bool>(e3.Group.CalculateCenter() == new Vector3(2, 1, -1), true);

        var e6 = new EnemyGroupUTDummy(new Vector3(2, 3, 3));
        EnemyGroup.Add(e3, e6);
        EnemyGroup.Add(e1, e3);
        UT.CheckEquality<bool>(e3.Group.CalculateCenter() == new Vector3(2, 1, 0), true);
    }

    public static void MoveTest()
    {
        // Positive test
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();

        e1.Group.Move(center1, new Vector3(2, 0, 2), 1);
        UT.CheckEquality<bool>(e1.Velocity == new Vector3(0, 0, 1), true);
        UT.CheckEquality<bool>(e2.Velocity == new Vector3(0, 0, 1), true);
        
        // Negative test
        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e4 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e3, e4);
        Vector3 center2 =
            e3.Group.CalculateCenter();

        e3.Group.Move(center2, new Vector3(2, 0, -2), 1);
        UT.CheckEquality<bool>(e3.Velocity == new Vector3(0, 0, -1), true);
        UT.CheckEquality<bool>(e4.Velocity == new Vector3(0, 0, -1), true);

        // Diagonal Test
        var e5 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e6 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e5, e6);
        Vector3 center3 =
            e5.Group.CalculateCenter();

        e5.Group.Move(center3, new Vector3(0, 0, 2), 1);
        Vector2 diagonalTestDirection = 
            Matho.DirectionVectorFromAngle(135.0f);
        
        UT.CheckEquality<bool>(
            Matho.AngleBetween(e5.Velocity, new Vector3(diagonalTestDirection.x, 0, diagonalTestDirection.y)) < 5f, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(e6.Velocity, new Vector3(diagonalTestDirection.x, 0, diagonalTestDirection.y)) < 5f, true);

        // Vertical Test
        var e7 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e8 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e7, e8);
        Vector3 center4 =
            e7.Group.CalculateCenter();

        e7.Group.Move(center4, new Vector3(2, -2, 0), 1);
        UT.CheckEquality<bool>(e7.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e8.Velocity == new Vector3(0, 0, 0), true);

        // Origin Test
        var e9 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e10 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e9, e10);
        Vector3 center5 =
            e9.Group.CalculateCenter();

        e9.Group.Move(center5, new Vector3(2, 0, 0), 1);
        UT.CheckEquality<bool>(e9.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e10.Velocity == new Vector3(0, 0, 0), true);

        e9.Group.Move(center5, new Vector3(2 + offsetThreshold * 0.5f, 0, 0), 1);
        UT.CheckEquality<bool>(e9.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e10.Velocity == new Vector3(0, 0, 0), true);
    }

    public static void AbsoluteAngleBetweenTest()
    {
        float a1 = AbsoluteAngleBetween(Vector2.down, Vector2.right);
        UT.CheckEquality<bool>(Matho.IsInRange(a1, 1, UT.Threshold), true);

        float a2 = AbsoluteAngleBetween(Vector2.down, -Vector2.right);
        UT.CheckEquality<bool>(Matho.IsInRange(a2, 1, UT.Threshold), true);

        float a3 = AbsoluteAngleBetween(Vector2.down, Vector2.up);
        UT.CheckEquality<bool>(Matho.IsInRange(a3, 0, UT.Threshold), true);

        float a4 = AbsoluteAngleBetween(Vector2.down, (Vector2.right + Vector2.up).normalized);
        float a5 = AbsoluteAngleBetween(Vector2.down, (Vector2.right - Vector2.up).normalized);
        UT.CheckEquality<bool>(Matho.IsInRange(a4, a5, UT.Threshold), true);

        float a6 = AbsoluteAngleBetween(Vector2.down, (-Vector2.right + Vector2.up).normalized);
        float a7 = AbsoluteAngleBetween(Vector2.down, (-Vector2.right - Vector2.up).normalized);
        UT.CheckEquality<bool>(Matho.IsInRange(a6, a7, UT.Threshold), true);
    }

    public static void CalculateRotationConstantTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();
        
        // Cardinal tests
        float r1 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, 0, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r1, 1, UT.Threshold), true);

        float r2 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(5, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r2, 0, UT.Threshold), true);

        float r3 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(-5, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r3, 0, UT.Threshold), true);

        float r4 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, 0, 3));
        UT.CheckEquality<bool>(Matho.IsInRange(r4, 1, UT.Threshold), true);

        // Diagonal tests
        float r5 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(0, 0, -2));
        UT.CheckEquality<bool>(Matho.IsInRange(r5, 0.5f, UT.Threshold), true);

        float r6 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(0, 0, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r6, 0.5f, UT.Threshold), true);

        float r7 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 0, -2));
        UT.CheckEquality<bool>(Matho.IsInRange(r7, 0.5f, UT.Threshold), true);

        float r8 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 0, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r8, 0.5f, UT.Threshold), true);

        // Vertical tests
        float r9 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 6, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r9, 0.5f, UT.Threshold), true);

        float r10 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, -6, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r10, 0.5f, UT.Threshold), true);

        float r11 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, -7, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r11, 1, UT.Threshold), true);

        // Empty test
        EnemyGroup linkGroup =
            e1.Group;

        EnemyGroup.Remove(e1);
        EnemyGroup.Remove(e2);
        Vector3 center2 =
            linkGroup.CalculateCenter();
        float r12 = 
            linkGroup.CalculateRotationConstant(center2, new Vector3(2, -7, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r12, 1, UT.Threshold), true);
        
        // In Crowd test
        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e4 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e3, e4);
        Vector3 center3 =
            e3.Group.CalculateCenter();
        
        // Cardinal tests
        float r13 = 
            e3.Group.CalculateRotationConstant(center3, new Vector3(2, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r13, 1, UT.Threshold), true);

        float r14 = 
            e3.Group.CalculateRotationConstant(center3, new Vector3(2.5f, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r14, 0, UT.Threshold), true);
    }

    public static void RotateTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();

        // Cardinal tests
        e1.Group.Rotate(center1, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, -1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e2.Velocity, new Vector3(0, 0, 1), UT.Threshold), true);

        var e3 = new EnemyGroupUTDummy(new Vector3(0, 0, 1));
        var e4 = new EnemyGroupUTDummy(new Vector3(0, 0, -1));

        EnemyGroup.Add(e1, e3);
        EnemyGroup.Add(e1, e4);

        Vector3 center2 =
            e1.Group.CalculateCenter();
        e1.Group.Rotate(center1, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, -2), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e2.Velocity, new Vector3(0, 0, 2), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e3.Velocity, new Vector3(-1, 0, 0), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e4.Velocity, new Vector3(1, 0, 0), UT.Threshold), true);

        // Diagonal test
        var d1 = new EnemyGroupUTDummy(new Vector3(0.5f, 0, 0.5f));
        var d2 = new EnemyGroupUTDummy(new Vector3(-0.5f, 0, 0.5f));
        var d3 = new EnemyGroupUTDummy(new Vector3(-0.5f, 0, -0.5f));
        var d4 = new EnemyGroupUTDummy(new Vector3(0.5f, 0, -0.5f));
        EnemyGroup.Add(d1, d2);
        EnemyGroup.Add(d2, d3);
        EnemyGroup.Add(d3, d4);
        Vector3 center3 =
            d1.Group.CalculateCenter();
        d1.Group.Rotate(center3, 1);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d1.Velocity, new Vector3(-Matho.Diagonal, 0, Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d2.Velocity, new Vector3(-Matho.Diagonal, 0, -Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d3.Velocity, new Vector3(Matho.Diagonal, 0, -Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d4.Velocity, new Vector3(Matho.Diagonal, 0, Matho.Diagonal)) < UT.Threshold, true);

        // Center test
        var w1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var w2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var w0 = new EnemyGroupUTDummy(Vector3.zero);
        EnemyGroup.Add(w0, w1);
        EnemyGroup.Add(w0, w2);
        Vector3 center4 =
            w1.Group.CalculateCenter();
        w1.Group.Rotate(center4, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(w1.Velocity, new Vector3(0, 0, -1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(w2.Velocity, new Vector3(0, 0, 1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(w0.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);
    } 

    public static void ExpandTest()
    {
        // Single bunch
        var e1 = new EnemyGroupUTDummy(new Vector3(0, 0, 0));

        // Duo bunch
        var e2 = new EnemyGroupUTDummy(new Vector3(5, 0, 3));
        var e3 = new EnemyGroupUTDummy(new Vector3(6, 0, 3));
        e2.NearbyEnemies.Add(e3);
        e3.NearbyEnemies.Add(e2);

        // Trio bunch
        var e4 = new EnemyGroupUTDummy(new Vector3(-5 - Matho.Diagonal, 0, -3 - Matho.Diagonal));
        var e5 = new EnemyGroupUTDummy(new Vector3(-5, 0, -3));
        var e6 = new EnemyGroupUTDummy(new Vector3(-5 + Matho.Diagonal, 0, -3 + Matho.Diagonal));
        e4.NearbyEnemies.Add(e5);
        e4.NearbyEnemies.Add(e6);

        e5.NearbyEnemies.Add(e4);
        e5.NearbyEnemies.Add(e6);

        e6.NearbyEnemies.Add(e4);
        e6.NearbyEnemies.Add(e5);

        EnemyGroup.Add(e1, e2);
        EnemyGroup.Add(e2, e3);
        EnemyGroup.Add(e3, e4);
        EnemyGroup.Add(e4, e5);
        EnemyGroup.Add(e5, e6);

        e1.Group.Expand(1);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);

        UT.CheckEquality<bool>(Matho.IsInRange(e2.Velocity, new Vector3(-1, 0, 0), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e3.Velocity, new Vector3(1, 0, 0), UT.Threshold), true);

        UT.CheckEquality<bool>(Matho.IsInRange(e4.Velocity, new Vector3(-2 * Matho.Diagonal, 0, -2 * Matho.Diagonal), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e5.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e6.Velocity, new Vector3(2 * Matho.Diagonal, 0, 2 * Matho.Diagonal), UT.Threshold), true);
    }
}