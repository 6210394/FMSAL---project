using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public string sceneName = "GameSceneNameHere";

    [SerializeField]
    GameObject gameManagerObject;

    void Start()
    {
        CreateGameManager();
        SetCursorState(true, CursorLockMode.None);
    }

    public void LoadScene()
    {
        Debug.Log("Loading Scene: " + sceneName);

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CreateGameManager()
    {
        if (GameManager.instance == null)
        {
            Instantiate(gameManagerObject);
        }
    }

    public void SetCursorState(bool visible, CursorLockMode lockMode)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }
}
