using UnityEngine;

public class CloseGameScript : MonoBehaviour
{
    public void CloseGame()
    {
        Debug.Log("Game is closing..."); // Logs a message in the editor
        Application.Quit(); // Closes the application
    }
}
