using System.Collections;
using System.Collections.Generic;

public interface ISettings
{
    object RawData { get; }
    ISettingsData[] GetData();
    void Save();
}