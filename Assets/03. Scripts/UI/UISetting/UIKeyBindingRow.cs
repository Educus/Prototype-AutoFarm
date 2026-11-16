using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyBindingRow : MonoBehaviour
{
    [RequiredRef, SerializeField] private TMP_Text label;
    [RequiredRef, SerializeField] private Button keyButton;
    [RequiredRef, SerializeField] private TMP_Text keyText;

    [SerializeField] private string inputPrompt = "Input Key";
    [SerializeField] private float messageSeconds = 1.2f;
    [SerializeField] private Color errorColor = new Color(1f, 0.25f, 0.25f);

    private string actionName;
    private bool rebindable;
    private bool capturing;
    private Coroutine restoreRoutine;

    private UIKeyBindingPanel panel;

    private PlayerDataManager Player =>
        DataManager.Instance.PlayerDataManager;

    public string ActionName => actionName;

    public void Setup(UIKeyBindingPanel owner, KeyCodeData data, bool canRebind)
    {
        panel = owner;
        actionName = data.Name;
        rebindable = canRebind;

        label.text = string.IsNullOrEmpty(data.Description_KR) ? data.Name : data.Description_KR;

        keyButton.onClick.RemoveAllListeners();
        keyButton.onClick.AddListener(StartCapture);
        keyButton.interactable = canRebind;

        Refresh();
    }

    public void Refresh()
    {
        if (!capturing)
            keyText.text = KeyNames.Format(Player.GetKey(actionName));
    }

    private void StartCapture()
    {
        if (!rebindable || capturing)
            return;

        if (!panel.TryBeginCapture(this))
            return;

        capturing = true;
        Player.BeginKeyCapture();
        keyText.text = inputPrompt;
    }

    private void Update()
    {
        if (!capturing)
            return;

        // 마우스 클릭 취소
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            CancelQuietly();
            return;
        }

        if (!Input.anyKeyDown)
            return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key))
                continue;

            if (key == KeyCode.Escape)
            {
                CancelQuietly();
                return;
            }

            RebindResult result = Player.TryRebind(actionName, key, out string conflictAction);

            if (result == RebindResult.Success)
            {
                Finish();
                return;
            }

            Fail(key, result == RebindResult.Conflict ? conflictAction : null);

            return;
        }
    }

    private void Finish()
    {
        EndCapture();
        keyText.text = KeyNames.Format(Player.GetKey(actionName));
    }

    // 취소, 오류X
    private void CancelQuietly()
    {
        EndCapture();
        keyText.text = KeyNames.Format(Player.GetKey(actionName));
    }

    // 취소, 오류O
    private void Fail(KeyCode pressed, string conflictAction)
    {
        EndCapture();

        ShowErrorKey(pressed);

        if (!string.IsNullOrEmpty(conflictAction))
            panel.ShowErrorKey(conflictAction, pressed);
    }

    public void ShowErrorKey(KeyCode key)
    {
        if (!isActiveAndEnabled)
            return;

        StopRestore();

        string hex = ColorUtility.ToHtmlStringRGB(errorColor);
        keyText.text = $"<color=#{hex}>{KeyNames.Format(key)}</color>";

        restoreRoutine = StartCoroutine(ReleaseCaptureNextFrame());
    }

    private void EndCapture()
    {
        capturing = false;
        panel.EndCapture(this);

        StartCoroutine(ReleaseCaptureNextFrame());
    }

    // 같은 프레임에 누른 키가 입력으로 처리X
    private IEnumerator ReleaseCaptureNextFrame()
    {
        yield return null;
        Player.EndKeyCapture();
    }

    private IEnumerator RestoreLater()
    {
        // 일시정지 때 동작하도록
        yield return new WaitForSecondsRealtime(messageSeconds);

        restoreRoutine = null;

        if (!capturing)
            keyText.text = KeyNames.Format(Player.GetKey(actionName));
    }

    private void StopRestore()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }
    }

    private void OnDisable()
    {
        restoreRoutine = null;

        if (capturing)
        {
            capturing = false;
            panel.EndCapture(this);
            Player.EndKeyCapture();
        }
    }
}
