public interface ISaveRepository
{
    bool Exists();
    bool TryLoad(out SaveData saveData);
    bool TrySave(SaveData saveData);
    bool Delete();
}
