using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingFireballSensor : MonoBehaviour
{
    public List<EnemyManager> EnemiesInRange { get; private set; }
    public bool EnemyInRange { get; private set; }
    public float Range { get; private set; }

    private void Start()
    {
        EnemiesInRange = new List<EnemyManager>();
        EnemyInRange = false;
        Range = GetComponent<SphereCollider>().radius;
    }

    public void Reset()
    {
        EnemiesInRange.Clear();
        EnemyInRange = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemiesInRange.Add(other.GetComponent<EnemyManager>());
        if (!EnemyInRange)
        {
            EnemyInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EnemiesInRange.Remove(other.GetComponent<EnemyManager>());
        if (EnemiesInRange.Count == 0)
        {
            EnemyInRange = false;
        }
    }
}
