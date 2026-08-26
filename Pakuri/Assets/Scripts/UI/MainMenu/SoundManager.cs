/*
 * 역할: 전역 UI 오디오 재생.
 * 책임: 씬별 BGM과 공용 UI Button SFX를 씬 전환 중에도 유지한다.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SoundManager : MonoBehaviour
{
    private const string InGameSceneName = "InGameScene";

    private static SoundManager instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioClip inGameBgm;
    private AudioClip stageTwoBgm;
    private AudioClip uiButtonClickSfx;

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

        EnsureAudioSources();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        instance = null;
    }

    public void Configure(AudioClip newInGameBgm, AudioClip newStageTwoBgm, AudioClip newUiButtonClickSfx)
    {
        inGameBgm = newInGameBgm;
        stageTwoBgm = newStageTwoBgm;
        uiButtonClickSfx = newUiButtonClickSfx;
        EnsureAudioSources();
        BindUiButtons(SceneManager.GetActiveScene());
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == InGameSceneName)
        {
            PlayBgm(inGameBgm);
        }

        StartCoroutine(BindUiButtonsAfterSceneStart(scene));
    }

    private IEnumerator BindUiButtonsAfterSceneStart(Scene scene)
    {
        BindUiButtons(scene);
        yield return null;
        BindUiButtons(scene);
    }

    private void BindUiButtons(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null || button.gameObject.scene != scene)
            {
                continue;
            }

            var clickSound = button.GetComponent<UiButtonClickSound>()
                ?? button.gameObject.AddComponent<UiButtonClickSound>();
            clickSound.Initialize(this, button);
        }
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("SoundManager cannot play BGM because the AudioClip is null.", this);
            return;
        }

        EnsureAudioSources();

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayStageTwoBgm()
    {
        PlayBgm(stageTwoBgm);
    }

    internal void PlayUiButtonClick()
    {
        if (uiButtonClickSfx == null)
        {
            return;
        }

        EnsureAudioSources();
        sfxSource.PlayOneShot(uiButtonClickSfx);
    }
}
