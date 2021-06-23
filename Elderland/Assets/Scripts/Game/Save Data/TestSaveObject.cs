using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class TestSaveObject : BaseSaveObject
{
    public override string Save(SaveManager saveManager, bool resetSave = false)
    {
        CheckID(saveManager, resetSave);
        return ID + " " + "{\"test\": 1}";
    }

    public override void Load(string jsonString)
    {

    }
}