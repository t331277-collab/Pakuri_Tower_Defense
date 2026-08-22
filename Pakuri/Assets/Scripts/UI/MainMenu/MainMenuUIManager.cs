/*
 * 역할: Main Menu 이동.
 * 책임: Menu 씬 참조와 Button을 연결하고 몬스터 선택 및 Run 시작을 처리한다.
 */

using System.Collections;
using Pakuri.InGame;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuUIManager : MonoBehaviour
{
    private const string NewRunScenePath = "Assets/Scenes/NewScene/InGameScene.unity";
    private const string DefaultMonsterName = "eve";
    private const float SummaryFadeDuration = 0.5f;
    private const float GameStartFadeDuration = 1f;
    private const int IntroVideoWidth = 1920;
    private const int IntroVideoHeight = 1080;

    [SerializeField] private AudioClip mainMenuBgm;
    [SerializeField] private VideoClip introVideoClip;
    [SerializeField] private VideoClip loopVideoClip;

    private GameObject introPanel;
    private GameObject mainMenuPanel;
    private GameObject monsterSelectPanel;
    private GameObject summaryPanel;
    private GameObject gameStartPanel;
    private GameObject monsterStanding;
    private Button introGameStartButton;
    private Button runButton;
    private Button monsterSelectGameStartButton;
    private Button arielButton;
    private Button eveButton;
    private Button seinButton;
    private Button vegaButton;
    private Button rinButton;
    private Image monsterStandingImage;
    private TextMeshProUGUI monsterNameText;
    private TextMeshProUGUI monsterDescriptionText;
    private CanvasGroup summaryCanvasGroup;
    private CanvasGroup gameStartCanvasGroup;
    private VideoPlayer introVideoPlayer;
    private VideoPlayer loopVideoPlayer;
    private RawImage introVideoSurface;
    private RenderTexture introVideoTexture;
    private RenderTexture loopVideoTexture;
    private Coroutine introSequence;

    private string selectedMonsterName;
    private bool isLoadingRunScene;
    private bool isLoopVideoPending;

    /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
    private void Start()
    {
        if (!BindObject())
        {
            return;
        }

        BindButtons();
        ShowIntro();
        StartMainMenuBgm();
        StartIntroSequence();
    }

    /// MainMenuScene의 패널과 버튼을 계층 경로로 찾아 런타임 참조를 구성한다.
    private bool BindObject()
    {
        var canvas = FindSceneRoot("Canvas");
        if (canvas == null)
        {
            Debug.LogError(
                "MainMenuUIManager BindObject failed: scene root 'Canvas' was not found.",
                this);
            return false;
        }

        var valid = true;
        introPanel = FindGameObject(canvas, "Intro", nameof(introPanel), ref valid);
        mainMenuPanel = FindGameObject(canvas, "MainMenuUI", nameof(mainMenuPanel), ref valid);
        monsterSelectPanel = FindGameObject(canvas, "MosterSelectUI", nameof(monsterSelectPanel), ref valid);
        summaryPanel = FindGameObject(canvas, "Intro/Summary", nameof(summaryPanel), ref valid);
        gameStartPanel = FindGameObject(canvas, "Intro/GameStart", nameof(gameStartPanel), ref valid);
        monsterStanding = FindGameObject(canvas, "MosterSelectUI/MonsterStanding", nameof(monsterStanding), ref valid);

        introGameStartButton = FindComponent<Button>(canvas, "Intro/GameStart", nameof(introGameStartButton), ref valid);
        runButton = FindComponent<Button>(canvas, "MainMenuUI/RunBtn", nameof(runButton), ref valid);
        monsterSelectGameStartButton = FindComponent<Button>(canvas, "MosterSelectUI/GameStart", nameof(monsterSelectGameStartButton), ref valid);
        monsterStandingImage = FindComponent<Image>(canvas, "MosterSelectUI/MonsterStanding", nameof(monsterStandingImage), ref valid);
        monsterNameText = FindComponent<TextMeshProUGUI>(canvas, "MosterSelectUI/MonsterStanding/Name", nameof(monsterNameText), ref valid);
        monsterDescriptionText = FindComponent<TextMeshProUGUI>(canvas, "MosterSelectUI/MonsterStanding/Desc", nameof(monsterDescriptionText), ref valid);
        arielButton = FindComponent<Button>(canvas, "MosterSelectUI/Ariel", nameof(arielButton), ref valid);
        eveButton = FindComponent<Button>(canvas, "MosterSelectUI/Eve", nameof(eveButton), ref valid);
        seinButton = FindComponent<Button>(canvas, "MosterSelectUI/Sein", nameof(seinButton), ref valid);
        vegaButton = FindComponent<Button>(canvas, "MosterSelectUI/Vega", nameof(vegaButton), ref valid);
        rinButton = FindComponent<Button>(canvas, "MosterSelectUI/Rin", nameof(rinButton), ref valid);
        if (valid)
        {
            summaryCanvasGroup = GetOrAddCanvasGroup(summaryPanel);
            gameStartCanvasGroup = GetOrAddCanvasGroup(gameStartPanel);
        }

        return valid;
    }

    /// MainMenuScene 진입 시 지정된 BGM을 전역 사운드 매니저로 반복 재생한다.
    private void StartMainMenuBgm()
    {
        SoundManager.Instance.PlayBgm(mainMenuBgm);
    }

    /// Intro 영상 재생과 Summary·GameStart 순차 페이드 인을 시작한다.
    private void StartIntroSequence()
    {
        if (introSequence != null)
        {
            StopCoroutine(introSequence);
        }

        introSequence = StartCoroutine(PlayIntroSequence());
    }

    /// Intro 영상과 두 UI 그룹의 순차 페이드 인을 처리한다.
    private IEnumerator PlayIntroSequence()
    {
        PrepareFadeTargets();
        StartIntroVideo();

        yield return FadeCanvasGroup(summaryCanvasGroup, SummaryFadeDuration, false);
        yield return FadeCanvasGroup(gameStartCanvasGroup, GameStartFadeDuration, true);

        if (introGameStartButton != null)
        {
            introGameStartButton.interactable = true;
        }

        introSequence = null;
    }

    /// Summary와 GameStart를 투명·비상호작용 상태로 초기화한다.
    private void PrepareFadeTargets()
    {
        SetCanvasGroupState(summaryCanvasGroup, 0f, false);
        SetCanvasGroupState(gameStartCanvasGroup, 0f, false);

        if (introGameStartButton != null)
        {
            introGameStartButton.interactable = false;
        }
    }

    /// CanvasGroup 하나를 목표 알파까지 지정 시간 동안 페이드 인한다.
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float duration, bool enableInteraction)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        SetCanvasGroupState(canvasGroup, 1f, enableInteraction);
    }

    /// CanvasGroup 알파와 상호작용 가능 여부를 설정한다.
    private static void SetCanvasGroupState(CanvasGroup canvasGroup, float alpha, bool enableInteraction)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = enableInteraction;
        canvasGroup.blocksRaycasts = enableInteraction;
    }

    /// 기존 CanvasGroup을 재사용하거나 대상 UI에 하나를 추가한다.
    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        return target.GetComponent<CanvasGroup>() ?? target.AddComponent<CanvasGroup>();
    }

    /// Intro 하위 UI 표면과 VideoPlayer를 구성하고 첫 영상을 재생한다.
    private void StartIntroVideo()
    {
        if (introPanel == null || introVideoClip == null || loopVideoClip == null)
        {
            Debug.LogError("MainMenuUIManager cannot start Intro video because Intro or VideoClip references are missing.", this);
            return;
        }

        EnsureIntroVideoSurface();
        introVideoPlayer.clip = introVideoClip;
        introVideoPlayer.isLooping = false;
        loopVideoPlayer.clip = loopVideoClip;
        loopVideoPlayer.isLooping = true;
        introVideoSurface.texture = introVideoTexture;
        isLoopVideoPending = false;

        introVideoPlayer.loopPointReached -= HandleIntroVideoFinished;
        introVideoPlayer.loopPointReached += HandleIntroVideoFinished;
        introVideoPlayer.Prepare();
        loopVideoPlayer.Prepare();
    }

    /// Intro 패널에 준비 재생용 VideoPlayer 두 개와 전체 화면 RawImage를 한 번 구성한다.
    private void EnsureIntroVideoSurface()
    {
        if (introVideoTexture == null)
        {
            introVideoTexture = CreateVideoTexture("MainMenuIntroVideoTexture");
        }

        if (loopVideoTexture == null)
        {
            loopVideoTexture = CreateVideoTexture("MainMenuLoopVideoTexture");
        }

        if (introVideoPlayer == null)
        {
            introVideoPlayer = introPanel.GetComponent<VideoPlayer>();
            if (introVideoPlayer == null)
            {
                introVideoPlayer = introPanel.AddComponent<VideoPlayer>();
            }

            ConfigureVideoPlayer(introVideoPlayer, introVideoTexture);
            introVideoPlayer.prepareCompleted += HandleIntroVideoPrepared;
            introVideoPlayer.errorReceived += HandleVideoError;
        }

        if (loopVideoPlayer == null)
        {
            loopVideoPlayer = introPanel.AddComponent<VideoPlayer>();
            ConfigureVideoPlayer(loopVideoPlayer, loopVideoTexture);
            loopVideoPlayer.prepareCompleted += HandleLoopVideoPrepared;
            loopVideoPlayer.errorReceived += HandleVideoError;
        }

        if (introVideoSurface == null)
        {
            var surfaceObject = new GameObject("IntroVideoSurface", typeof(RectTransform), typeof(RawImage));
            surfaceObject.transform.SetParent(introPanel.transform, false);
            surfaceObject.transform.SetAsFirstSibling();
            var surfaceRect = surfaceObject.GetComponent<RectTransform>();
            surfaceRect.anchorMin = Vector2.zero;
            surfaceRect.anchorMax = Vector2.one;
            surfaceRect.offsetMin = Vector2.zero;
            surfaceRect.offsetMax = Vector2.zero;
            introVideoSurface = surfaceObject.GetComponent<RawImage>();
            introVideoSurface.raycastTarget = false;
        }

        introVideoSurface.texture = introVideoTexture;
    }

    /// VideoPlayer 공통 설정을 무음·프레임 보존·RenderTexture 출력으로 구성한다.
    private static void ConfigureVideoPlayer(VideoPlayer player, RenderTexture targetTexture)
    {
        player.playOnAwake = false;
        player.skipOnDrop = false;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.targetTexture = targetTexture;
    }

    /// 1920x1080 영상 출력용 RenderTexture를 만든다.
    private static RenderTexture CreateVideoTexture(string textureName)
    {
        var texture = new RenderTexture(IntroVideoWidth, IntroVideoHeight, 0, RenderTextureFormat.ARGB32)
        {
            name = textureName
        };
        texture.Create();
        return texture;
    }

    /// 준비가 끝난 BG1을 재생한다.
    private void HandleIntroVideoPrepared(VideoPlayer source)
    {
        if (source == introVideoPlayer)
        {
            source.Play();
        }
    }

    /// BG1 종료 후 대기 중이면 준비된 BG2를 재생한다.
    private void HandleLoopVideoPrepared(VideoPlayer source)
    {
        if (source == loopVideoPlayer && isLoopVideoPending)
        {
            StartLoopVideo();
        }
    }

    /// BG1 종료 직후 준비된 BG2로 출력 표면을 전환한다.
    private void HandleIntroVideoFinished(VideoPlayer source)
    {
        if (source != introVideoPlayer || loopVideoPlayer == null)
        {
            return;
        }

        source.loopPointReached -= HandleIntroVideoFinished;
        isLoopVideoPending = true;
        if (loopVideoPlayer.isPrepared)
        {
            StartLoopVideo();
        }
    }

    /// 미리 준비한 BG2 텍스처를 표시하고 무한 반복 재생한다.
    private void StartLoopVideo()
    {
        isLoopVideoPending = false;
        introVideoSurface.texture = loopVideoTexture;
        loopVideoPlayer.Play();
    }

    /// VideoPlayer 오류를 Unity Console에 기록한다.
    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"MainMenuUIManager Intro video error: {message}", this);
    }

    /// 현재 씬의 루트 오브젝트 중 이름이 일치하는 계층을 찾는다.
    private GameObject FindSceneRoot(string objectName)
    {
        var roots = gameObject.scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && string.Equals(roots[i].name, objectName, System.StringComparison.Ordinal))
            {
                return roots[i];
            }
        }

        return null;
    }

    /// 계층 경로의 GameObject를 찾아 필드에 연결하고 누락 시 오류를 기록한다.
    private GameObject FindGameObject(
        GameObject root,
        string path,
        string fieldName,
        ref bool valid)
    {
        var target = root != null ? root.transform.Find(path) : null;
        if (target == null)
        {
            LogBindingError(fieldName, path, "GameObject");
            valid = false;
            return null;
        }

        return target.gameObject;
    }

    /// 계층 경로의 컴포넌트를 찾아 필드에 연결하고 누락 시 오류를 기록한다.
    private T FindComponent<T>(
        GameObject root,
        string path,
        string fieldName,
        ref bool valid)
        where T : Component
    {
        var target = root != null ? root.transform.Find(path) : null;
        var component = target != null ? target.GetComponent<T>() : null;
        if (component == null)
        {
            LogBindingError(fieldName, path, typeof(T).Name);
            valid = false;
            return null;
        }

        return component;
    }

    /// 자동 참조 연결 실패 원인을 필드·경로·타입과 함께 기록한다.
    private void LogBindingError(string fieldName, string path, string expectedType)
    {
        Debug.LogError(
            $"MainMenuUIManager BindObject failed: field '{fieldName}', path 'Canvas/{path}', expected '{expectedType}'.",
            this);
    }

    private void BindButtons()
    {
        Bind(introGameStartButton, ShowMainMenu, nameof(introGameStartButton));
        Bind(runButton, ShowMonsterSelect, nameof(runButton));
        Bind(monsterSelectGameStartButton, StartSelectedMonsterRun, nameof(monsterSelectGameStartButton));

        Bind(arielButton, () => SelectMonster("ariel"), nameof(arielButton));
        Bind(eveButton, () => SelectMonster("eve"), nameof(eveButton));
        Bind(seinButton, () => SelectMonster("sein"), nameof(seinButton));
        Bind(vegaButton, () => SelectMonster("vega"), nameof(vegaButton));
        Bind(rinButton, () => SelectMonster("rin"), nameof(rinButton));
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action, string fieldName)
    {
        if (button == null)
        {
            Debug.LogError($"MainMenuUIManager Bind failed: button field '{fieldName}' is null.");
            return;
        }

        if (action == null)
        {
            Debug.LogError($"MainMenuUIManager Bind failed: action for '{fieldName}' is null.");
            return;
        }

        button.onClick.AddListener(action);
    }

    /// 인트로 패널을 열어 게임 시작 화면을 보여준다.
    private void ShowIntro()
    {
        SetOnlyActive(introPanel);
    }

    /// 메인 메뉴 패널을 보여준다.
    private void ShowMainMenu()
    {
        SetOnlyActive(mainMenuPanel);
    }

    /// 몬스터 선택 패널을 열어 출전 몬스터를 고르게 한다.
    private void ShowMonsterSelect()
    {
        SetOnlyActive(monsterSelectPanel);
    }

    /// 선택한 몬스터의 Standing 정보와 출전 몬스터를 표시·저장한다.
    private void SelectMonster(string monsterName)
    {
        selectedMonsterName = monsterName;

        var monster = GameDataLoader.CurrentCatalog?.GetMonster(monsterName);
        if (monster == null || monster.Image == null)
        {
            Debug.LogError($"MainMenuUIManager cannot resolve standing image for monster '{monsterName}'.", this);
            return;
        }

        monsterStandingImage.sprite = monster.Image;
        monsterNameText.text = monster.DisplayName;
        monsterDescriptionText.text = monster.RoleSummary;
        monsterStanding.SetActive(true);
    }

    /// 선택한 몬스터를 StartContext에 저장하고 Run 씬을 연다.
    private void StartSelectedMonsterRun()
    {
        if (isLoadingRunScene)
        {
            return;
        }

        var monsterName = string.IsNullOrWhiteSpace(selectedMonsterName) ? DefaultMonsterName : selectedMonsterName;
        StartContext.Prepare(monsterName);

        if (string.IsNullOrWhiteSpace(NewRunScenePath))
        {
            Debug.LogError("MainMenuUIManager cannot load InGameScene because the scene path is empty.");
            return;
        }

        isLoadingRunScene = true;
        if (monsterSelectGameStartButton != null)
        {
            monsterSelectGameStartButton.interactable = false;
        }

        StartCoroutine(LoadRunSceneAsync());
    }

    private IEnumerator LoadRunSceneAsync()
    {
        var loadOperation = SceneManager.LoadSceneAsync(NewRunScenePath);
        if (loadOperation == null)
        {
            isLoadingRunScene = false;
            if (monsterSelectGameStartButton != null)
            {
                monsterSelectGameStartButton.interactable = true;
            }

            Debug.LogError("MainMenuUIManager failed to start asynchronous InGameScene loading.", this);
            yield break;
        }

        yield return loadOperation;
    }

    /// 씬 종료 시 런타임 영상 리소스와 이벤트를 정리한다.
    private void OnDestroy()
    {
        if (introSequence != null)
        {
            StopCoroutine(introSequence);
        }

        if (introVideoPlayer != null)
        {
            introVideoPlayer.prepareCompleted -= HandleIntroVideoPrepared;
            introVideoPlayer.loopPointReached -= HandleIntroVideoFinished;
            introVideoPlayer.errorReceived -= HandleVideoError;
            introVideoPlayer.Stop();
        }

        if (loopVideoPlayer != null)
        {
            loopVideoPlayer.prepareCompleted -= HandleLoopVideoPrepared;
            loopVideoPlayer.errorReceived -= HandleVideoError;
            loopVideoPlayer.Stop();
        }

        if (introVideoTexture != null)
        {
            introVideoTexture.Release();
            Destroy(introVideoTexture);
        }

        if (loopVideoTexture != null)
        {
            loopVideoTexture.Release();
            Destroy(loopVideoTexture);
        }
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        UiObjectUtility.SetActive(introPanel, introPanel == activePanel);
        UiObjectUtility.SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        UiObjectUtility.SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

}
