using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Menu,
    Gameplay,
    End
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Références principales")]
    public Camera menuCamera;
    public Camera gameplayCamera;
    public Camera endCamera;
    public GameObject player;
    public GameObject uiMenu;
    public GameObject uiEnd;

    [Header("Boutons UI")]
    public Button playButton;
    public Button quitButton;

    [Header("État du jeu actuel")]
    public GameState currentState = GameState.Menu;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Initialisation de base
        SwitchState(GameState.Menu);

        // Assignation des boutons
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Exemple de condition de fin de jeu (à adapter)
        if (currentState == GameState.Gameplay && player.transform.position.y < -10f)
            SwitchState(GameState.End);
    }

    public void StartGame()
    {
        SwitchState(GameState.Gameplay);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwitchState(GameState newState)
    {
        currentState = newState;

        // Désactivation des caméras et UIs
        menuCamera.gameObject.SetActive(false);
        gameplayCamera.gameObject.SetActive(false);
        endCamera.gameObject.SetActive(false);
        uiMenu.SetActive(false);
        uiEnd.SetActive(false);

        switch (currentState)
        {
            case GameState.Menu:
                menuCamera.gameObject.SetActive(true);
                uiMenu.SetActive(true);
                break;

            case GameState.Gameplay:
                gameplayCamera.gameObject.SetActive(true);
                Cursor.visible = false;
                break;

            case GameState.End:
                endCamera.gameObject.SetActive(true);
                uiEnd.SetActive(true);
                Cursor.visible = true;
                break;
        }
    }
}
