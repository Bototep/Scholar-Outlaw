using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(EventTrigger))]
public class UIManager : MonoBehaviour
{
	[System.Serializable]
	public class ButtonSetup
	{
		public Image buttonImage;
		public Sprite normalSprite;
		public Sprite pressedSprite;
	}

	public ButtonSetup startButton;
	public ButtonSetup scoreButton;
	public ButtonSetup exitButton;

	void Start()
	{
		SetupButton(startButton, LoadGameScene);
		SetupButton(scoreButton, ShowScore); // Empty for now
		SetupButton(exitButton, ExitGame);
	}

	void SetupButton(ButtonSetup buttonSetup, UnityEngine.Events.UnityAction action)
	{
		if (buttonSetup.buttonImage == null) return;

		// Set initial sprite
		buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;

		// Add EventTrigger if not exists
		var eventTrigger = buttonSetup.buttonImage.gameObject.GetComponent<EventTrigger>() ??
						 buttonSetup.buttonImage.gameObject.AddComponent<EventTrigger>();

		// Create pointer down event
		var pointerDown = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerDown,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerDown.callback.AddListener((data) => {
			buttonSetup.buttonImage.sprite = buttonSetup.pressedSprite;
		});

		// Create pointer up event
		var pointerUp = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerUp,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerUp.callback.AddListener((data) => {
			buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;
		});

		// Create pointer exit event
		var pointerExit = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit,
			callback = new EventTrigger.TriggerEvent()
		};
		pointerExit.callback.AddListener((data) => {
			buttonSetup.buttonImage.sprite = buttonSetup.normalSprite;
		});

		// Clear existing triggers and add new ones
		eventTrigger.triggers.Clear();
		eventTrigger.triggers.Add(pointerDown);
		eventTrigger.triggers.Add(pointerUp);
		eventTrigger.triggers.Add(pointerExit);

		// Add click functionality
		var button = buttonSetup.buttonImage.gameObject.GetComponent<Button>() ??
					buttonSetup.buttonImage.gameObject.AddComponent<Button>();
		button.onClick.AddListener(action);
	}

	void LoadGameScene()
	{
		SceneManager.LoadScene(1); // Load scene with index 1
	}

	void ShowScore()
	{
		// Leave this empty as requested
		Debug.Log("Score button pressed - functionality not implemented");
	}

	void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}