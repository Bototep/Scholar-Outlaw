using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
	[SerializeField] private GameObject inventoryPanel;
	public static InventoryController Instance { get; private set; }

	[HideInInspector]
	private ItemGrid selectedItemGrid;

	public ItemGrid SelectedItemGrid {
		get => selectedItemGrid;
		set{
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

		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (selectedItem == null) CreateRandomItem();
		}

		if (Input.GetKeyDown(KeyCode.W))
		{
			InsertRandomItem();
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			RotateItem();
		}

		if (Input.GetKeyDown(KeyCode.T)) // Press T to test
		{
			CalculateTotalInventoryValue();
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
				if (selectedItemGrid == null ||
					!selectedItemGrid.BoundryCheck(gridPos.x, gridPos.y, selectedItem.WIDTH, selectedItem.HEIGHT))
				{
					Destroy(selectedItem.gameObject);
					selectedItem = null;
				}
				else
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

	private void RotateItem()
	{
		if (selectedItem == null) { return; }

		selectedItem.Rotate();
	}

	private void InsertRandomItem()
	{
		if(selectedItemGrid == null) { return; }

		CreateRandomItem();
		InventoryItem itemToInsert = selectedItem;
		selectedItem= null;
		InsertItem(itemToInsert);
	}

	public void InsertItem(InventoryItem itemToInsert)
	{
		Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
		if (posOnGrid == null) { return; }

		selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
	}

	Vector2Int oldPosition;
	InventoryItem itemToHighlight;
	private void HandleHighlight()
	{
		Vector2Int positionOnGrid = GetTileGridPosition();
		if(oldPosition == positionOnGrid) { return; }

		oldPosition= positionOnGrid;
		if (selectedItem == null)
		{
			itemToHighlight = (InventoryItem)selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

			if (itemToHighlight != null)
			{
				inventoryHighlight.Show(true);
				inventoryHighlight.SetSize(itemToHighlight);
				//inventoryHighlight.SetParent(selectedItemGrid);
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
				selectedItem.HEIGHT)
				);
			inventoryHighlight.SetSize(selectedItem);
			//inventoryHighlight.SetParent(selectedItemGrid);
			inventoryHighlight.SetPostion(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
		}
	}

	private void CreateRandomItem()
	{
		InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
		selectedItem = inventoryItem;

		Debug.Log("adwad");

		rectTransform = inventoryItem.GetComponent<RectTransform>();
		rectTransform.SetParent(canvasTransform);
		rectTransform.SetAsLastSibling();


		int selectedItemID = UnityEngine.Random.Range(0, items.Count);
		inventoryItem.Set(items[selectedItemID]);
	}

	public bool AddItemToInventory(ItemData itemData)
	{
		// Create the item
		InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
		rectTransform = inventoryItem.GetComponent<RectTransform>();
		rectTransform.SetParent(canvasTransform);
		rectTransform.SetAsLastSibling();

		// Set up the item
		inventoryItem.Set(itemData);

		// Try to place it in inventory
		selectedItem = inventoryItem;
		InsertItem(inventoryItem);

		// Return whether the item was successfully placed
		if (inventoryItem.transform.parent == null)
		{
			Destroy(inventoryItem.gameObject);
			return false; // Failed to add
		}
		return true; // Successfully added
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
			PlaceItem(tileGridPosition);
		}
	}

	private Vector2Int GetTileGridPosition()
	{
		Vector2 position = Input.mousePosition;

		if (selectedItem != null)
		{
			position.x -= (selectedItem.WIDTH - 1) * ItemGrid.tileSizeWidth / 2;
			position.y += (selectedItem.HEIGHT - 1) * ItemGrid.tileSizeHeight / 2;
		}

		return selectedItemGrid.GetTileGridPosition(Input.mousePosition); 
	}

	private void PlaceItem(Vector2Int tileGridPosition)
	{
		bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
		if (complete)
		{
			selectedItem = null;
			if(overlapItem != null)
			{
				selectedItem= overlapItem;
				overlapItem = null;
				rectTransform = selectedItem.GetComponent<RectTransform>();
				rectTransform.SetAsLastSibling();
			}
		}
	}

	private void PickUpItem(Vector2Int tileGridPosition)
	{
		selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
		if (selectedItem != null)
		{
			rectTransform = selectedItem.GetComponent<RectTransform>();
		}
	}

	public void ToggleInventory(bool show)
	{
		if (inventoryPanel != null)
		{
			inventoryPanel.SetActive(show);
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
				// Optional: Add effect here
				Debug.Log("Discarding " + selectedItem.itemData.name);
				Destroy(selectedItem.gameObject);
				selectedItem = null;
			}

			inventoryPanel.SetActive(false);
		}
	}

	public void CalculateTotalInventoryValue()
	{
		if (selectedItemGrid == null)
		{
			Debug.Log("No inventory grid selected");
			return;
		}

		int totalValue = 0;
		int itemCount = 0;

		for (int x = 0; x < selectedItemGrid.gridSizeWidth; x++)
		{
			for (int y = 0; y < selectedItemGrid.gridSizeHeight; y++)
			{
				InventoryItem item = selectedItemGrid.GetItem(x, y) as InventoryItem;
				if (item != null && item.itemData != null)
				{
					// Only count if this is the top-left corner of the item
					if (item.onGridPositionX == x && item.onGridPositionY == y)
					{
						totalValue += item.itemData.cost;
						itemCount++;
					}
				}
			}
		}

		Debug.Log($"Inventory contains {itemCount} items worth {totalValue} total");
	}
}
