public interface ISettingsData
{
    string Group { get; }
    string Key { get; }
    System.Type GetDataType();
    T GetData<T>();
    void SetData<T>(T value);
}