using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

[DefaultExecutionOrder(-130)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private string defaultBgmName = "BGM_Main_Dawn_Loop";

    // 같은 소리 동시 호출 시 무시
    private const float MinPlayInterval = 0.05f;

    private Dictionary<string, AudioClip> BGM_Clips;
    private Dictionary<string, AudioClip> SFX_Clips;

    private float masterVilume = 0.8f;
    private float bgmVolume = 0.8f;
    private float sfxVolume = 0.8f;

    private AudioSource BGM;
    private AudioSource SFX;

    private readonly HashSet<string> warnedNames = new HashSet<string>();
    private readonly Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();

    private float BgmFinal => masterVilume * bgmVolume;
    private float SfxFinal => masterVilume * sfxVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        init();
    }

    private void Update()
    {
        // 게임이 플레이 중이 아닐 때는 BGM 재생하지 않음
        if (GameManager.Instance == null || !GameManager.Instance.isPlay) 
            return;

        if (BGM == null || BGM.isPlaying)
            return;

        if (BGM_Clips.TryGetValue(defaultBgmName, out AudioClip clip))
            PlayBGM(clip, BGM);
    }

    public void init()
    {
        Load_BGM();
        Load_SFX();
        ApplyVolumes();

        ValidateSfxNames();
    }

    // Instance 없이 호출 가능
    public static void Play(string name)
    {
        if (Instance != null)
            Instance.PlaySFX(name);
    }

    #region Volume
    public float MasterVolume => masterVilume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    public void SetVolumes(float master, float bgm, float sfx)
    {
        masterVilume = Mathf.Clamp01(master);
        bgmVolume = Mathf.Clamp01(bgm);
        sfxVolume = Mathf.Clamp01(sfx);

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (BGM != null) BGM.volume = BgmFinal;
        if (SFX != null) SFX.volume = SfxFinal;
    }
    #endregion

    #region Load
    private void Load_BGM()
    {
        AudioClip[] Clips = Resources.LoadAll<AudioClip>("Sound/BGM");
        BGM = transform.Find("BGM").GetComponent<AudioSource>();

        BGM_Clips = new Dictionary<string, AudioClip>();

        foreach (AudioClip Cl in Clips)
        {
            if (!BGM_Clips.ContainsKey(Cl.name))
            {
                BGM_Clips.Add(Cl.name, Cl);
            }
        }
    }
    private void Load_SFX()
    {
        AudioClip[] Clips = Resources.LoadAll<AudioClip>("Sound/SFX");
        SFX= transform.Find("SFX").GetComponent<AudioSource>();

        SFX_Clips = new Dictionary<string, AudioClip>();

        foreach (AudioClip Cl in Clips)
        {
            if (!SFX_Clips.ContainsKey(Cl.name))
            {
                SFX_Clips.Add(Cl.name, Cl);
            }
        }
    }

    private void ValidateSfxNames()
    {
        var registered = new HashSet<string>();

        FieldInfo[] fields = typeof(SFXName).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo field in fields)
        {
            if (!field.IsLiteral || field.FieldType != typeof(string))
                continue;

            string value = (string)field.GetRawConstantValue();
            registered.Add(value);

            if (!SFX_Clips.ContainsKey(value))
                Debug.LogWarning($"[SoundManager] SfxName.{field.Name} = \"{value}\" 에 해당하는 SFX 파일이 없습니다. (이름 오타 또는 파일 누락)");
        }

        foreach (string clipName in SFX_Clips.Keys)
        {
            if (!registered.Contains(clipName))
                Debug.LogWarning($"[SoundManager] SFX 파일 \"{clipName}\" 이(가) SfxName 에 등록되어 있지 않습니다.");
        }
    }
    #endregion

    //BGM 재생 함수
    //파일명 입력
    public void PlayBGM(AudioClip bgm, AudioSource source)
    {
        if (BGM_Clips.TryGetValue(bgm.name, out AudioClip clip))
        {
            if (source.clip == clip && source.isPlaying) 
                return;

            source.clip = clip;
            source.loop = true;
            source.volume = BgmFinal;
            source.Play();
        }
    }


    //SFX 재생 함수
    //상황에 따라 반복 재생 여부 파악

    //특정 오브젝트에서 재생할 경우
    public void PlaySFX(string name, AudioSource source, bool isloop)
    {
        if (source == null)
            return;

        if (!SFX_Clips.TryGetValue(name, out AudioClip clip))
            return;

        if (isloop)
        {
            if (source.clip == clip && source.isPlaying) return;
            source.clip = clip;
            source.loop = true;
            source.volume = SfxFinal;
            source.Play();
        }
        else
        {
            if (MinPlaying(name))
                return;

            source.PlayOneShot(clip, SfxFinal);
        }
    }

    //Manager에서 직접 재생할 경우
    public void PlaySFX(string name)
    {
        if (SFX == null || SFX_Clips == null)
            return;

        if (!SFX_Clips.TryGetValue(name, out AudioClip clip) || clip == null)
            return;

        if (MinPlaying(name))
            return;

        SFX.PlayOneShot(clip);
    }


    //반복 재생 중이던 SFX를 중지시키는 함수
    public void StopSFX(AudioSource source)
    {
        source.Stop();
    }

    private bool MinPlaying(string name)
    {
        float now = Time.unscaledTime;

        if (lastPlayTime.TryGetValue(name, out float last) && now - last < MinPlayInterval)
            return true;

        lastPlayTime[name] = now;
        return false;
    }
}
