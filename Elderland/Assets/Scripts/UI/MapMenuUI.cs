using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapMenuUI : StartMenuUI
{
    [SerializeField]
    private GameObject teleporterParent;
    [SerializeField]
    private Color currentLocationButtonColor;
    [SerializeField]
    private RectTransform playerPosition;
    [SerializeField]
    private RectTransform UIPoint1Transform;
    [SerializeField]
    private RectTransform UIPoint2Transform;
    [SerializeField]
    private Transform worldPoint1Transform;
    [SerializeField]
    private Transform worldPoint2Transform;
    [SerializeField]
    private GameObject selectedTeleporter;

    private Button[] teleporterButtons;

    private bool tabSwapEnabled;

    private Button currentLocationButton;
    private ColorBlock currentLocationButtonColorblock;

    private Vector2 worldToUIFactors;

    private bool initialized;

    public Button CurrentLocationButton { get { return currentLocationButton; } }

    private void Initialize()
    {
        if (!initialized)
        {
            teleporterButtons =
                teleporterParent.GetComponentsInChildren<Button>();
            tabSwapEnabled = true;
            initialized = true;
        }
    }

    protected override void OnEnable()
    {
        Initialize();
        StartCoroutine(DefaultSelectedCoroutine());

        CalculateCoordinateConversion();
        playerPosition.anchoredPosition =
            WorldToUIPosition(PlayerInfo.Player.transform.position);
        Vector3 playerEulerAngles =
            PlayerInfo.Player.transform.rotation.eulerAngles;
        playerPosition.localRotation =
            Quaternion.Euler(0, 0, -playerEulerAngles.y);
    }

    public void DisableTeleporters()
    {
        Initialize();
        foreach (Button button in teleporterButtons)
        {
            button.interactable = false;
        }
        tabSwapEnabled = true;
        selectedTeleporter.SetActive(false);
    }

    public void EnableTeleporters()
    {
        Initialize();
        foreach (Button button in teleporterButtons)
        {
            button.interactable = true;
        }

        tabSwapEnabled = false;
        selectedTeleporter.SetActive(true);
    }

    public override void DisableMenu()
    {
        transform.parent.gameObject.SetActive(false);

        GameInfo.Paused = false;
        Time.timeScale = 1;
        GameInfo.Manager.OverlayUnfreezeInput();

        // Deselect active button to have highlight by default when turning on
        eventSystem.SetSelectedGameObject(null);

        gameplayUI.SetActive(true);

        DisableTeleporters();
        ResetCurrentLocationButton();
    }

    public void DelayDisableMenu()
    {
        StartCoroutine(DelayDisableMenu(1f));
    }

    private IEnumerator DelayDisableMenu(float duration)
    {
        // Deselect active button to have highlight by default when turning on
        eventSystem.SetSelectedGameObject(null);

        yield return new WaitForSecondsRealtime(duration);

        transform.parent.gameObject.SetActive(false);

        GameInfo.Paused = false;
        Time.timeScale = 1;
        //GameInfo.Manager.OverlayUnfreezeInput();

        gameplayUI.SetActive(true);

        DisableTeleporters();
        ResetCurrentLocationButton();
    }

    protected override void TabSwap()
    {
        if (tabSwapEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button4) &&
                leftNeighborMenu != null)
            {
                transform.gameObject.SetActive(false);
                leftNeighborMenu.gameObject.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.Joystick1Button5) &&
                    rightNeighborMenu != null)
            {
                transform.gameObject.SetActive(false);
                rightNeighborMenu.gameObject.SetActive(true);
            }
        }
    }

    public void SetCurrentLocationButton(Button button)
    {
        currentLocationButtonColorblock = button.colors;
        currentLocationButton = button;
        ColorBlock newColors = button.colors;
        newColors.pressedColor = currentLocationButtonColor;
        newColors.selectedColor = currentLocationButtonColor;
        newColors.normalColor = 
            new Color(
                    currentLocationButtonColor.r * 0.70f,
                    currentLocationButtonColor.g * 0.70f,
                    currentLocationButtonColor.b * 0.70f,
                    1f);
        currentLocationButton.colors = newColors;
    }

    private void ResetCurrentLocationButton()
    {
        if (currentLocationButton != null)
        {
            currentLocationButton.colors = currentLocationButtonColorblock;
            currentLocationButton = null;
        }
    }

    public void CalculateCoordinateConversion()
    {
        worldToUIFactors =
            new Vector2(
                (UIPoint2Transform.anchoredPosition.x - UIPoint1Transform.anchoredPosition.x) /
                (worldPoint2Transform.position.x - worldPoint1Transform.position.x),
                (UIPoint2Transform.anchoredPosition.y - UIPoint1Transform.anchoredPosition.y) /
                (worldPoint2Transform.position.z - worldPoint1Transform.position.z)
            );
    }

    public Vector2 WorldToUIPosition(Vector3 worldPosition)
    {
        Vector2 deltaPosition =
            new Vector2(
                worldPosition.x - worldPoint1Transform.position.x,
                worldPosition.z - worldPoint1Transform.position.z);
        
        Vector2 UIDeltaPosition = 
            new Vector2(
                deltaPosition.x * worldToUIFactors.x,
                deltaPosition.y * worldToUIFactors.y);
        
        return UIPoint1Transform.anchoredPosition + UIDeltaPosition;
    }
}
