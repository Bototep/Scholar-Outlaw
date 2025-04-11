using UnityEngine;
using TMPro;
using System.Linq;

public class ScoreboardUI : MonoBehaviour
{
	public TMP_Text scoreboardText;
	public int maxScoresToShow = 10;

	void Start()
	{
		DisplayScores();
	}

	public void DisplayScores()
	{
		var scores = GameManager.Instance.GetScoreboard()
			.OrderByDescending(s => s.score)
			.Take(maxScoresToShow);

		string scoreText = "TOP SCORES:\n";
		int rank = 1;

		foreach (var score in scores)
		{
			scoreText += $"{rank}. {score.playerName}: {score.score}\n";
			rank++;
		}

		scoreboardText.text = scoreText;
	}
}