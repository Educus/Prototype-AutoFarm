using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private Dictionary<string, AudioClip> BGM_Clips;
    private Dictionary<string, AudioClip> SFX_Clips;

    private float bgmVolume = 0.8f;
    private float sfxVolume = 0.8f;

    private AudioSource BGM;
    private AudioSource SFX;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        init();
    }

    private void Update()
    {
        // 게임이 플레이 중이 아닐 때는 BGM 재생하지 않음
        // 시간대에 따라 BGM 변경되게 업데이트 필요
        if (!GameManager.Instance.isPlay) return;
        PlayBGM(BGM_Clips["BGM_Main_Dawn_Loop"], BGM);
    }

    //awake 초기화 오류로 수정 필요.
    public void init()
    {
        Load_BGM();
        Load_SFX();
    }
    
    public void SetVolumes(float bgm, float sfx)
    {
        bgmVolume = Mathf.Clamp01(bgm);
        sfxVolume = Mathf.Clamp01(sfx);

        if (BGM != null) BGM.volume = bgmVolume;
        if (SFX != null) SFX.volume = sfxVolume;
    }

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

        BGM.volume = bgmVolume;
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

        SFX.volume = sfxVolume;
    }


    //BGM 재생 함수
    //파일명 입력
    public void PlayBGM(AudioClip bgm, AudioSource source)
    {
        if (BGM_Clips.TryGetValue(bgm.name, out AudioClip clip))
        {
            if (source.clip == clip && source.isPlaying) return;
            source.clip = clip;
            source.loop = true;
            source.volume = bgmVolume;
            source.Play();
        }
    }


    //SFX 재생 함수
    //상황에 따라 반복 재생 여부 파악

    //특정 오브젝트에서 재생할 경우
    public void PlaySFX(string name, AudioSource source, bool isloop)
    {
        if(isloop)
        {
            if (SFX_Clips.TryGetValue(name, out AudioClip clip))
            {
                if (source.clip == clip && source.isPlaying) return;
                source.clip = clip;
                source.loop = true;
                source.volume = sfxVolume;
                source.Play();
            }
        }
        else
        {
            if (SFX_Clips.TryGetValue(name, out AudioClip clip))
            {
                source.PlayOneShot(clip, sfxVolume);
            }
        }
    }

    //Manager에서 직접 재생할 경우
    public void PlaySFX(string name)
    {
        if (SFX_Clips == null)
        {
            Debug.Log("it is not init");
            return;
        }
        if (SFX_Clips.TryGetValue(name, out AudioClip clip))
        {
            if(clip == null)
            {
                Debug.Log("it is not init");
                return;
            }
            SFX.PlayOneShot(clip, sfxVolume);
        }
    }


    //반복 재생 중이던 SFX를 중지시키는 함수
    public void StopSFX(AudioSource source)
    {
        source.Stop();
    }
}
