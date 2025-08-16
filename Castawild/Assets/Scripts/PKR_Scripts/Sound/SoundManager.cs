using UnityEngine;
using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum Sound
{
    Env_Start = 100,
    Env_Bush,
    Env_Coast,
    Env_Day,
    Env_DeepWater,
    Env_Fire,
    Env_InvenOpen,
    Env_MorningBird,
    Env_MorningBird2,
    Env_Night,
    Env_NightBird,
    Env_NightWolf,
    Env_Rain,
    Env_Thunder,
    Env_Title,
    Env_WaterFall,

    Mon_Start = 200,
    Mon_Bear_Cry1,
    Mon_Bear_Cry2,
    Mon_Bear_Cry3,
    Mon_Damaged,
    Mon_Rabbit_Damaged,
    Mon_Rabbit_Dead,
    Mon_Rabbit_Run,

    Player_Start = 300,
    Player_Attack,
    Player_Damaged1,
    Player_Damaged2,
    Player_Damaged3,
    Player_Dead,
    Player_Drink,
    Player_Eat,
    Player_HeartBeat,
    Player_Jump,
    Player_Revive,
    Player_Run,
    Player_Shoot,
    Player_Sleep3,
    Player_Sleep4,
    Player_Walk1,
    Player_Walk2,
    Player_Walk3,
    Player_Walk4,
    Player_Walk5,
    Player_Walk6,
    Player_Walk7,

    UI_Start = 400,
    UI_ButtonClick,
    UI_ItemDrop,
    UI_Scroll,

    Weapon_Start = 500,
    Weapon_Arrow1,
    Weapon_Arrow2,
    Weapon_Arrow3,
    Weapon_Arrow4,
}

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioSource> sfxSources;
    private Dictionary<Sound, AudioClip> _audioClips = new Dictionary<Sound, AudioClip>();


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    public override void Spawned()
    {
        if (Instance != this)
        {
            Runner.Despawn(Object);
        }
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
            if (Enum.TryParse(clip.name, out Sound sfx))
            {
                _audioClips.Add(sfx, clip);
            }
        }

        bgmSource.loop = true;
        sfxSources.ForEach(x => x.loop = false);
        bgmSource.playOnAwake = false;
        sfxSources.ForEach(x => x.playOnAwake = false);
    }


    #region BGM

    public void PlayBGM(Sound sfx, float volume = 0.2f)
    {
        if (_bgmCo != null)
        {
            StopCoroutine(_bgmCo);
        }

        _bgmCo = StartCoroutine(BgmFadeInOut(sfx, volume));
    }

    public void StopBGM()
    {
        if (_bgmCo != null)
        {
            StopCoroutine(_bgmCo);
        }
        _bgmCo = StartCoroutine(BgmFadeOut());
    }


    private IEnumerator BgmFadeOut()
    {
        //볼륨을 서서히 줄이기.
        float _fadeSpeed = 1f;
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= Time.deltaTime * _fadeSpeed;
            yield return null;
        }

        bgmSource.Stop();
    }
    private Coroutine _bgmCo = null;

    private IEnumerator BgmFadeInOut(Sound sfx, float volume)
    {
        //볼륨을 서서히 줄이기.
        float _fadeSpeed = 0.5f;
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= Time.deltaTime * _fadeSpeed;
            yield return null;
        }

        bgmSource.Stop();
        if (_audioClips.ContainsKey(sfx) == false)
        {
            yield break;
        }

        bgmSource.clip = _audioClips[sfx];
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

    public void PlayLocalSound2D(PlayerRef target, Sound sfx, float volume = 1f)
    {
        // Runner가 실행 중(네트워크 연결 상태)일 때만 RPC를 호출
        if (Runner != null && Runner.IsRunning)
        {
            RPC_RequestPlaySound(PlayerRef.None, sfx, Vector3.zero, false, volume);
        }
        else // 아니라면 (ex. 타이틀 씬) 그냥 로컬에서 재생
        {
            PlaySoundInternal(sfx, Vector3.zero, false, volume);
        }
    }
    public void PlayLocalSound3D(PlayerRef target, Sound sfx, Vector3 position, float volume = 1f)
    {
        if (Runner != null && Runner.IsRunning)
        {
            RPC_RequestPlaySound(target, sfx, position, true, volume);
        }
        else
        {
            PlaySoundInternal(sfx, Vector3.zero, false, volume);
        }
    }

    public void PlayGlobalSound2D(Sound sfx, float volume = 1f)
    {
        if (Runner != null && Runner.IsRunning)
        {
            RPC_RequestPlaySound(PlayerRef.None, sfx, Vector3.zero, false, volume);
        }
        else
        {
            PlaySoundInternal(sfx, Vector3.zero, false, volume);
        }

    }

    public void PlayGlobalSound3D(Sound sfx, Vector3 position, float volume = 1f)
    {
        if (Runner != null && Runner.IsRunning)
        {
            RPC_RequestPlaySound(PlayerRef.None, sfx, position, true, volume);
        }
        else
        {
            PlaySoundInternal(sfx, Vector3.zero, false, volume);
        }
    }

    #endregion

    #region RPC Methods

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPlaySound(PlayerRef target, Sound sfx, Vector3 position, bool is3D, float volume)
    {
        RPC_BroadcastPlaySound(target, sfx, position, is3D, volume);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastPlaySound(PlayerRef target, Sound sfx, Vector3 position, bool is3D, float volume)
    {
        if (target == PlayerRef.None || target == Runner.LocalPlayer)
        {
            PlaySoundInternal(sfx, position, is3D, volume);
        }
    }

    #endregion

    #region Internal Logic

    public void PlaySoundInternal(Sound sfx, Vector3 position, bool is3D, float volume)
    {
        var clip = _audioClips[sfx];
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