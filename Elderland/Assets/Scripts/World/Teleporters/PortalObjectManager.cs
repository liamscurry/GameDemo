using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Construct for portal teleporters to and from the safe haven. Only renders
// costly preview plane image when in range of trigger.
public class PortalObjectManager : MonoBehaviour
{
    private List<Collider> touchingColliders;
    private List<GameObject> copiedObjects;
    private List<GameObject> targetObjects;

    private PortalTeleporter teleporter;

    public GameObject PlayerCopy { get; private set; }

    private void Start()
    {
        touchingColliders = new List<Collider>();
        copiedObjects = new List<GameObject>();
        targetObjects = new List<GameObject>();
        teleporter = transform.parent.GetComponentInChildren<PortalTeleporter>();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < copiedObjects.Count; i++)
        {
            Transform armatureParent = 
                copiedObjects[i].transform.Find("Armature");
            Transform targetParent =
                targetObjects[i].transform.Find("Armature");

            teleporter.RootMirror(armatureParent, targetParent);
            RecursiveMirror(armatureParent, targetParent);
        }
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
        targetObjects.Add(other.transform.parent.gameObject);

        if (other.transform.parent.parent != null && 
            other.transform.parent.parent.gameObject.name == TagConstants.Player)
            PlayerCopy = copiedObject;
    }

    private void DeleteCopy(Collider other)
    {
        if (other == PlayerCopy)
            PlayerCopy = null;

        int indexToDelete = 
            touchingColliders.IndexOf(other);
        GameObject copyToDelete = 
            copiedObjects[indexToDelete];
        copiedObjects.RemoveAt(indexToDelete);
        targetObjects.RemoveAt(indexToDelete);
        StartCoroutine(DeleteCopyCoroutine(copyToDelete));
    }

    private IEnumerator DeleteCopyCoroutine(GameObject copyToDelete)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        GameObject.Destroy(copyToDelete);
    }

    private void RecursiveMirror(Transform root, Transform targetRoot, int count = 0)
    {
        /*
        string name = "";
        for (int i = 0; i < count; i++)
        {
            name += "  ";
        }

        Debug.Log(name + (root.name + ", " + targetRoot.name));
        */
        if (count != 0)
        {
            root.localPosition = targetRoot.localPosition;
            root.localRotation = targetRoot.localRotation;
            root.localScale = targetRoot.localScale;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RecursiveMirror(root.GetChild(i), targetRoot.GetChild(i), count + 1);
        }
    }
}
