using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;


public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioSource> sfxSources;

    // 로드된 오디오 클립들을 저장하는 딕셔너리
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();


    public override void Spawned()
    {
        if (Instance != null)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Init()
    {
        var clips = Resources.LoadAll<AudioClip>("Sound");
        foreach (AudioClip clip in clips)
        {
            _audioClips.Add(clip.name, clip);
        }

        bgmSource.loop = true;
        sfxSources.ForEach(x => x.loop = false);
        bgmSource.playOnAwake = false;
        sfxSources.ForEach(x => x.playOnAwake = false);
    }
    

    #region BGM

    public void PlayBGM(string name, float volume = 0.2f)
    {
        if (_bgmCo != null)
        {
            StopCoroutine(_bgmCo);
        }

        _bgmCo = StartCoroutine(BgmFadeInOut(name, volume));
    }


    private Coroutine _bgmCo = null;

    private IEnumerator BgmFadeInOut(string name, float volume)
    {
        //볼륨을 서서히 줄이기.
        float _fadeSpeed = 0.5f;
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= Time.deltaTime * _fadeSpeed;
            yield return null;
        }

        bgmSource.Stop();
        if (_audioClips.ContainsKey(name) == false)
        {
            yield break;
        }
        bgmSource.clip = _audioClips[name];
        bgmSource.Play();
        
        //볼륨 서서히 높이기.
        while (bgmSource.volume < volume)
        {
            bgmSource.volume += Time.deltaTime * _fadeSpeed;
            yield return null;
        }

        bgmSource.volume = volume;
    }

    #endregion

    #region Effect

    public void PlayLocalSound2D(string name, float volume = 1f)
    {
        PlaySoundInternal(name, Vector3.zero, false, volume);
    }

    public void PlayLocalSound3D(string name, Vector3 position, float volume = 1f)
    {
        PlaySoundInternal(name, position, true, volume);
    }

    public void PlayGlobalSound2D(string name, float volume = 1f)
    {
        RPC_RequestPlaySound(name, Vector3.zero, false, volume);
    }
    public void PlayGlobalSound3D(string name, Vector3 position, float volume = 1f)
    {
        RPC_RequestPlaySound(name, position, true, volume);
    }

    #endregion

    #region RPC Methods

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPlaySound(string name, Vector3 position, bool is3D, float volume)
    {
        RPC_BroadcastPlaySound(name, position, is3D, volume);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastPlaySound(string name, Vector3 position, bool is3D, float volume)
    {
        PlaySoundInternal(name, position, is3D, volume);
    }

    #endregion

    #region Internal Logic

    private void PlaySoundInternal(string name, Vector3 position, bool is3D, float volume)
    {
        var clip = _audioClips[name];
        AudioSource source = GetAvailableSfxSource();
        if (source == null) return;

        source.transform.position = position;
        source.spatialBlend = is3D ? 1.0f : 0.0f;

        source.PlayOneShot(clip, volume);
    }

    private AudioSource GetAvailableSfxSource()
    {
        if (sfxSources == null || sfxSources.Count == 0)
        {
            Debug.LogError("SFX AudioSource가 할당되지 않았습니다!");
            return null;
        }

        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        return null;
    }

    #endregion
}

