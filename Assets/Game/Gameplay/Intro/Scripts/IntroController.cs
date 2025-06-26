using UnityEngine;

public class IntroController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private GameplayUI gameplayUI = null;
    [SerializeField] private Dog dog = null;
    [SerializeField] private AudioEvent musicEvent = null;

    private void Start()
    {
        playerController.Init(ToggleOnPause, gameplayUI.UpdatePlayerHealth, null);
        gameplayUI.Init(ToggleTimeScale, ToggleOffPause);
        dog.onTriggered = () => gameplayUI.OpenDialog();

        GameManager.Instance.AudioManager.PlayAudio(musicEvent);
    }

    private void ToggleOnPause()
    {
        gameplayUI.TogglePause(true);
        ToggleTimeScale(false);
    }

    private void ToggleOffPause()
    {
        ToggleTimeScale(true);
        playerController.TogglePause(false);
    }

    private void ToggleTimeScale(bool status)
    {
        Time.timeScale = status ? 1f : 0f;
    }
}
