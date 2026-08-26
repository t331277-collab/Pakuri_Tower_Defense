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
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Video;

public class MainMenuUIManager : MonoBehaviour
{
    private const string NewRunScenePath = "Assets/Scenes/NewScene/InGameScene.unity";
    private const string DefaultMonsterName = "eve";
    private const float SummaryFadeDuration = 0.5f;
    private const float GameStartFadeDuration = 1f;
    private const float MenuTransitionDuration = 0.15f;
    private const float SecondaryMenuFontSize = 40f;
    private const float PrimaryMenuFontSize = 50f;
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
    private GameObject monsterStandingPanel;
    private GameObject monsterStanding;
    private Button introGameStartButton;
    private Button runButton;
    private Button tutorialButton;
    private Button upArrowButton;
    private Button downArrowButton;
    private Button monsterSelectGameStartButton;
    private Button arielButton;
    private Button eveButton;
    private Button seinButton;
    private Button vegaButton;
    private Button rinButton;
    private Image monsterStandingImage;
    private Image monsterMainTypeImage;
    private Image monsterSubTypeImage;
    private SpriteRenderer monsterStandingSpriteRenderer;
    private Animator monsterStandingAnimator;
    private PlayableGraph monsterStandingGraph;
    private TextMeshProUGUI monsterNameText;
    private TextMeshProUGUI monsterDescriptionText;
    private TextMeshProUGUI runButtonText;
    private TextMeshProUGUI tutorialButtonText;
    private RectTransform runButtonRect;
    private RectTransform tutorialButtonRect;
    private RectTransform downMarkerRect;
    private CanvasGroup summaryCanvasGroup;
    private CanvasGroup gameStartCanvasGroup;
    private VideoPlayer introVideoPlayer;
    private VideoPlayer loopVideoPlayer;
    private RawImage introVideoSurface;
    private RenderTexture introVideoTexture;
    private RenderTexture loopVideoTexture;
    private Coroutine introSequence;
    private Coroutine menuTransition;

    private Vector2 runOriginalPosition;
    private Vector2 runOriginalSize;
    private Vector2 tutorialOriginalPosition;
    private Vector2 tutorialOriginalSize;
    private Transform[] originalMenuSiblingOrder;

    private string selectedMonsterName;
    private bool isLoadingRunScene;
    private bool isLoopVideoPending;

    /// 재생 중인 SpriteRenderer 프레임을 기존 UGUI Image에 반영한다.
    private void LateUpdate()
    {
        if (monsterStandingImage == null || monsterStandingSpriteRenderer == null)
        {
            return;
        }

        var sprite = monsterStandingSpriteRenderer.sprite;
        if (sprite != null)
        {
            monsterStandingImage.sprite = sprite;
        }
    }

    /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
    private void Start()
    {
        if (!BindObject())
        {
            return;
        }

        InitializeMainMenuLayout();
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
        monsterStandingPanel = FindGameObject(canvas, "MosterSelectUI/MonsterStandingPanel", nameof(monsterStandingPanel), ref valid);
        monsterStanding = FindGameObject(canvas, "MosterSelectUI/MonsterStandingPanel/MonsterStanding", nameof(monsterStanding), ref valid);

        introGameStartButton = FindComponent<Button>(canvas, "Intro/GameStart", nameof(introGameStartButton), ref valid);
        runButton = FindComponent<Button>(canvas, "MainMenuUI/RunBtn", nameof(runButton), ref valid);
        tutorialButton = FindComponent<Button>(canvas, "MainMenuUI/Tutorial", nameof(tutorialButton), ref valid);
        upArrowButton = FindOrAddButton(canvas, "MainMenuUI/UPArrow", nameof(upArrowButton), ref valid);
        downArrowButton = FindOrAddButton(canvas, "MainMenuUI/DownArrow", nameof(downArrowButton), ref valid);
        monsterSelectGameStartButton = FindComponent<Button>(canvas, "MosterSelectUI/GameStart", nameof(monsterSelectGameStartButton), ref valid);
        runButtonRect = FindComponent<RectTransform>(canvas, "MainMenuUI/RunBtn", nameof(runButtonRect), ref valid);
        tutorialButtonRect = FindComponent<RectTransform>(canvas, "MainMenuUI/Tutorial", nameof(tutorialButtonRect), ref valid);
        downMarkerRect = FindComponent<RectTransform>(canvas, "MainMenuUI/Down", nameof(downMarkerRect), ref valid);
        runButtonText = FindComponent<TextMeshProUGUI>(canvas, "MainMenuUI/RunBtn/Text (TMP)", nameof(runButtonText), ref valid);
        tutorialButtonText = FindComponent<TextMeshProUGUI>(canvas, "MainMenuUI/Tutorial/Text (TMP)", nameof(tutorialButtonText), ref valid);
        monsterStandingImage = FindComponent<Image>(canvas, "MosterSelectUI/MonsterStandingPanel/MonsterStanding", nameof(monsterStandingImage), ref valid);
        monsterNameText = FindComponent<TextMeshProUGUI>(canvas, "MosterSelectUI/MonsterStandingPanel/MonsterStanding/Name", nameof(monsterNameText), ref valid);
        monsterDescriptionText = FindComponent<TextMeshProUGUI>(canvas, "MosterSelectUI/MonsterStandingPanel/MonsterStanding/Desc", nameof(monsterDescriptionText), ref valid);
        monsterMainTypeImage = FindComponent<Image>(canvas, "MosterSelectUI/MonsterStandingPanel/MainType", nameof(monsterMainTypeImage), ref valid);
        monsterSubTypeImage = FindComponent<Image>(canvas, "MosterSelectUI/MonsterStandingPanel/SubType", nameof(monsterSubTypeImage), ref valid);
        arielButton = FindComponent<Button>(canvas, "MosterSelectUI/Panel/Ariel", nameof(arielButton), ref valid);
        eveButton = FindComponent<Button>(canvas, "MosterSelectUI/Panel/Eve", nameof(eveButton), ref valid);
        seinButton = FindComponent<Button>(canvas, "MosterSelectUI/Panel/Sein", nameof(seinButton), ref valid);
        vegaButton = FindComponent<Button>(canvas, "MosterSelectUI/Panel/Vega", nameof(vegaButton), ref valid);
        rinButton = FindComponent<Button>(canvas, "MosterSelectUI/Panel/Rin", nameof(rinButton), ref valid);
        if (valid)
        {
            EnsureStandingAnimationComponents();
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

    /// 화살표 Image에 기존 Button을 재사용하거나 클릭용 Button을 런타임으로 추가한다.
    private Button FindOrAddButton(
        GameObject root,
        string path,
        string fieldName,
        ref bool valid)
    {
        var target = root != null ? root.transform.Find(path) : null;
        var image = target != null ? target.GetComponent<Image>() : null;
        if (image == null)
        {
            LogBindingError(fieldName, path, "Image");
            valid = false;
            return null;
        }

        var button = target.GetComponent<Button>() ?? target.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
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
        Bind(tutorialButton, StartTutorialRun, nameof(tutorialButton));
        Bind(upArrowButton, () => StartMenuTransition(true), nameof(upArrowButton));
        Bind(downArrowButton, () => StartMenuTransition(false), nameof(downArrowButton));
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

    /// 씬에 작성된 세 기준 RectTransform을 저장하고 기본 Run 선택 상태를 적용한다.
    private void InitializeMainMenuLayout()
    {
        runOriginalPosition = runButtonRect.anchoredPosition;
        runOriginalSize = runButtonRect.sizeDelta;
        tutorialOriginalPosition = tutorialButtonRect.anchoredPosition;
        tutorialOriginalSize = tutorialButtonRect.sizeDelta;
        var menuRoot = runButtonRect.parent;
        originalMenuSiblingOrder = new Transform[menuRoot.childCount];
        for (var i = 0; i < originalMenuSiblingOrder.Length; i++)
        {
            originalMenuSiblingOrder[i] = menuRoot.GetChild(i);
        }

        SetMenuLayout(false);
        SetMenuInteraction(false);
        upArrowButton.gameObject.SetActive(true);
        downArrowButton.gameObject.SetActive(false);
    }

    /// 화살표 입력 하나당 메뉴 전환 코루틴 하나만 실행한다.
    private void StartMenuTransition(bool tutorialIsPrimary)
    {
        if (menuTransition != null)
        {
            return;
        }

        SetMenuLayerOrder(tutorialIsPrimary);
        runButton.interactable = false;
        tutorialButton.interactable = false;
        upArrowButton.interactable = false;
        downArrowButton.interactable = false;
        menuTransition = StartCoroutine(AnimateMenuLayout(tutorialIsPrimary));
    }

    /// Run과 Tutorial의 위치·크기·폰트를 같은 0.3초 동안 보간한다.
    private IEnumerator AnimateMenuLayout(bool tutorialIsPrimary)
    {
        var runStartPosition = runButtonRect.anchoredPosition;
        var runStartSize = runButtonRect.sizeDelta;
        var runStartFontSize = runButtonText.fontSize;
        var tutorialStartPosition = tutorialButtonRect.anchoredPosition;
        var tutorialStartSize = tutorialButtonRect.sizeDelta;
        var tutorialStartFontSize = tutorialButtonText.fontSize;
        var runTargetPosition = tutorialIsPrimary ? downMarkerRect.anchoredPosition : runOriginalPosition;
        var runTargetSize = tutorialIsPrimary ? tutorialOriginalSize : runOriginalSize;
        var runTargetFontSize = tutorialIsPrimary ? SecondaryMenuFontSize : PrimaryMenuFontSize;
        var tutorialTargetPosition = tutorialIsPrimary ? runOriginalPosition : tutorialOriginalPosition;
        var tutorialTargetSize = tutorialIsPrimary ? runOriginalSize : tutorialOriginalSize;
        var tutorialTargetFontSize = tutorialIsPrimary ? PrimaryMenuFontSize : SecondaryMenuFontSize;
        var elapsed = 0f;

        while (elapsed < MenuTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / MenuTransitionDuration);
            runButtonRect.anchoredPosition = Vector2.Lerp(runStartPosition, runTargetPosition, progress);
            runButtonRect.sizeDelta = Vector2.Lerp(runStartSize, runTargetSize, progress);
            runButtonText.fontSize = Mathf.Lerp(runStartFontSize, runTargetFontSize, progress);
            tutorialButtonRect.anchoredPosition = Vector2.Lerp(tutorialStartPosition, tutorialTargetPosition, progress);
            tutorialButtonRect.sizeDelta = Vector2.Lerp(tutorialStartSize, tutorialTargetSize, progress);
            tutorialButtonText.fontSize = Mathf.Lerp(tutorialStartFontSize, tutorialTargetFontSize, progress);
            yield return null;
        }

        SetMenuLayout(tutorialIsPrimary);
        SetMenuInteraction(tutorialIsPrimary);
        upArrowButton.gameObject.SetActive(!tutorialIsPrimary);
        downArrowButton.gameObject.SetActive(tutorialIsPrimary);
        upArrowButton.interactable = true;
        downArrowButton.interactable = true;
        menuTransition = null;
    }

    /// 초기화와 애니메이션 완료 시 목표값을 정확히 적용한다.
    private void SetMenuLayout(bool tutorialIsPrimary)
    {
        runButtonRect.anchoredPosition = tutorialIsPrimary ? downMarkerRect.anchoredPosition : runOriginalPosition;
        runButtonRect.sizeDelta = tutorialIsPrimary ? tutorialOriginalSize : runOriginalSize;
        runButtonText.fontSize = tutorialIsPrimary ? SecondaryMenuFontSize : PrimaryMenuFontSize;
        tutorialButtonRect.anchoredPosition = tutorialIsPrimary ? runOriginalPosition : tutorialOriginalPosition;
        tutorialButtonRect.sizeDelta = tutorialIsPrimary ? runOriginalSize : tutorialOriginalSize;
        tutorialButtonText.fontSize = tutorialIsPrimary ? PrimaryMenuFontSize : SecondaryMenuFontSize;
    }

    /// 중앙에 배치된 기능 버튼 하나만 클릭 가능하게 한다.
    private void SetMenuInteraction(bool tutorialIsPrimary)
    {
        runButton.interactable = !tutorialIsPrimary;
        tutorialButton.interactable = tutorialIsPrimary;
    }

    /// Tutorial 선택 중에는 Tutorial을 Run보다 위, 두 화살표보다 아래에 그린다.
    private void SetMenuLayerOrder(bool tutorialIsPrimary)
    {
        if (!tutorialIsPrimary)
        {
            for (var i = 0; i < originalMenuSiblingOrder.Length; i++)
            {
                originalMenuSiblingOrder[i].SetSiblingIndex(i);
            }

            return;
        }

        runButtonRect.SetAsLastSibling();
        tutorialButtonRect.SetAsLastSibling();
        upArrowButton.transform.SetAsLastSibling();
        downArrowButton.transform.SetAsLastSibling();
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
        monsterStandingPanel.SetActive(false);
    }

    /// 선택한 몬스터의 Standing 애니메이션과 출전 몬스터를 표시·저장한다.
    private void SelectMonster(string monsterName)
    {
        selectedMonsterName = monsterName;

        var monster = GameDataLoader.CurrentCatalog?.GetMonster(monsterName);
        if (monster == null || monster.StandingAnimation == null)
        {
            Debug.LogError($"MainMenuUIManager cannot resolve standing animation for monster '{monsterName}'.", this);
            return;
        }

        monsterNameText.text = monster.DisplayName;
        monsterDescriptionText.text = monster.RoleSummary;
        monsterMainTypeImage.sprite = monster.MainTypeIcon;
        monsterSubTypeImage.sprite = monster.SubTypeIcon;
        monsterStandingPanel.SetActive(true);
        monsterStanding.SetActive(true);
        PlayStandingAnimation(monster.StandingAnimation, monster.Image);
    }

    /// MonsterStanding에 필요한 애니메이터와 SpriteRenderer를 한 번만 구성한다.
    private void EnsureStandingAnimationComponents()
    {
        if (monsterStandingSpriteRenderer == null)
        {
            monsterStandingSpriteRenderer = monsterStanding.GetComponent<SpriteRenderer>();
            if (monsterStandingSpriteRenderer == null)
            {
                monsterStandingSpriteRenderer = monsterStanding.AddComponent<SpriteRenderer>();
            }

            monsterStandingSpriteRenderer.enabled = false;
        }

        if (monsterStandingAnimator == null)
        {
            monsterStandingAnimator = monsterStanding.GetComponent<Animator>();
            if (monsterStandingAnimator == null)
            {
                monsterStandingAnimator = monsterStanding.AddComponent<Animator>();
            }

            monsterStandingAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }

    /// CSV에서 해석한 AnimationClip을 MonsterStanding의 SpriteRenderer에 재생한다.
    private void PlayStandingAnimation(AnimationClip animationClip, Sprite fallbackSprite)
    {
        EnsureStandingAnimationComponents();
        DestroyStandingAnimationGraph();

        monsterStandingImage.sprite = fallbackSprite;
        monsterStandingSpriteRenderer.sprite = fallbackSprite;

        monsterStandingGraph = PlayableGraph.Create("MonsterStandingIdle");
        monsterStandingGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var clipPlayable = AnimationClipPlayable.Create(monsterStandingGraph, animationClip);
        var output = AnimationPlayableOutput.Create(
            monsterStandingGraph,
            "MonsterStandingIdle",
            monsterStandingAnimator);
        output.SetSourcePlayable(clipPlayable);
        monsterStandingGraph.Play();
        monsterStandingAnimator.Update(0f);
    }

    /// 선택 변경·씬 종료 시 Standing PlayableGraph를 해제한다.
    private void DestroyStandingAnimationGraph()
    {
        if (monsterStandingGraph.IsValid())
        {
            monsterStandingGraph.Destroy();
        }
    }

    /// 선택한 몬스터를 StartContext에 저장하고 Run 씬을 연다.
    private void StartSelectedMonsterRun()
    {
        var monsterName = string.IsNullOrWhiteSpace(selectedMonsterName) ? DefaultMonsterName : selectedMonsterName;
        StartRun(monsterName, RunMode.Normal);
    }

    private void StartTutorialRun()
    {
        StartRun(DefaultMonsterName, RunMode.Tutorial);
    }

    /// 선택 몬스터와 Run 종류를 저장하고 기존 비동기 InGameScene 로드를 시작한다.
    private void StartRun(string monsterName, RunMode mode)
    {
        if (isLoadingRunScene)
        {
            return;
        }

        StartContext.Prepare(monsterName, mode);

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

        if (tutorialButton != null)
        {
            tutorialButton.interactable = false;
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

            if (tutorialButton != null)
            {
                tutorialButton.interactable = true;
            }

            Debug.LogError("MainMenuUIManager failed to start asynchronous InGameScene loading.", this);
            yield break;
        }

        yield return loadOperation;
    }

    /// 씬 종료 시 런타임 영상 리소스와 이벤트를 정리한다.
    private void OnDestroy()
    {
        DestroyStandingAnimationGraph();

        if (introSequence != null)
        {
            StopCoroutine(introSequence);
        }

        if (menuTransition != null)
        {
            StopCoroutine(menuTransition);
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
