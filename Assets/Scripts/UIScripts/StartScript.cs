using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public string sceneName = "GameSceneNameHere";

    [SerializeField]
    GameObject gameManagerObject;

    bool overwrite = false;

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

    public void StartNewGame()
    {
        if(SaveManager.instance.fileDataHandler.Load() != null && !overwrite)
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Save found. Give up on them?", 1, 2);
            overwrite = true;
            return;
        }
        SaveManager.instance.NewGame();
        LoadScene();
    }

    public void Continue()
    {
        if(SaveManager.instance.fileDataHandler.Load() != null)
        {
            Debug.Log("Loading in game");
            SaveManager.instance.LoadGame();
            LoadScene();
        }
        else
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("No save file available.", 1, 3);
        }
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
