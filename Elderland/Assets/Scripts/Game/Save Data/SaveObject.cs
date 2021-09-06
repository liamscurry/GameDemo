using UnityEngine;
// References Microsoft C# file docs.

public interface SaveObject
{
    int ID { get; }
    GameObject GameObject { get; }
    // Format for Json in files is [ID jsonString\n] without braces. One object per line.
    string Save(SaveManager saveManager, bool resetSave = false);
    void Load(string jsonString);
    void CheckID(SaveManager saveManager, bool resetSave);
    void PrintID();
}