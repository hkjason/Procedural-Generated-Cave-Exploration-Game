using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioGameSetting audioSetting;
    public AudioGameSetting AudioSetting
    {
        get
        {
            return audioSetting;
        }
        set
        {
            audioSetting = value;
            ApplyAudioSettings();
        }
    }
    [SerializeField] List<AudioSource> audioSources;
    [SerializeField] AudioSource looperAudioSource;

    [SerializeField] AudioSource bgmIntroAudioSource;
    [SerializeField] AudioSource bgmAudioSource;


    public enum AudioType
    {
        BGM,
        SFX,
        UI_SFX
    }

    [System.Serializable]
    public class BGM_SpecialHandler
    {
        public AudioClip BGM;
        public float endFadingTime = 5f;
    }

    [SerializeField] List<AudioClip> BGMs;
    [SerializeField] List<BGM_SpecialHandler> BGM_specialHandler;
    [SerializeField] List<AudioClip> sfxs;
    [SerializeField] List<AudioClip> UI_sfxs;

    //private parameters
    private Coroutine bgmTransitionCoroutine = null;

    //private void OnValidate()
    //{
    //    Button[] fooGroup = Resources.FindObjectsOfTypeAll<Button>();
    //print(fooGroup.Length);
    //    foreach (Button b in fooGroup)
    //    {
    //        if (b.GetComponent<MedalGameAudioPlayer>() == null)
    //        {
    //            b.gameObject.AddComponent<MedalGameAudioPlayer>();
    //        }
    //    }
    //}

#if UNITY_EDITOR
    private void OnValidate()
    {
        sfxs = FindAssetsByType<AudioClip>("Assets/Audio").ToList();
    }

    public static T[] FindAssetsByType<T>(params string[] folders) where T : Object
    {
        string type = typeof(T).ToString().Replace("UnityEngine.", "");

        string[] guids;
        if (folders == null || folders.Length == 0)
        {
            guids = AssetDatabase.FindAssets("t:" + type);
        }
        else
        {
            guids = AssetDatabase.FindAssets("t:" + type, folders);
        }

        T[] assets = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return assets;
    }
#endif

    void Awake()
    {
        /*
        if (instance == null)
            instance = this;
        else
            this.gameObject.SetActive(false);
        */

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void ApplyAudioSettings()
    {
        BgmVolumeAdjust(audioSetting.BGMVolume);
        //Adjust player sound
        if (Player.Instance != null)
        {
            Player.Instance.walkSound.volume = 0.5f * audioSetting.GetSFXValue();
            Player.Instance.runSound.volume = 0.5f * audioSetting.GetSFXValue();
            Player.Instance.jumpSound.volume = 0.5f * audioSetting.GetSFXValue();
        }

        //print("Apply audio settings");
    }

    private void Start()
    {
        ChangeBGM(0);
        ApplyAudioSettings();
    }

    public int GetAudioIDByName(AudioType type, string name)
    {
        switch (type)
        {
            case AudioType.BGM:
                for (int i = 0; i < BGMs.Count; i++)
                {
                    if (BGMs[i].name.Equals(name))
                    {
                        return i;
                    }
                }
                break;
            case AudioType.SFX:
                for (int i = 0; i < sfxs.Count; i++)
                {
                    if (sfxs[i].name.Equals(name))
                    {
                        return i;
                    }
                }
                break;
            case AudioType.UI_SFX:
                for (int i = 0; i < UI_sfxs.Count; i++)
                {
                    if (UI_sfxs[i].name.Equals(name))
                    {
                        return i;
                    }
                }
                break;
        }
        Debug.LogError("Put the correct sound effect under the correct audio folder please");
        return -1;
    }

    public void Play(string audioName, float volume = 1)
    {
        audioSources[0].volume = volume * audioSetting.sfxVolume;
        audioSources[0].clip = (sfxs.Find((s) => s.name == audioName));
        audioSources[0].Play();
    }


    //public void PlayNoOverlap(int audioIndex, int channel = 1, float volume = 1)
    //{
    //    if (!audioSources[channel].isPlaying)
    //    {
    //        audioSources[channel].volume = volume * audioSetting.sfxVolume;
    //        audioSources[channel].clip = sfxs[audioIndex];
    //        audioSources[channel].Play();
    //    }
    //}
    //public void PlayNoOverlap(string audioName, int channel = 1, float volume = 1)
    //{
    //    if (!audioSources[channel].isPlaying)
    //    {
    //        audioSources[channel].volume = volume * audioSetting.sfxVolume;
    //        audioSources[channel].clip = sfxs.Find((s) => s.name == audioName);
    //        audioSources[channel].Play();
    //    }
    //}

    public void PlayOnUnusedTrack(Vector3 loc, int audioIndex, float volume = 1)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == sfxs[audioIndex] && !audioSources[i].isPlaying)
            {
                audioSources[i].transform.position = loc;
                audioSources[i].volume = volume * audioSetting.sfxVolume;
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }
        }

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].transform.position = loc;
                audioSources[i].volume = volume * audioSetting.sfxVolume;
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].clip = sfxs[audioIndex];
                audioSources[i].Play();
                return;
            }

        }
    }
    public void PlayOnUnusedTrack(Vector3 loc, string audioName, float volume = 1)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == sfxs.Find((s) => s.name == audioName) && !audioSources[i].isPlaying)
            {
                audioSources[i].transform.position = loc;
                audioSources[i].volume = volume * audioSetting.sfxVolume;
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }
        }

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].transform.position = loc;
                audioSources[i].volume = volume * audioSetting.sfxVolume;
                audioSources[i].clip = sfxs.Find((s) => s.name == audioName);
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }

        }
    }

    public void PlayOnUnusedTrack_UI(int audioIndex, float volume = 1)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == UI_sfxs[audioIndex] && !audioSources[i].isPlaying)
            {
                audioSources[i].volume = volume * audioSetting.UIsfxVolume;
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }
        }

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].volume = volume * audioSetting.UIsfxVolume;
                audioSources[i].clip = UI_sfxs[audioIndex];
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }

        }

    }

    public void PlayOnUnusedTrack_UI(string audioName, float volume = 1)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == UI_sfxs.Find((s) => s.name == audioName) && !audioSources[i].isPlaying)
            {
                audioSources[i].volume = volume * audioSetting.UIsfxVolume;
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }
        }

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].volume = volume * audioSetting.UIsfxVolume;
                audioSources[i].clip = UI_sfxs.Find((s) => s.name == audioName);
                audioSources[i].pitch = Time.timeScale;
                audioSources[i].Play();
                return;
            }

        }

    }
    public void LoopSFX(int audioIndex, float volume = 1)
    {
        looperAudioSource.volume = volume * audioSetting.sfxVolume;
        looperAudioSource.loop = true;
        looperAudioSource.clip = sfxs[audioIndex];
        looperAudioSource.Play();
    }
    public void LoopSFX(string audioName, float volume = 1)
    {
        looperAudioSource.volume = volume * audioSetting.sfxVolume;
        looperAudioSource.loop = true;
        looperAudioSource.clip = sfxs.Find((s) => s.name == audioName);
        looperAudioSource.Play();
    }

    public void BgmVolumeAdjust(float volume)
    {
        bgmAudioSource.volume = audioSetting.BGMVolume;
        bgmIntroAudioSource.volume = audioSetting.BGMVolume;
    }

    //public int GetCurrentBGMIndex()
    //{

    //    return BGMs.FindIndex((b) => b.name.Equals(bgmAudioSource.clip?.name));
    //}

    public void PlayConnectedBGM(int audioIndex, int introAudioIndex)
    {
        print("play connected bgm");
        bgmIntroAudioSource.clip = BGMs[introAudioIndex];
        bgmIntroAudioSource.volume = audioSetting.BGMVolume;
        bgmIntroAudioSource.Play();
        bgmAudioSource.clip = BGMs[audioIndex];
        bgmAudioSource.PlayDelayed(bgmIntroAudioSource.clip.length);
    }

    public void ChangeBGM(int audioIndex)
    {
        if (bgmAudioSource.clip == BGMs[audioIndex] && bgmAudioSource.isPlaying) // if the same bgm is playing
        {
            print("Same bgm is playing already");
            return;
        }
        //else if (bgmAudioSource_Track1.clip == BGMs[audioIndex])                        // if the same bgm is playing but not in its volume
        //{
        //    print("same bgm is playing but not in its volume");
        //    StopCoroutine(bgmTransitionCoroutine);
        //    StartCoroutine( FadeInBGM(audioIndex,0.2f));
        //    bgmTransitionCoroutine = null;
        //}
        else
        {
            if (bgmIntroAudioSource.isPlaying)
            {
                StartCoroutine(FadeOutBGMIntro(0.5f));
            }
            if (bgmAudioSource.isPlaying && bgmTransitionCoroutine == null)     // if other bgm is playing
            {
                print("Change BGM to " + BGMs[audioIndex].name.ToString());
                bgmTransitionCoroutine = StartCoroutine(BGMTransition(audioIndex));
            }
            else if (bgmTransitionCoroutine != null)                            // if the last bgm transition running
            {
                print("BGM is changing too quickly, please check your bgm playing method");
                return;
            }
            else                                                                // if no bgm is playing
            {
                print("Player BGM " + BGMs[audioIndex].name.ToString());
                bgmAudioSource.volume = audioSetting.BGMVolume;
                bgmAudioSource.loop = true;
                bgmAudioSource.clip = BGMs[audioIndex];
                bgmAudioSource.Play();
            }
        }
    }
    public void ForcePlayLastSecondOfBGM(int audioIndex)
    {
        bgmAudioSource.clip = BGMs[audioIndex];
        bgmAudioSource.time = BGMs[audioIndex].length - 0.01f;
        bgmAudioSource.Play();
        StartCoroutine(FadeOutBGMIntro(0.2f));
    }

    private IEnumerator BGMTransition(int audioIndex, float fadeTime = 0.75f)
    {
        bgmTransitionCoroutine = StartCoroutine(FadeOutBGM(fadeTime));

        yield return bgmTransitionCoroutine;

        bgmTransitionCoroutine = StartCoroutine(FadeInBGM(audioIndex, fadeTime));
        yield return bgmTransitionCoroutine;
        bgmTransitionCoroutine = null;
    }


    public void StopBGM(float fadeTime = 1.5f)
    {
        StartCoroutine(FadeOutBGM(fadeTime));
    }

    private IEnumerator FadeInBGM(int audioIndex, float FadeTime)
    {
        float targetVolume = audioSetting.BGMVolume;
        bgmAudioSource.volume = 0;
        bgmAudioSource.loop = true;
        bgmAudioSource.clip = BGMs[audioIndex];
        bgmAudioSource.Play();
        while (bgmAudioSource.volume < targetVolume)
        {
            bgmAudioSource.volume += targetVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        bgmAudioSource.volume = targetVolume;
    }
    private IEnumerator FadeOutBGM(float FadeTime)
    {
        float startVolume = bgmAudioSource.volume;

        while (bgmAudioSource.volume > 0)
        {
            bgmAudioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        bgmAudioSource.Stop();
        bgmAudioSource.volume = startVolume;
    }
    private IEnumerator FadeOutBGMIntro(float FadeTime)
    {
        float startVolume = bgmIntroAudioSource.volume;

        while (bgmIntroAudioSource.volume > 0)
        {
            bgmIntroAudioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        bgmIntroAudioSource.Stop();
        bgmIntroAudioSource.volume = startVolume;
    }


    public void StopLooperSFX()
    {
        looperAudioSource.Pause();
        looperAudioSource.loop = false;


    }

}
[System.Serializable]
public class AudioGameSetting
{
    [SerializeField]float _masterVolume = 0.8f;
    public float masterVolume { get { return _masterVolume; }  set { _masterVolume = value; AudioManager.instance.ApplyAudioSettings(); } }
    [SerializeField] float _BGMVolume = 0.3f;
    public float BGMVolume { get { return _BGMVolume * masterVolume; } set { _BGMVolume = value; AudioManager.instance.ApplyAudioSettings(); }  }
    [SerializeField] float _sfxVolume = 0.5f;
    public float sfxVolume { get { return _sfxVolume * masterVolume; } set { _sfxVolume = value; AudioManager.instance.ApplyAudioSettings(); } }
    [SerializeField] float _UIsfxVolume = 0.5f;
    public float UIsfxVolume { get { return _UIsfxVolume * masterVolume; } set { _UIsfxVolume = value; AudioManager.instance.ApplyAudioSettings(); } }

    public float GetBGMValue()
    {
        return _BGMVolume;
    }

    public float GetSFXValue()
    {
        return _sfxVolume;
    }
}