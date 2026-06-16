using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Dictionary<string, AudioClip> BGM_Clips;
    private Dictionary<string, AudioClip> SFX_Clips;

    private void Awake()
    {
        init();
    }

    private void init()
    {
        Load_BGM();
        Load_SFX();
    }

    private void Load_BGM()
    {
        AudioClip[] Clips = Resources.LoadAll<AudioClip>("Sound/BGM");
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
        AudioClip[] Clips = Resources.LoadAll<AudioClip>("Sound/BGM");
        BGM_Clips = new Dictionary<string, AudioClip>();

        foreach (AudioClip Cl in Clips)
        {
            if (!SFX_Clips.ContainsKey(Cl.name))
            {
                SFX_Clips.Add(Cl.name, Cl);
            }
        }
    }
}
