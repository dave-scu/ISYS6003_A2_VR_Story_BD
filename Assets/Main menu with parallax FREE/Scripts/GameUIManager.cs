using UnityEngine;

// This is the main class for managing UI interactions like quitting
public class GameUIManager : MonoBehaviour
{
    // Assign your "Are you sure you want to quit?" panel here in the Inspector.
    // This allows the script to show and hide the entire confirmation box.
    public GameObject quitConfirmationPanel;

    // --- Core Functions for Buttons ---

    /**
     * Called by the main 'Exit Game' button to open the confirmation panel.
     */
    public void OpenQuitConfirmation()
    {
        // Check if the reference is set and activate the panel
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(true);
            // Optional: Pause the game when the panel is up
            // Time.timeScale = 0f; 
            Debug.Log("Quit Confirmation Panel opened.");
        }
        else
        {
            Debug.LogError("Quit Confirmation Panel is not assigned in the Inspector!");
        }
    }

    /**
     * Called by the 'Cancel' button to close the confirmation panel.
     */
    public void CancelQuit()
    {
        // Check if the reference is set and deactivate the panel
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
            // Optional: Resume the game time
            // Time.timeScale = 1f; 
            Debug.Log("Quit Confirmation Panel closed (Canceled).");
        }
    }

    /**
     * Called by the 'Exit' button to close the panel and quit the application.
     */
    public void ExitApplication()
    {
        // 1. Optional: Close the panel immediately
        CancelQuit();
        
        // 2. Quit the application
        Debug.Log("Exiting Application...");

        // NOTE: Application.Quit() only works in a built game (Windows, Mac, etc.).
        // It is ignored in the Unity Editor.
        Application.Quit();

        // If you need to stop play in the Editor during testing:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
