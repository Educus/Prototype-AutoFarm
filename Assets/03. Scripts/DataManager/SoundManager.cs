using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Dictionary<string, AudioClip> BGM_Clips;
    private Dictionary<string, AudioClip> SFX_Clips;

    private AudioSource BGM;
    private AudioSource SFX;

    private void Awake()
    {
        init();

        //PlayBGM("BGM_Main_Dawn_Loop", BGM);
        //PlaySFX("SFX_GUI_Button", SFX);
    }

    private void init()
    {
        Load_BGM();
        Load_SFX();
    }

    private void Load_BGM()
    {
        AudioClip[] Clips = Resources.LoadAll<AudioClip>("Sound/BGM");
        BGM = GetComponent<AudioSource>();

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
        SFX= GetComponent<AudioSource>();

        SFX_Clips = new Dictionary<string, AudioClip>();

        foreach (AudioClip Cl in Clips)
        {
            if (!SFX_Clips.ContainsKey(Cl.name))
            {
                SFX_Clips.Add(Cl.name, Cl);
            }
        }
    }

    //BGM 재생 함수
    //파일명 입력
    public void PlayBGM(string name, AudioSource source)
    {

        if (BGM_Clips.TryGetValue(name, out AudioClip clip))
        {
            if (source.clip == clip && source.isPlaying) return;
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
    }


    //SFX 재생 함수
    //SFX는 1회만 재생
    public void PlaySFX(string name, AudioSource source)
    {
        if (SFX_Clips.TryGetValue(name, out AudioClip clip))
        {
            source.PlayOneShot(clip);
        }
    }
}
