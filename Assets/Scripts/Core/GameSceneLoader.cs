using UnityEngine;

public class GameSceneLoader : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.MainMenu)
        {
            GameManager.Instance.StartGame();
        }
    }
}
