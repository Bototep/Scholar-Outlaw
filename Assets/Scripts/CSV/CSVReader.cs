using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class CSVReader : MonoBehaviour
{
	public TextAsset csvFile;

	[ContextMenu("Load CSV Data")]
	public void LoadCSVData()
	{
#if UNITY_EDITOR
		if (csvFile == null)
		{
			Debug.LogError("CSV file is missing!");
			return;
		}

		string[] lines = csvFile.text.Split('\n');
		string itemFolderPath = "Assets/Items";

		if (!Directory.Exists(itemFolderPath))
			Directory.CreateDirectory(itemFolderPath);

		HashSet<string> validItemNames = new HashSet<string>();

		for (int i = 1; i < lines.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(lines[i]))
				continue;

			string[] columns = lines[i].Split(',');
			if (columns.Length < 4)
				continue;

			string itemName = columns[0].Trim();
			validItemNames.Add(itemName);
		}

		CleanUnusedItems(itemFolderPath, validItemNames);

		for (int i = 1; i < lines.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(lines[i]))
				continue;

			string[] columns = lines[i].Split(',');
			if (columns.Length < 4)
				continue;

			string itemName = columns[0].Trim();
			string assetPath = $"{itemFolderPath}/{itemName}.asset";

			ItemData itemData = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

			if (itemData == null)
			{
				itemData = ScriptableObject.CreateInstance<ItemData>();
				UnityEditor.AssetDatabase.CreateAsset(itemData, assetPath);
			}

			itemData.itemName = itemName;
			itemData.width = int.Parse(columns[1].Trim());
			itemData.height = int.Parse(columns[2].Trim());
			itemData.cost = int.Parse(columns[3].Trim());

			UnityEditor.EditorUtility.SetDirty(itemData);
		}

		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
		Debug.Log($"Successfully processed {validItemNames.Count} items");
#endif
	}

	private void CleanUnusedItems(string folderPath, HashSet<string> validNames)
	{
#if UNITY_EDITOR
		string[] existingItems = Directory.GetFiles(folderPath, "*.asset");

		foreach (var itemPath in existingItems)
		{
			string fileName = Path.GetFileNameWithoutExtension(itemPath);
			if (!validNames.Contains(fileName))
			{
				UnityEditor.AssetDatabase.DeleteAsset(itemPath);
			}
		}
#endif
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(CSVReader))]
public class CSVReaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		CSVReader reader = (CSVReader)target;

		EditorGUILayout.Space();

		GUI.enabled = reader.csvFile != null;
		if (GUILayout.Button("Load CSV Data", GUILayout.Height(30)))
		{
			reader.LoadCSVData();
		}
		GUI.enabled = true;

		if (reader.csvFile == null)
		{
			EditorGUILayout.HelpBox("Assign a CSV file to begin", MessageType.Warning);
		}
	}
}
#endif