using UnityEngine;
using System.Collections;
using static SoundManager;
using TMPro;

public class CountDown : MonoBehaviour
{
	public TMP_Text timeText; 
	public float totalTime = 600f; 
	public PlayerMovement playerMovement;
	private float currentTime;
	private bool isRunning = false;

	void Start()
	{
		currentTime = totalTime;
		UpdateTimeDisplay();
		StartCountdown();
	}

	public void StartCountdown()
	{
		if (!isRunning)
		{
			isRunning = true;
			StartCoroutine(CountdownRoutine());
		}
	}

	public void StopCountdown()
	{
		isRunning = false;
		StopAllCoroutines();
	}

	private IEnumerator CountdownRoutine()
	{
		while (currentTime > 0 && isRunning)
		{
			yield return new WaitForSeconds(1f);
			currentTime -= 1f;
			UpdateTimeDisplay();

			if (currentTime <= 0)
			{
				TimeExpired();
			}
		}
	}

	private void UpdateTimeDisplay()
	{
		int minutes = Mathf.FloorToInt(currentTime / 60f);
		int seconds = Mathf.FloorToInt(currentTime % 60f);
		timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	private void TimeExpired()
	{
		timeText.text = "00:00";
		if (SoundManager.Instance != null)
		{
			SoundManager.Instance.Play(SoundType.Death);
		}
		playerMovement.LoadGameOverScene(true);
	}

	public void PauseCountdown()
	{
		isRunning = false;
	}

	public void ResumeCountdown()
	{
		if (currentTime > 0)
		{
			StartCountdown();
		}
	}
}