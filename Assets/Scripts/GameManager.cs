using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    [Header("Références")]
    public Camera menuCamera;
    public Camera gameplayCamera;
    public Camera endCamera;
    public GameObject player;
    public GameObject uiMenu;
    public GameObject uiEnd;
    public GameObject uiPause;
    public PlayerInventory playerInventory;
    public AudioSource moteurPlayer;
    public PlayerInput playerInput;
    private InputAction pauseKey;

    [Header("État du jeu actuel")]
    public GameState currentState = GameState.Menu;

    private bool isPause;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseKey = playerInput.actions["Pause"];

        SwitchState(GameState.Menu);
    }

    void Update()
    {
        if (currentState == GameState.Gameplay && playerInventory.coinCount >= 12)
        {
            SwitchState(GameState.End);
            moteurPlayer.volume = 0; 
        }

        if (pauseKey.triggered)
        {
            PauseGame();
        }
    }

    public void StartGame()
    {
        SwitchState(GameState.Gameplay);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void PauseGame()
    {
        isPause = !isPause;

        if (isPause)
        {
            uiPause.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
        }
        else
        {
            uiPause.SetActive(false);
            Time.timeScale = 1;
            Cursor.visible = false;

        }
    }

    public void SwitchState(GameState newState)
    {
        currentState = newState;

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
