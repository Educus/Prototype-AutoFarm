using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class UIScaleApplier : MonoBehaviour
{
    private CanvasScaler scaler;
    private Vector2 baseReference;
    private float baseScaleFactor;
    private bool cached;
    private bool subscribed;

    private void Cache()
    {
        if (cached)
            return;

        scaler = GetComponent<CanvasScaler>();
        baseReference = scaler.referenceResolution;
        baseScaleFactor = scaler.scaleFactor;
        cached = true;
    }

    private void OnEnable()
    {
        Cache();
        Subscribe();

        if (SettingManager.Instance != null)
            Apply(SettingManager.Instance.Data.uiScale);
    }

    private void Start()
    {
        Subscribe();   // OnEnable 때 SettingManager 가 없었던 경우 대비
    }

    private void OnDisable()
    {
        if (subscribed && SettingManager.Instance != null)
            SettingManager.Instance.OnUIScaleChanged -= Apply;

        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed || SettingManager.Instance == null)
            return;

        SettingManager.Instance.OnUIScaleChanged += Apply;
        subscribed = true;
    }

    private void Apply(float scale)
    {
        Cache();

        switch (scaler.uiScaleMode)
        {
            case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                scaler.referenceResolution = baseReference / scale;
                break;

            case CanvasScaler.ScaleMode.ConstantPixelSize:
                scaler.scaleFactor = baseScaleFactor * scale;
                break;
        }
    }
}
