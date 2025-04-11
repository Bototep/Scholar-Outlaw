using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SummaryUIManager : MonoBehaviour
{
	public Button confirmButton;
	public TMP_Text nameText;
	public TMP_Text scoreText;
	public float countDuration = 2f;

	private int targetScore;

	private void Start()
	{
		if (confirmButton != null)
		{
			confirmButton.onClick.AddListener(LoadMainMenu);
			confirmButton.interactable = false;
		}

		if (GameManager.Instance != null)
		{
			var scores = GameManager.Instance.GetScoreboard();
			if (scores.Count > 0)
			{
				var latestScore = scores[scores.Count - 1];
				nameText.text = $"{latestScore.playerName}";
				targetScore = latestScore.score;
				StartCoroutine(CountScoreUp());
			}
			else
			{
				nameText.text = "No scores yet!";
				scoreText.text = "0";
				confirmButton.interactable = true;
			}
		}
		else
		{
			nameText.text = "Game Manager not found";
			scoreText.text = "0";
			confirmButton.interactable = true;
		}
	}

	IEnumerator CountScoreUp()
	{
		float progress = 0f;
		int currentScore = 0;
		scoreText.text = "0";

		while (progress < 1f)
		{
			progress += Time.deltaTime / countDuration;
			currentScore = Mathf.FloorToInt(Mathf.Lerp(0, targetScore, progress));
			scoreText.text = currentScore.ToString();
			yield return null;
		}

		scoreText.text = targetScore.ToString();

		if (confirmButton != null)
		{
			confirmButton.interactable = true;
		}
	}

	public void LoadMainMenu()
	{
		SceneManager.LoadScene(0);
	}

	private void OnDestroy()
	{
		if (confirmButton != null)
		{
			confirmButton.onClick.RemoveListener(LoadMainMenu);
		}
	}
}