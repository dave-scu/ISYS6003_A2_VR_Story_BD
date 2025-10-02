using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    // --- INSPECTOR ASSIGNMENTS ---
    public Text menuText;
    public AudioClip selectClip;
    public AudioClip sceneSelectClip;
    public Canvas menuCanvas; 
    
    // NEW: The panel that appears when the user selects "Exit"
    public GameObject confirmationPanel; // <--- MUST be linked to the Confirmation Panel UI

    // --- CONFIGURATION ---
    private const float SceneCycleDelay = 5.0f; 
    private const float FinalSceneDelay = 5.0f; 

    // --- PRIVATE VARIABLES ---
    private AudioSource audioSource;
    private int option = 0; // 0 for "Select Scene", 1 for "Exit"
    private readonly string[] options = { "Select Scene", "Exit" }; 

    private readonly string[] cycleScenes = { "BeachScene", "MountainScene", "ForestScene" };

    private const string WaterfallSceneName = "WaterfallScene";
    private const string MenuSceneName = "Main Menu Demo"; 

    private int currentCycleIndex = 0; 
    private bool isConfirmingExit = false; 

    void Start()
    {
        // Don'tDestroyOnLoad is attached to an object holding this script.
        DontDestroyOnLoad(this.gameObject); 
        SceneManager.sceneLoaded += OnSceneLoaded;

        audioSource = GetComponent<AudioSource>();
        
        if (menuText != null)
        {
            menuText.text = options[option];
        }

        // Ensure the confirmation panel is hidden at startup
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are NOT in the Main Menu
        if (scene.name != MenuSceneName)
        {
            StopAllCoroutines(); 
            StartCoroutine(AutoCycleScenes(SceneCycleDelay));
        }
        else
        {
            // If we are back at the Main Menu, re-enable the UI
            StopAllCoroutines();
            if (menuCanvas != null) 
            {
                menuCanvas.enabled = true; // SHOWS THE MENU BAR
            }
            
            // Ensure pop-up is hidden and index is reset
            isConfirmingExit = false;
            if (confirmationPanel != null) confirmationPanel.SetActive(false); 
            currentCycleIndex = 0;
        }
    }
    
    void Update()
    {
        // Update menu text
        if (menuText != null)
        {
            menuText.text = options[option];
        }

        // Only allow keyboard input if the main menu is visible AND we are NOT confirming exit
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

    // These public methods should be linked to the OnClick events of your UI buttons/triggers
    public void UIMoveRight() { MoveRight(); }
    public void UIMoveLeft() { MoveLeft(); }
    public void UIPressEnter() { ExecuteCurrentOption(); }

    private void ExecuteCurrentOption()
    {
        // Option 0: Select Scene
        if (option == 0)
        {
            currentCycleIndex = -1; 
            StartCoroutine(LoadSceneWithDelay(WaterfallSceneName));
        }
        // Option 1: Exit (Show Confirmation Pop-up)
        else if (option == 1)
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
        // Cycles from 0 ("Select Scene") to 1 ("Exit")
        if (option < options.Length - 1)
        {
            option++; 
            if (audioSource != null && selectClip != null) audioSource.PlayOneShot(selectClip);
        }
    }

    void MoveLeft()
    {
        // Cycles from 1 ("Exit") to 0 ("Select Scene")
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


    // Coroutine for MANUAL scene load from the menu
    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        if (audioSource != null && sceneSelectClip != null)
        {
            audioSource.PlayOneShot(sceneSelectClip);
            yield return new WaitForSeconds(0.3f); 
        }
        
        // HIDES THE MENU BAR
        if (menuCanvas != null) menuCanvas.enabled = false; 

        StopAllCoroutines(); 
        
        SceneManager.LoadScene(sceneName);
    }

    // Coroutine for AUTOMATICALLY loading the next scene in the cycle
    IEnumerator LoadSceneForCycle(string sceneName)
    {
        StopAllCoroutines(); 
        
        SceneManager.LoadScene(sceneName);
        yield break; 
    }
    
    // =======================================================
    // --- AUTOMATION LOGIC ---
    // =======================================================
    
    IEnumerator AutoCycleScenes(float delay)
    {
        // Check if we are currently in the LAST scene
        if (SceneManager.GetActiveScene().name == cycleScenes[cycleScenes.Length - 1])
        {
            yield return new WaitForSeconds(FinalSceneDelay); 
            yield return StartCoroutine(LoadSceneForCycle(MenuSceneName));
            yield break; 
        }
        
        while (true)
        {
            yield return new WaitForSeconds(delay); 

            currentCycleIndex++;

            if (currentCycleIndex > cycleScenes.Length - 1)
            {
                break; 
            }

            string sceneToLoad = cycleScenes[currentCycleIndex];

            yield return StartCoroutine(LoadSceneForCycle(sceneToLoad));
        }

        yield return new WaitForSeconds(FinalSceneDelay);
        yield return StartCoroutine(LoadSceneForCycle(MenuSceneName));
    }
}