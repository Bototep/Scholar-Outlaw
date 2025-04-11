using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PlayerScore
{
	public string playerName;
	public int score;
}

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	public string PlayerName { get; private set; }
	private List<PlayerScore> scoreboard = new List<PlayerScore>();
	private const string SAVE_FILE = "scoreboard.json";

	void Awake()
	{
		transform.SetParent(null);

		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			LoadScores();
			Application.targetFrameRate = 140;
			QualitySettings.vSyncCount = 0;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			scoreboard.Clear();
			SaveScores();
		}
	}

	public void SetPlayerName(string name)
	{
		PlayerName = name;
	}

	public void SaveScore(int score)
	{
		scoreboard.Add(new PlayerScore
		{
			playerName = PlayerName,
			score = score
		});
		SaveScores();
	}

	private void SaveScores()
	{
		string json = JsonUtility.ToJson(new ScoreboardWrapper { scores = scoreboard });
		string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
		File.WriteAllText(path, json);
	}

	private void LoadScores()
	{
		string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			scoreboard = JsonUtility.FromJson<ScoreboardWrapper>(json).scores;
		}
	}

	public List<PlayerScore> GetScoreboard()
	{
		return new List<PlayerScore>(scoreboard);
	}

	[System.Serializable]
	private class ScoreboardWrapper
	{
		public List<PlayerScore> scores;
	}
}