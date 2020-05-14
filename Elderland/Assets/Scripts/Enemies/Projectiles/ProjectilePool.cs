using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Pool which manages when projectiles are created and reused.
public class ProjectilePool : MonoBehaviour 
{
    //Delegate which stores each projectiles type's clear method.
    private static Action clearProjectiles;

    //Adds projectile to pool, called on projectile recycle.
    public void Add<T>(GameObject obj) where T : Projectile
    {
        ProjectileGroup<T>.AsleepProjectiles.Add(obj.GetComponent<T>());
        obj.SetActive(false);
    }

    //Returns a projectile and sets its properties
    public T Create<T>(
        GameObject projectile,
        Vector3 position,
        Vector3 velocity,
        float time,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info) where T : Projectile, new()
    {
        //If no projectiles are ready for reuse, create new one
        if (ProjectileGroup<T>.AsleepProjectiles.Count == 0)
        {
            GameObject obj = Instantiate(projectile, Vector3.zero, Quaternion.identity) as GameObject;
            //Default values
            T p = obj.GetComponentInChildren<T>();
            p.Initialize(position, velocity, time, targetTag, hitTarget, info);
            obj.transform.parent = gameObject.transform;

            if (!ProjectileGroup<T>.InUse)
            {
                clearProjectiles += ProjectileGroup<T>.Clear;
                ProjectileGroup<T>.InUse = true;
            }

            ProjectileGroup<T>.Projectiles.Add(p);

            return p;
        }
        else
        //use older object
        {
            T p = ProjectileGroup<T>.AsleepProjectiles[0];
            ProjectileGroup<T>.AsleepProjectiles[0].gameObject.SetActive(true);      
            ProjectileGroup<T>.AsleepProjectiles[0].Reset(position, velocity, time, targetTag, hitTarget, info);
            ProjectileGroup<T>.AsleepProjectiles.RemoveAt(0);  

            return p;          
        }
    }

    //Clear all current projectiles
    public void ClearProjectilePool()
    {
        if (clearProjectiles != null)
        {
            clearProjectiles();
            clearProjectiles = null;
        }
    }

    //Stores a list of all the current projectiles according to type
    private static class ProjectileGroup<T> where T : Projectile
    {
        public static List<T> AsleepProjectiles { get; set; }
        public static List<T> Projectiles { get; set; }
        public static bool InUse { get; set; }

        static ProjectileGroup()
        {
            AsleepProjectiles = new List<T>();
            Projectiles = new List<T>();
            InUse = false;
        }

        //Destroys and clears all of a specific projectile type
        public static void Clear()
        {
            foreach (T t in Projectiles)
            {
                Destroy(t.transform.gameObject);
            }

            AsleepProjectiles.Clear();
            Projectiles.Clear();
            InUse = false;
        }
    }
}
