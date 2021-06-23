// References Microsoft C# file docs.

public interface SaveObject
{
    int ID { get; set; }
    string Save(SaveManager saveManager, bool resetSave = false);
    void Load(string jsonString);
}