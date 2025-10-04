using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    // --- INSPECTOR ASSIGNMENTS ---
    public Text menuText;
    // NEW (optional): assign if your label is TextMeshProUGUI instead of legacy Text
    public TMPro.TextMeshProUGUI menuTMPText;

    public AudioClip selectClip;
    public AudioClip sceneSelectClip;
    public Canvas menuCanvas;

    // NEW: The panel that appears when the user selects "Exit"
    public GameObject confirmationPanel; // <--- MUST be linked to the Confirmation Panel UI

    // --- CONFIGURATION ---
    private const string ForestSceneName = "ForestScene";
    private const string ForestDemoSceneName = "Demo";

    // exact path of the Demo scene as it appears in Build Settings
    private const string DemoScenePath = "Assets/NatureStarterKit2/Scene/Demo.unity";

    private const float SceneCycleDelay = 3.0f;
    private const float FinalSceneDelay = 3.0f;

    // --- PRIVATE VARIABLES ---
    private AudioSource audioSource;
    private int option = 0; // 0 for "Select Scene", 1 for "Exit"
    private readonly string[] options = { "Select Scene", "Exit" };

    private readonly string[] cycleScenes = { "BeachScene", "MountainScene", "ForestScene" };

    private const string WaterfallSceneName = "WaterfallScene";
    private const string MenuSceneName = "Main Menu Demo";

    private int currentCycleIndex = 0;
    private bool isConfirmingExit = false;

    // NEW: control flags
    private bool autoCycleStarted = false;
    private bool destroyOnNextLoad = false;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color forestHighlightColor = Color.green;

    void Awake()
    {
        // Persist so it can drive the cycle across scenes (menu UI/audio visible everywhere until Demo)
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        audioSource = GetComponent<AudioSource>();

        if (menuText != null)
        {
            menuText.text = options[option];
        }
        if (menuTMPText != null)
        {
            menuTMPText.text = options[option];
        }

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        // If we start in the menu scene, kick off the auto-cycle once.
        if (SceneManager.GetActiveScene().name == MenuSceneName)
        {
            autoCycleStarted = true;
            currentCycleIndex = -1; // reset index
            StartCoroutine(AutoCycleScenes(SceneCycleDelay));
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Keep menu UI visible across all scenes until we intentionally hide it for Demo.
        if (menuCanvas != null) menuCanvas.enabled = true;

        // If we land in Demo as part of "Proceed", self-destruct to keep it clean
        if (destroyOnNextLoad && (scene.path == DemoScenePath || scene.name == ForestDemoSceneName))
        {
            Destroy(gameObject);
            return;
        }

        // Reset menu state when back in menu
        if (scene.name == MenuSceneName)
        {
            isConfirmingExit = false;
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            currentCycleIndex = 0;

            // Start auto-cycle exactly once from the menu
            if (!autoCycleStarted)
            {
                autoCycleStarted = true;
                StartCoroutine(AutoCycleScenes(SceneCycleDelay));
            }
        }

        // Show the “Proceed to Demo” panel when we arrive at the Forest scene (optional UI helper)
        if (scene.name == ForestSceneName)
            StartCoroutine(ShowProceedPanelAfterDelay(FinalSceneDelay));
    }

    void Update()
    {
        // Update menu text + color
        if (menuText != null) menuText.text = options[option];
        if (menuTMPText != null) menuTMPText.text = options[option];

        bool highlight = (SceneManager.GetActiveScene().name == ForestSceneName && option == 0);
        if (menuText != null)    menuText.color    = highlight ? forestHighlightColor : normalColor;
        if (menuTMPText != null) menuTMPText.color = highlight ? forestHighlightColor : normalColor;

        // Input is always allowed while the menu UI is visible and not confirming exit
        if (menuCanvas != null && menuCanvas.enabled && !isConfirmingExit)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) { MoveRight(); }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { MoveLeft(); }
            if (Input.GetKeyDown(KeyCode.Return)) { ExecuteCurrentOption(); }
        }
    }

    // =======================================================
    // --- UI BUTTON FUNCTIONS (Called by Event Trigger/OnClick) ---
    // =======================================================

    public void UIMoveRight() { MoveRight(); }
    public void UIMoveLeft() { MoveLeft(); }
    public void UIPressEnter() { ExecuteCurrentOption(); }

    private void ExecuteCurrentOption()
    {
        if (option == 0) // "Select Scene"
        {
            // If we're in Forest, this is the moment to go to Demo.
            if (SceneManager.GetActiveScene().name == ForestSceneName)
            {
                GoToForestDemo(); // will stop audio, hide UI, and load Demo
            }
            else
            {
                // Otherwise, ensure the cycle is running (start at Waterfall if not already away from menu)
                if (!autoCycleStarted)
                {
                    autoCycleStarted = true;
                    currentCycleIndex = -1;
                    StartCoroutine(AutoCycleScenes(SceneCycleDelay));
                }
                // And if we are still in the menu, jump to Waterfall to begin the cycle.
                if (SceneManager.GetActiveScene().name == MenuSceneName)
                {
                    StartCoroutine(LoadSceneForCycle(WaterfallSceneName));
                }
                // If we're already somewhere in the cycle, do nothing special.
            }
        }
        else if (option == 1) // Exit
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
                isConfirmingExit = true; // Block main menu input
            }
        }
    }

    void MoveRight()
    {
        if (option < options.Length - 1)
        {
            option++;
            if (audioSource != null && selectClip != null) audioSource.PlayOneShot(selectClip);
        }
    }

    void MoveLeft()
    {
        if (option > 0)
        {
            option--;
            if (audioSource != null && selectClip != null) audioSource.PlayOneShot(selectClip);
        }
    }

    // PUBLIC METHOD: Called by the "YES" button on the Confirmation Panel
    public void ConfirmExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("Quitting Game...");
#endif
    }

    // PUBLIC METHOD: Called by the "NO" or "Cancel" button on the Confirmation Panel
    public void CancelExitGame()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
            isConfirmingExit = false; // Restore main menu input
        }
    }

    // --- LOADERS ---
    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        if (audioSource != null && sceneSelectClip != null)
        {
            audioSource.PlayOneShot(sceneSelectClip);
            yield return new WaitForSeconds(0.3f);
        }

        // Keep menu UI/audio visible during cycle
        SceneManager.LoadScene(sceneName);
    }

    // loader that targets the exact scene by full path (used only for Demo)
    IEnumerator LoadSceneByPathWithDelay(string scenePath)
    {
        if (audioSource != null && sceneSelectClip != null)
        {
            audioSource.PlayOneShot(sceneSelectClip);
            yield return new WaitForSeconds(0.2f);
        }

        // For Demo: hide UI and stop audio right before switching
        if (menuCanvas != null) menuCanvas.enabled = false;
        if (audioSource != null) audioSource.Stop();

        // Mark that we should destroy ourselves after the load completes
        destroyOnNextLoad = true;

        SceneManager.LoadScene(SceneUtility.GetBuildIndexByScenePath(scenePath));
    }

    IEnumerator LoadSceneForCycle(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        yield break;
    }

    // =======================================================
    // --- AUTOMATION LOGIC ---
    // =======================================================

    IEnumerator AutoCycleScenes(float delay)
    {
        // Always start with Waterfall
        yield return StartCoroutine(LoadSceneForCycle(WaterfallSceneName));

        currentCycleIndex = -1; // first increment = 0 (Beach)

        while (true)
        {
            yield return new WaitForSeconds(delay);

            currentCycleIndex++;
            if (currentCycleIndex > cycleScenes.Length - 1)
            {
                // Loop back to Waterfall
                currentCycleIndex = -1;
                yield return StartCoroutine(LoadSceneForCycle(WaterfallSceneName));
                continue;
            }

            string sceneToLoad = cycleScenes[currentCycleIndex];
            yield return StartCoroutine(LoadSceneForCycle(sceneToLoad));

            if (sceneToLoad == ForestSceneName && destroyOnNextLoad)
                yield break;
        }
    }

    // Optional: Tag your panel in Forest scene as "ProceedToDemo"
    [SerializeField] private string proceedPanelTag = "ProceedToDemo";
    private GameObject proceedPanelInScene;

    IEnumerator ShowProceedPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        proceedPanelInScene = GameObject.FindWithTag(proceedPanelTag);
        if (proceedPanelInScene != null)
        {
            proceedPanelInScene.SetActive(true);

            var btn = proceedPanelInScene.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GoToForestDemo);
            }
        }
    }

    // Called by the Forest "Proceed" button OR when Select Scene is pressed in Forest
    public void GoToForestDemo()
    {
        if (menuCanvas != null) menuCanvas.enabled = false;
        if (audioSource != null) audioSource.Stop();

        destroyOnNextLoad = true; // so we self-destruct after loading Demo
        StartCoroutine(LoadSceneByPathWithDelay(DemoScenePath));
    }
}
