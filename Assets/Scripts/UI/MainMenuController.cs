using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button researchButton;
    [SerializeField] private Button periodicTableButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button compendiumButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (researchButton != null)
            researchButton.onClick.AddListener(() => SceneManager.LoadScene("ResearchTree"));
        if (periodicTableButton != null)
            periodicTableButton.onClick.AddListener(() => SceneManager.LoadScene("PeriodicTable"));
        if (exploreButton != null)
            exploreButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelect"));
        if (compendiumButton != null)
            compendiumButton.onClick.AddListener(() => SceneManager.LoadScene("Compendium"));
        if (upgradesButton != null)
            upgradesButton.onClick.AddListener(() => SceneManager.LoadScene("Upgrades"));
        if (quitButton != null)
            quitButton.onClick.AddListener(() => Application.Quit());
    }
}