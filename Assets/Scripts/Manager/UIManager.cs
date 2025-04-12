using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(EventTrigger))]
public class UIManager : MonoBehaviour
{
	[System.Serializable]
	public class ButtonSetup
	{
		public Image buttonImage;
		public Sprite normalSprite;
		public Sprite pressedSprite;
		[HideInInspector] public Button buttonComponent;
	}

	public ButtonSetup startButton;
	public ButtonSetup scoreButton;
	public ButtonSetup exitButton;
	public ButtonSetup backButton;
	public ButtonSetup confirmButton;
	public GameObject nameInputPanel;
	public TMP_InputField nameInputField;
	public GameObject mainMenuPanel;
	private const int MAX_NAME_LENGTH = 20;

	private void Start()
	{
		nameInputPanel.SetActive(false);
		mainMenuPanel.SetActive(true);

		SetupButton(startButton, ShowNameInput);
		SetupButton(scoreButton, ShowScore);
		SetupButton(exitButton, ExitGame);
		SetupButton(backButton, BackToMainMenu);
		SetupButton(confirmButton, ConfirmName);

		confirmButton.buttonComponent.interactable = false;
		confirmButton.buttonImage.sprite = confirmButton.normalSprite;
		nameInputField.characterLimit = MAX_NAME_LENGTH;
		nameInputField.onValueChanged.AddListener(ValidateNameInput);
	}

	private void SetupButton(ButtonSetup buttonSetup, UnityEngine.Events.UnityAction action)
	{
		if (buttonSetup.buttonImage == null) return;

		buttonSetup.buttonComponent = buttonSetup.buttonImage.GetComponent<Button>() ??
								   buttonSetup.buttonImage.gameObject.AddComponent<Button>();

		buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;
		buttonSetup.buttonComponent.onClick.AddListener(action);

		var eventTrigger = buttonSetup.buttonImage.gameObject.GetComponent<EventTrigger>() ??
						 buttonSetup.buttonImage.gameObject.AddComponent<EventTrigger>();

		var pointerDown = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerDown,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerDown.callback.AddListener((data) => {
			if (buttonSetup.buttonComponent.interactable)
				buttonSetup.buttonImage.sprite = buttonSetup.pressedSprite;
		});

		var pointerUp = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerUp,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerUp.callback.AddListener((data) => {
			buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;
		});

		var pointerExit = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerExit.callback.AddListener((data) => {
			buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;
		});

		eventTrigger.triggers.Clear();
		eventTrigger.triggers.Add(pointerDown);
		eventTrigger.triggers.Add(pointerUp);
		eventTrigger.triggers.Add(pointerExit);
	}

	private void ValidateNameInput(string input)
	{
		bool isValid = !string.IsNullOrWhiteSpace(input) && input.Length <= MAX_NAME_LENGTH;
		confirmButton.buttonComponent.interactable = isValid;
		confirmButton.buttonImage.sprite = isValid ?
			confirmButton.normalSprite : confirmButton.normalSprite;
	}

	private void ShowNameInput()
	{
		mainMenuPanel.SetActive(false);
		nameInputPanel.SetActive(true);
		nameInputField.text = "";
	}

	private void ConfirmName()
	{
		string playerName = nameInputField.text.Trim();
		if (!string.IsNullOrEmpty(playerName))
		{
			GameManager.Instance.SetPlayerName(playerName);
			LoadGameScene();
		}
	}

	private void BackToMainMenu()
	{
		nameInputPanel.SetActive(false);
		mainMenuPanel.SetActive(true);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene(4);
	}

	private void ShowScore()
	{
		SceneManager.LoadScene(3);
	}

	private void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}