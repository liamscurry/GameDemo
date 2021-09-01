using UnityEngine;
using UnityEditor;

/*
Base class for save objects. If implementing directly, follow structure of CheckID carefully.
If objects used initialize methods, make sure to call initialze at the start of both save and load
methods.

Syntax:
Objects that hold save data are called <Name>Object, and so is their child component that inherits
from this class. The object we are serializing is called <Name>Data and is a private nested class
of the above <Name>Object component. Save data fields need to be public.
*/
[System.Serializable]
[ExecuteAlways]
public abstract class BaseSaveObject : MonoBehaviour, SaveObject
{
    public GameObject GameObject { get { return gameObject; } }
    [HideInInspector]
    [SerializeField]
    protected int id;
    public int ID { get { return id; } }
    public abstract string Save(SaveManager saveManager, bool resetSave = false);
    public abstract void Load(string jsonString);

    /*
    Creates an ID if the object does not have an ID or the save file is wiped. 
    Base implementation of check ID. Should always be called when save is called. 

    Inputs:
    SaveManager : saveManager : Which save manager to use. Generally simply GameInfo.SaveManager.
    bool : resetSave : Field to indicate that the ID should be regenerated upon save structure updates in development.

    Outputs:
    None
    */
    public void CheckID(SaveManager saveManager, bool resetSave) 
    {
        if (id == 0 || resetSave)
        {
            id = saveManager.RequestUniqueID();
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
    }

    /*
    Helper event method called during editor events. Needed to reset id when pasted/duplicated.

    Inputs:
    None

    Outputs:
    None

    References:
    Unity Event, EventType, OnInspectorGUI API
    */
    private void OnGUI()
    {
        //Debug.Log("called gui");
        if (Event.current.commandName == "Paste")//Event.current.type == EventType.ValidateCommand && 
        {
            Debug.Log("paste reset");
            /*
            id = 0;
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            */
        }
    }
}