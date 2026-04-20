using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button researchButton;
    [SerializeField] private Button periodicTableButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button constructButton;

    private void Start()
    {
        if (researchButton != null)
            researchButton.onClick.AddListener(() => SceneManager.LoadScene("ResearchTree"));
        if (periodicTableButton != null)
            periodicTableButton.onClick.AddListener(() => SceneManager.LoadScene("PeriodicTable"));
        if (exploreButton != null)
            exploreButton.onClick.AddListener(() => SceneManager.LoadScene("Explore"));
        if (constructButton != null)
            constructButton.onClick.AddListener(() => SceneManager.LoadScene("Construction"));
    }
}
