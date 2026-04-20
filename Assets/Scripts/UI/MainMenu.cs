using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Explore()
    {
        SceneManager.LoadScene("WorldSelect");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}