using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Construct for portal teleporters to and from the safe haven. Only renders
// costly preview plane image when in range of trigger.
public class PortalObjectManager : MonoBehaviour
{
    private List<Collider> touchingColliders;
    private List<GameObject> copiedObjects;

    private void Start()
    {
        touchingColliders = new List<Collider>();
        copiedObjects = new List<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.TeleporterObject &&
            !touchingColliders.Contains(other))
        {
            touchingColliders.Add(other);
            CreateCopy(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TagConstants.TeleporterObject &&
            touchingColliders.Contains(other))
        {
            DeleteCopy(other);
            touchingColliders.Remove(other);
        }
    }

    private void CreateCopy(Collider other)
    {
        GameObject copiedObject =
            GameObject.Instantiate(other.transform.parent.gameObject, transform);
        GameObject copiedTeleporterObject =
            copiedObject.transform.Find("Teleporter Object").gameObject;
        GameObject.Destroy(copiedTeleporterObject);
        copiedObjects.Add(copiedObject);
    }

    private void DeleteCopy(Collider other)
    {
        int indexToDelete = 
            touchingColliders.IndexOf(other);
        GameObject copyToDelete = 
            copiedObjects[indexToDelete];
        copiedObjects.RemoveAt(indexToDelete);
        GameObject.Destroy(copyToDelete);
    }
}
