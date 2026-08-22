/*
 * 역할: 전역 BGM 재생.
 * 책임: 하나의 AudioSource로 반복 BGM을 재생하고 씬 전환 중에도 유지한다.
 */

using UnityEngine;

public sealed class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    private AudioSource bgmSource;

    public static SoundManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            var soundManagerObject = new GameObject(nameof(SoundManager));
            instance = soundManagerObject.AddComponent<SoundManager>();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("SoundManager cannot play BGM because the AudioClip is null.", this);
            return;
        }

        if (bgmSource == null)
        {
            Awake();
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
}
