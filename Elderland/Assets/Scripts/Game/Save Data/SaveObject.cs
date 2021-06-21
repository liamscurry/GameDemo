// References Microsoft C# file docs.

public interface SaveObject
{
    int ID { get; set; }
    string Save();
    void Load(string jsonString);
}