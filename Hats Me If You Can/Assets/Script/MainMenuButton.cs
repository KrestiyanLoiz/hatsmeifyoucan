using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;

    [Tooltip("Animation type: Expand or Spin")]
    public string hoverAnimationType = "Expand"; // or "Spin"

    [Tooltip("Function to perform: NewGame, LoadGame, ShowSettings, ShowCredits, ShowMainMenu, Exit")]
    public string buttonFunction;

    [Header("Optional Panel Targets")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject loadGamePanel;
    public GameObject gameSceneLoader;  

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator == null) return;

        switch (hoverAnimationType)
        {
            case "Expand":
                animator.Play("HoverExpand", 0, 0);
                break;
            case "Spin":
                animator.Play("SpinAnimation", 0, 0);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator == null) return;

        switch (hoverAnimationType)
        {
            case "Expand":
                animator.Play("HoverShrink", 0, 0);
                break;
            case "Spin":
                animator.Play("Idle", 0, 0); // or a reset rotation animation
                break;
        }
    }

    public void OnClick()
    {
        switch (buttonFunction)
        {
            case "NewGame":
                SceneManager.LoadScene(1);
                Debug.Log("New Game Started");
                break;

            case "LoadGame":
                TogglePanels(loadGamePanel);
                break;

            case "ShowSettings":
                TogglePanels(settingsPanel);
                break;

            case "ShowCredits":
                TogglePanels(creditsPanel);
                break;

            case "ShowMainMenu":
                TogglePanels(mainMenuPanel);
                break;

            case "Exit":
                Debug.Log("Game Closed");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    private void TogglePanels(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);
    }
}
