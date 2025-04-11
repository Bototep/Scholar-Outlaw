using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighScoreManager : MonoBehaviour
{
	public TMP_Text[] nameTexts = new TMP_Text[7];
	public TMP_Text[] scoreTexts = new TMP_Text[7];

	public Button backButton;

	void Start()
	{
		backButton.onClick.AddListener(ReturnToMainMenu);

		DisplayHighScores();
	}

	void DisplayHighScores()
	{
		List<PlayerScore> scores = GameManager.Instance.GetScoreboard();
		scores.Sort((a, b) => b.score.CompareTo(a.score));

		int count = Mathf.Min(scores.Count, 7);
		for (int i = 0; i < count; i++)
		{
			nameTexts[i].text = scores[i].playerName;
			scoreTexts[i].text = scores[i].score.ToString();
		}

		for (int i = count; i < 7; i++)
		{
			nameTexts[i].text = "-";
			scoreTexts[i].text = "-";
		}
	}

	void ReturnToMainMenu()
	{
		SceneManager.LoadScene(0);
	}
}