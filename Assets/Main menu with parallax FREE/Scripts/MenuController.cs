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
    // CRITICAL: Must be linked in the Inspector!
    public Canvas menuCanvas; 

    // --- CONFIGURATION ---
    private const float SceneCycleDelay = 5.0f; // Time in seconds between automatic scene changes

    // --- PRIVATE VARIABLES ---
    private AudioSource audioSource;
    private int option = 0;
    private readonly string[] options = { "Select Scene", "Exit" };

    private readonly string[] cycleScenes = { "BeachScene", "MountainScene", "ForestScene" };

    private const string WaterfallSceneName = "WaterfallScene";
    private const string MenuSceneName = "Main Menu Demo"; 

    private int currentCycleIndex = 0;

    void Start()
    {
        // PREVENTS THIS CONTROLLER FROM BEING DESTROYED WHEN SCENES LOAD
        DontDestroyOnLoad(this.gameObject); 

        // CRITICAL: Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 1. Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // 2. Set initial menu text
        if (menuText != null)
        {
            menuText.text = options[option];
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are NOT in the Main Menu (i.e., we are in a game scene)
        if (scene.name != MenuSceneName)
        {
            // Stop any potentially running timer and start the new one for this scene
            StopAllCoroutines(); 
            
            // The GameObject is now guaranteed to be active, so we can start the coroutine.
            StartCoroutine(AutoCycleScenes(SceneCycleDelay));
        }
        else
        {
            // If we are back at the Main Menu, re-enable the UI
            StopAllCoroutines();
            if (menuCanvas != null) menuCanvas.enabled = true; // SHOWS THE MENU UI
        }
    }
    
    void Update()
    {
        // This runs continuously, but the UI is only visible when menuCanvas.enabled = true.
        
        if (menuText != null)
        {
            menuText.text = options[option];
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) { MoveRight(); }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { MoveLeft(); }
        if (Input.GetKeyDown(KeyCode.Return)) { ExecuteCurrentOption(); }
    }

    // =======================================================
    // --- UI BUTTON FUNCTIONS (etc.) ---
    // =======================================================

    public void UIMoveRight() { MoveRight(); }
    public void UIMoveLeft() { MoveLeft(); }
    public void UIPressEnter() { ExecuteCurrentOption(); }

    private void ExecuteCurrentOption()
    {
        if (option == 0)
        {
            StartCoroutine(LoadSceneWithDelay(WaterfallSceneName));
        }
        else if (option == 1)
        {
            Application.Quit();
        #if UNITY_EDITOR
            Debug.Log("Quitting Game...");
        #endif
        }
    }

    void MoveRight()
    {
        if (option < options.Length - 1)
        {
            option++;
        }
    }

    void MoveLeft()
    {
        if (option > 0)
        {
            option--;
        }
    }

    // Coroutine to handle the delayed scene load 
    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        if (audioSource != null && sceneSelectClip != null)
        {
            audioSource.PlayOneShot(sceneSelectClip);
            yield return new WaitForSeconds(0.5f);
        }
        
        // FIX: Hide the UI by disabling the Canvas component
        if (menuCanvas != null) menuCanvas.enabled = false; 

        // Stop the current timer before loading the new scene
        StopAllCoroutines(); 
        
        // No need to set GameObject.SetActive(false) anymore!
        SceneManager.LoadScene(sceneName);
    }

    // =======================================================
    // --- AUTOMATION LOGIC ---
    // =======================================================
    
    IEnumerator AutoCycleScenes(float delay)
    {
        // Loop forever, changing scenes after the delay
        while (true)
        {
            // 1. Wait for the specified time (e.g., 5 seconds)
            yield return new WaitForSeconds(delay); 

            // 2. Load the next scene in the cycle
            string sceneToLoad = cycleScenes[currentCycleIndex];

            // 3. Load the scene (this calls LoadSceneWithDelay, which stops this coroutine)
            yield return StartCoroutine(LoadSceneWithDelay(sceneToLoad));

            // 4. Move to the next scene index, wrapping around
            currentCycleIndex = (currentCycleIndex + 1) % cycleScenes.Length;
        }
    }
}