using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{

    public GameObject canvas1;
    public GameObject canvas2;
    

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void EnableCredits()
    {
        canvas1.SetActive(false);
        canvas2.SetActive(true);
    }
    
    public void EnableMenu()
    {
        canvas1.SetActive(true);
        canvas2.SetActive(false);
    }
    

    public void ExitGame()
    {
        Application.Quit();
        
    }    
    
}
