using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroInfo : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
