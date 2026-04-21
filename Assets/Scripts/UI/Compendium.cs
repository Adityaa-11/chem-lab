using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Compendium : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
}
