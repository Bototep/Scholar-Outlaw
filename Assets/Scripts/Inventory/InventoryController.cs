using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
	[SerializeField] private GameObject inventoryPanel;
	public static InventoryController Instance { get; private set; }
	public TMP_Text scoreText;

	[HideInInspector]
	private ItemGrid selectedItemGrid;

	public ItemGrid SelectedItemGrid
	{
		get => selectedItemGrid;
		set
		{
			selectedItemGrid = value;
			inventoryHighlight.SetParent(value);
		}
	}

	public InventoryItem selectedItem;
	InventoryItem overlapItem;
	public RectTransform rectTransform;

	[SerializeField] List<ItemData> items;
	[SerializeField] public GameObject itemPrefab;
	[SerializeField] public Transform canvasTransform;

	InventoryHighlight inventoryHighlight;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}

		inventoryHighlight = GetComponent<InventoryHighlight>();
	}

	private void Update()
	{
		ItemIconDrag();

		if (Input.GetKeyDown(KeyCode.X) && selectedItem != null)
		{
			Destroy(selectedItem.gameObject);
			selectedItem = null;
			return;
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			RotateItem();
		}

		if (selectedItemGrid == null)
		{
			inventoryHighlight.Show(false);
			return;
		}

		HandleHighlight();

		if (Input.GetMouseButtonDown(0))
		{
			if (selectedItem != null)
			{
				Vector2Int gridPos = GetTileGridPosition();

				bool mouseInGrid = IsMouseInGridBounds();

				if (!mouseInGrid)
				{
					Destroy(selectedItem.gameObject);
					selectedItem = null;
				}
				else if (selectedItemGrid.BoundryCheck(gridPos.x, gridPos.y,
						 selectedItem.WIDTH, selectedItem.HEIGHT))
				{
					LeftMouseButtonPress();
				}
			}
			else
			{
				LeftMouseButtonPress();
			}
		}
	}

	private bool IsMouseInGridBounds()
	{
		if (selectedItemGrid == null) return false;

		Vector2 mousePosition = Input.mousePosition;
		RectTransform gridRect = selectedItemGrid.GetComponent<RectTransform>();

		Vector2 localMousePosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			gridRect, mousePosition, null, out localMousePosition);

		return gridRect.rect.Contains(localMousePosition);
	}

	private void RotateItem()
	{
		if (selectedItem == null) return;
		selectedItem.Rotate();
	}

	private void InsertRandomItem()
	{
		if (selectedItemGrid == null) return;
		CreateRandomItem();
		InventoryItem itemToInsert = selectedItem;
		selectedItem = null;
		InsertItem(itemToInsert);
	}

	public void InsertItem(InventoryItem itemToInsert)
	{
		Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
		if (posOnGrid == null) return;
		selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
		UpdateScoreText();
	}

	Vector2Int oldPosition;
	InventoryItem itemToHighlight;
	private void HandleHighlight()
	{
		Vector2Int positionOnGrid = GetTileGridPosition();
		if (oldPosition == positionOnGrid) return;

		oldPosition = positionOnGrid;
		if (selectedItem == null)
		{
			itemToHighlight = (InventoryItem)selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

			if (itemToHighlight != null)
			{
				inventoryHighlight.Show(true);
				inventoryHighlight.SetSize(itemToHighlight);
				inventoryHighlight.SetPostion(selectedItemGrid, itemToHighlight);
			}
			else
			{
				inventoryHighlight.Show(false);
			}
		}
		else
		{
			inventoryHighlight.Show(selectedItemGrid.BoundryCheck(
				positionOnGrid.x,
				positionOnGrid.y,
				selectedItem.WIDTH,
				selectedItem.HEIGHT));
			inventoryHighlight.SetSize(selectedItem);
			inventoryHighlight.SetPostion(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
		}
	}

	private void CreateRandomItem()
	{
		InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
		selectedItem = inventoryItem;
		rectTransform = inventoryItem.GetComponent<RectTransform>();
		rectTransform.SetParent(canvasTransform);
		rectTransform.SetAsLastSibling();
		int selectedItemID = UnityEngine.Random.Range(0, items.Count);
		inventoryItem.Set(items[selectedItemID]);
	}

	public bool AddItemToInventory(ItemData itemData)
	{
		InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
		rectTransform = inventoryItem.GetComponent<RectTransform>();
		rectTransform.SetParent(canvasTransform);
		rectTransform.SetAsLastSibling();
		inventoryItem.Set(itemData);
		selectedItem = inventoryItem;
		InsertItem(inventoryItem);
		if (inventoryItem.transform.parent == null)
		{
			Destroy(inventoryItem.gameObject);
			return false;
		}
		return true;
	}

	private void LeftMouseButtonPress()
	{
		Vector2Int tileGridPosition = GetTileGridPosition();
		if (selectedItem == null)
		{
			PickUpItem(tileGridPosition);
		}
		else
		{
			if (selectedItemGrid.BoundryCheck(tileGridPosition.x, tileGridPosition.y,
				selectedItem.WIDTH, selectedItem.HEIGHT))
			{
				PlaceItem(tileGridPosition);
			}
		}
		UpdateScoreText();
	}

	private Vector2Int GetTileGridPosition()
	{
		return selectedItemGrid.GetTileGridPosition(Input.mousePosition, selectedItem);
	}

	private void PlaceItem(Vector2Int tileGridPosition)
	{
		if (!selectedItemGrid.BoundryCheck(tileGridPosition.x, tileGridPosition.y,
			selectedItem.WIDTH, selectedItem.HEIGHT))
		{
			return;
		}

		bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x,
			tileGridPosition.y, ref overlapItem);
		if (complete)
		{
			selectedItem = null;
			if (overlapItem != null)
			{
				selectedItem = overlapItem;
				overlapItem = null;
				rectTransform = selectedItem.GetComponent<RectTransform>();
				rectTransform.SetAsLastSibling();
			}
		}
		UpdateScoreText();
	}

	private void PickUpItem(Vector2Int tileGridPosition)
	{
		selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
		if (selectedItem != null)
		{
			rectTransform = selectedItem.GetComponent<RectTransform>();
		}
		UpdateScoreText();
	}

	public void ToggleInventory(bool show)
	{
		if (inventoryPanel != null)
		{
			inventoryPanel.SetActive(show);
			if (show) UpdateScoreText();
		}
	}

	private void ItemIconDrag()
	{
		if (selectedItem != null)
		{
			rectTransform.position = Input.mousePosition;
		}
	}

	public void CloseInventory()
	{
		if (inventoryPanel != null)
		{
			if (selectedItem != null)
			{
				Destroy(selectedItem.gameObject);
				selectedItem = null;
			}
			inventoryPanel.SetActive(false);
		}
	}

	public void CalculateTotalInventoryValue()
	{
		if (selectedItemGrid == null) return;

		int totalValue = 0;
		for (int x = 0; x < selectedItemGrid.gridSizeWidth; x++)
		{
			for (int y = 0; y < selectedItemGrid.gridSizeHeight; y++)
			{
				InventoryItem item = selectedItemGrid.GetItem(x, y) as InventoryItem;
				if (item != null && item.itemData != null)
				{
					if (item.onGridPositionX == x && item.onGridPositionY == y)
					{
						totalValue += item.itemData.cost;
					}
				}
			}
		}
	}

	private void UpdateScoreText()
	{
		if (scoreText == null) return;

		int totalValue = 0;
		if (selectedItemGrid != null)
		{
			for (int x = 0; x < selectedItemGrid.gridSizeWidth; x++)
			{
				for (int y = 0; y < selectedItemGrid.gridSizeHeight; y++)
				{
					InventoryItem item = selectedItemGrid.GetItem(x, y) as InventoryItem;
					if (item != null && item.itemData != null)
					{
						if (item.onGridPositionX == x && item.onGridPositionY == y)
						{
							totalValue += item.itemData.cost;
						}
					}
				}
			}
		}
		scoreText.text = $"{totalValue}";
	}

	public void FinishGame()
	{
		int totalValue = 0;
		if (selectedItemGrid != null)
		{
			for (int x = 0; x < selectedItemGrid.gridSizeWidth; x++)
			{
				for (int y = 0; y < selectedItemGrid.gridSizeHeight; y++)
				{
					InventoryItem item = selectedItemGrid.GetItem(x, y) as InventoryItem;
					if (item != null && item.itemData != null)
					{
						if (item.onGridPositionX == x && item.onGridPositionY == y)
						{
							totalValue += item.itemData.cost;
						}
					}
				}
			}
		}
		GameManager.Instance.SaveScore(totalValue);
	}

	public void ForceSetScore(int newScore)
	{
		if (selectedItemGrid != null)
		{
			for (int x = 0; x < selectedItemGrid.gridSizeWidth; x++)
			{
				for (int y = 0; y < selectedItemGrid.gridSizeHeight; y++)
				{
					InventoryItem item = selectedItemGrid.GetItem(x, y) as InventoryItem;
					if (item != null)
					{
						Destroy(item.gameObject);
						selectedItemGrid.CleanGridReference(x, y);
					}
				}
			}
		}

		if (scoreText != null)
		{
			scoreText.text = newScore.ToString();
		}

		if (selectedItem != null)
		{
			Destroy(selectedItem.gameObject);
			selectedItem = null;
		}
	}
}