using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyBindingRow : MonoBehaviour
{
    [RequiredRef, SerializeField] private TMP_Text label;
    [RequiredRef, SerializeField] private Button keyButton;
    [RequiredRef, SerializeField] private TMP_Text keyText;

    [SerializeField] private float messageSeconds = 1.2f;

    private string actionName;
    private bool rebindable;
    private bool capturing;

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
        keyText.text = "Input Key";
    }

    private void Update()
    {
        if (!capturing)
            return;

        // 마우스 클릭 취소
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            Cancel("Cancel");
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
                Cancel("Cancel");
                return;
            }

            RebindResult result = Player.TryRebind(actionName, key, out string conflict);

            switch (result)
            {
                case RebindResult.Success:
                    Finish();
                    break;
                case RebindResult.Conflict:
                    Cancel($"이미 사용 중인 키입니다.");
                    break;
                case RebindResult.InvalidKey:
                    Cancel($"사용 할 수 없는 키입니다.");
                    break;
                default:
                    Cancel("변경할 수 없습니다.");
                    break;
            }

            return;
        }
    }

    private void Finish()
    {
        capturing = false;
        panel.EndCapture(this);

        keyText.text = KeyNames.Format(Player.GetKey(actionName));

        StartCoroutine(ReleaseCaptureNextFrame());
    }

    // 취소 : 안내문구 보여주고 원래 키로 변경
    private void Cancel(string message)
    {
        capturing = false;
        panel.EndCapture(this);

        keyText.text = message;

        StartCoroutine(ReleaseCaptureNextFrame());
        StartCoroutine(RestoreTextLater());
    }

    // 같은 프레임에 누른 키가 입력으로 처리X
    private IEnumerator ReleaseCaptureNextFrame()
    {
        yield return null;
        Player.EndKeyCapture();
    }

    private IEnumerator RestoreTextLater()
    {
        // 일시정지 때 동작하도록
        yield return new WaitForSecondsRealtime(messageSeconds);

        if (!capturing)
            keyText.text = KeyNames.Format(Player.GetKey(actionName));
    }

    private void OnDisable()
    {
        if (capturing)
        {
            capturing = false;
            panel.EndCapture(this);
            Player.EndKeyCapture();
        }
    }
}
