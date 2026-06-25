using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public void Start1P() { SceneManager.LoadScene("SampleScene"); }
    //Since Erik holds responsible for working on the game programming, the "SampleScene" scene will change its name once the single player features are complete.
    public void Start2P() { SceneManager.LoadScene("_MultiplayerLobby"); }
    public void QuitGame() { Application.Quit(); }
}
