#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class RefValidatorEditor
{
    private const string StopKey = "RefValidator.StopPlayOnMissing";
    private const string StopMenu = "Tools/필수 연결 점검/누락 시 플레이 중단";

    static RefValidatorEditor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static bool StopPlay
    {
        get => EditorPrefs.GetBool(StopKey, true);
        set => EditorPrefs.SetBool(StopKey, value);
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // 플레이 모드로 들어가기 직전 (편집 모드의 씬 상태를 검사)
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        int missing = RefValidator.ValidateAll();

        if (missing > 0 && StopPlay)
        {
            EditorApplication.isPlaying = false;
            Debug.LogError("[RefValidator] 필수 연결이 비어 있어 플레이를 시작하지 않았습니다. 위의 에러를 클릭하면 해당 오브젝트가 선택됩니다.");
        }
    }

    [MenuItem("Tools/필수 연결 점검/지금 검사")]
    private static void CheckNow()
    {
        RefValidator.ValidateAll();
    }

    [MenuItem(StopMenu)]
    private static void ToggleStop()
    {
        StopPlay = !StopPlay;
    }

    [MenuItem(StopMenu, true)]
    private static bool ToggleStopValidate()
    {
        Menu.SetChecked(StopMenu, StopPlay);
        return true;
    }
}
#endif