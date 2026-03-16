using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCompletedController : MonoBehaviour
{
    public void OnReturnToMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
