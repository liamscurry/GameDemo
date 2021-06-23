using UnityEngine;
using UnityEditor;

[System.Serializable]
public abstract class BaseSaveObject : MonoBehaviour, SaveObject
{
    public GameObject GameObject { get { return gameObject; } }
    [HideInInspector]
    [SerializeField]
    protected int id;
    public int ID { get { return id; } }
    public abstract string Save(SaveManager saveManager, bool resetSave = false);
    public abstract void Load(string jsonString);
    protected void CheckID(SaveManager saveManager, bool resetSave) 
    {
        Debug.Log(id + ", " + resetSave);
        if (id == 0 || resetSave)
        {
            id = saveManager.RequestUniqueID();
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
    }
}