using UnityEngine;

[CreateAssetMenu(menuName = "Building/Data")]
public class BuildingData : ScriptableObject
{
    // 프리팹에 저장된 데이터의 구조체(프리팹이랑 같이 보관)
    public int itemID;
    public int width;
    public int height;
    public int cost;

    // true = 통과 가능
    // false = 막힘
    public bool[] patternFlat;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (patternFlat == null || patternFlat.Length != width * height)
        {
            patternFlat = new bool[width * height];
        }
    }
#endif
}
