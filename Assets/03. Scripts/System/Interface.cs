
using System.Collections.Generic;

public interface Loader<Key, Value>
{
    public Dictionary<Key, Value> MakeDict();
}