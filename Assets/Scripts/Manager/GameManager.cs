using UnityEngine;

public class GameManager : MonoBehaviour
{
	void Awake()
	{
		Application.targetFrameRate = 140; // Set FPS to 60
		QualitySettings.vSyncCount = 0; // Disable VSync to enforce target FPS
	}
}
