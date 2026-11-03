using System;
using System.Collections.Generic;

[Serializable]
public class PlayerDataSaveData
{
    public int money;
    public List<KeyCodeSaveData> keyCodes = new List<KeyCodeSaveData>();
    public List<int> bookmarks = new List<int>();

}