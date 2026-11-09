using System;
using System.Reflection;
using UnityEngine;

public static class RefValidator
{
    private const BindingFlags Flags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    public static int ValidateAll()
    {
        int missing = 0;

        // includeInactive: 비활성 오브젝트의 컴포넌트도 포함. 프리팹 에셋은 scene 이 유효하지 않아 제외됨
        MonoBehaviour[] all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

        foreach (MonoBehaviour mb in all)
        {
            if (mb == null || !mb.gameObject.scene.IsValid())
                continue;

            missing += Validate(mb);
        }

        if (missing == 0)
            Debug.Log("[RefValidator] 필수 연결 점검 통과");
        else
            Debug.LogError($"[RefValidator] 필수 연결 누락 {missing}개");

        return missing;
    }

    public static int Validate(MonoBehaviour target)
    {
        int missing = 0;

        // 부모 클래스의 private 필드도 검사하려고 상속 단계를 따라 올라감
        for (Type type = target.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
        {
            foreach (FieldInfo field in type.GetFields(Flags))
            {
                if (field.GetCustomAttribute<RequiredRefAttribute>() == null)
                    continue;

                object value = field.GetValue(target);

                // Unity 오브젝트는 "연결 안 됨"/"파괴됨"이 == null 로만 판별됨
                bool isMissing = value == null || (value is UnityEngine.Object obj && obj == null);

                if (isMissing)
                {
                    missing++;
                    Debug.LogError(
                        $"[필수 연결 누락] {target.GetType().Name}.{field.Name}  -  {GetPath(target.transform)}",
                        target);
                }
            }
        }

        return missing;
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}
