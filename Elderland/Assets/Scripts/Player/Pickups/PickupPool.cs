using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pool which manages when pickups are created and reused.
public class PickupPool : MonoBehaviour 
{
    // Delegate which stores each projectiles type's clear method.
    private static Action clearPickups;

    // Adds projectile to pool, called on projectile recycle.
    public void Add<T>(GameObject obj) where T : Pickup
    {
        PickupGroup<T>.AsleepPickups.Add(obj.GetComponent<T>());
        obj.SetActive(false);
    }

    // Returns a pickup and sets its properties
    public T Create<T>(
        GameObject pickup,
        Vector3 position) where T : Pickup, new() // only need to test this function
    {
        // If no pickup are ready for reuse, create new one
        if (PickupGroup<T>.AsleepPickups.Count == 0)
        {
            GameObject obj = Instantiate(pickup, position, Quaternion.identity) as GameObject;
            // Default values
            T p = obj.GetComponentInChildren<T>();
            obj.transform.parent = gameObject.transform;

            if (!PickupGroup<T>.InUse)
            {
                clearPickups += PickupGroup<T>.Clear;
                PickupGroup<T>.InUse = true;
            }

            PickupGroup<T>.Pickups.Add(p);

            return p;
        }
        else
        // Use older object
        {
            T p = PickupGroup<T>.AsleepPickups[0];
            PickupGroup<T>.AsleepPickups[0].gameObject.SetActive(true);      
            PickupGroup<T>.AsleepPickups[0].Reset(position);
            PickupGroup<T>.AsleepPickups.RemoveAt(0);  

            return p;          
        }
    }

    // Clear all current projectiles
    public void ClearPickupPool()
    {
        if (clearPickups != null)
        {
            clearPickups();
            clearPickups = null;
        }
    }

    // Stores a list of all the current projectiles according to type
    private static class PickupGroup<T> where T : Pickup
    {
        public static List<T> AsleepPickups { get; set; }
        public static List<T> Pickups { get; set; }
        public static bool InUse { get; set; }

        static PickupGroup()
        {
            AsleepPickups = new List<T>();
            Pickups = new List<T>();
            InUse = false;
        }

        // Destroys and clears all of a specific pickup type
        public static void Clear()
        {
            foreach (T t in Pickups)
            {
                Destroy(t.transform.gameObject);
            }

            AsleepPickups.Clear();
            Pickups.Clear();
            InUse = false;
        }
    }
}
