using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSelect : MonoBehaviour
{
    public void Forest()
    {
        SceneManager.LoadScene("ForestWorld");
    }
    public void Cave()
    {
        SceneManager.LoadScene("CaveWorld");
    }
    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
