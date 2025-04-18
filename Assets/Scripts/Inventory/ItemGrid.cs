using System;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{
	public const float tileSizeWidth = 97;
	public const float tileSizeHeight = 97;

	InventoryItem[,] inventoryItemSlot;
	RectTransform rectTransform;
	Canvas parentCanvas;

	[SerializeField] public int gridSizeWidth = 20;
	[SerializeField] public int gridSizeHeight = 10;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		parentCanvas = GetComponentInParent<Canvas>();
		Init(gridSizeWidth, gridSizeHeight);
	}

	public InventoryItem PickUpItem(int x, int y)
	{
		InventoryItem toReturn = inventoryItemSlot[x, y];

		if (toReturn == null) { return null; }

		ClearGridReference(toReturn);

		return toReturn;
	}

	private void ClearGridReference(InventoryItem item)
	{
		for (int ix = 0; ix < item.WIDTH; ix++)
		{
			for (int iy = 0; iy < item.HEIGHT; iy++)
			{
				inventoryItemSlot[item.onGridPositionX + ix, item.onGridPositionY + iy] = null;
			}
		}
	}

	private void Init(int width, int hight)
	{
		inventoryItemSlot = new InventoryItem[width, hight];
		Vector2 size = new Vector2(width * tileSizeWidth, hight * tileSizeHeight);
		rectTransform.sizeDelta = size;
	}

	Vector2 positionOnTheGrid = new Vector2();
	Vector2Int tileGridPosition = new Vector2Int();

	public Vector2Int GetTileGridPosition(Vector2 mousePosition, InventoryItem item = null)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rectTransform,
			mousePosition,
			parentCanvas.worldCamera,
			out positionOnTheGrid);

		tileGridPosition.x = (int)(positionOnTheGrid.x / tileSizeWidth);
		tileGridPosition.y = (int)(-positionOnTheGrid.y / tileSizeHeight); 

		if (item != null)
		{
			tileGridPosition.x -= (item.WIDTH - 1) / 2;
			tileGridPosition.y -= (item.HEIGHT - 1) / 2;
		}

		return tileGridPosition;
	}

	public bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, ref InventoryItem overlapItem)
	{

		if (BoundryCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT) == false)
		{
			return false;
		}

		if (OverlapCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT, ref overlapItem) == false)
		{
			overlapItem = null;
			return false;
		}

		if (overlapItem != null)
		{
			ClearGridReference(overlapItem);
		}

		PlaceItem(inventoryItem, posX, posY);

		return true;
	}

	public void PlaceItem(InventoryItem inventoryItem, int posX, int posY)
	{
		RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
		rectTransform.SetParent(this.rectTransform);

		for (int x = 0; x < inventoryItem.WIDTH; x++)
		{
			for (int y = 0; y < inventoryItem.HEIGHT; y++)
			{
				inventoryItemSlot[posX + x, posY + y] = inventoryItem;
			}
		}

		inventoryItem.onGridPositionX = posX;
		inventoryItem.onGridPositionY = posY;
		Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);

		rectTransform.localPosition = position;
	}

	public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
	{
		Vector2 position = new Vector2();
		position.x = posX * tileSizeWidth + tileSizeWidth * inventoryItem.WIDTH / 2;
		position.y = -(posY * tileSizeHeight + tileSizeHeight * inventoryItem.HEIGHT / 2);
		return position;
	}

	private bool OverlapCheck(int posX, int posY, int width, int height, ref InventoryItem overlapItem)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (inventoryItemSlot[posX + x, posY + y] != null)
				{
					if (overlapItem == null)

					{
						overlapItem = inventoryItemSlot[posX + x, posY + y];
					}
					else
					{
						if (overlapItem != inventoryItemSlot[posX + x, posY + y])
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private bool CheckAvailableSpace(int posX, int posY, int width, int height)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (inventoryItemSlot[posX + x, posY + y] != null)
				{
					return false;

				}
			}
		}
		return true;
	}

	bool PositionCheck(int posX, int posY)
	{
		if(posX < 0 || posY < 0) return false;

		if(posX >= gridSizeWidth || posY >= gridSizeHeight) return false;

		return true;
	}

	public bool BoundryCheck(int posX, int posY, int width, int height)
	{
		if (!PositionCheck(posX, posY)) return false;
		if (!PositionCheck(posX + width - 1, posY)) return false; 
		if (!PositionCheck(posX, posY + height - 1)) return false; 
		if (!PositionCheck(posX + width - 1, posY + height - 1)) return false;

		return true;
	}

	public InventoryItem GetItem(int x, int y)
	{
		if (PositionCheck(x, y) == false) return null;
		return inventoryItemSlot[x, y];
	}

	public void CleanGridReference(int posX, int posY)
	{
		inventoryItemSlot[posX, posY] = null;
	}

	public bool IsPositionInGrid(Vector2Int gridPosition)
	{
		return PositionCheck(gridPosition.x, gridPosition.y);
	}

	internal Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
	{
		int height = gridSizeHeight - itemToInsert.HEIGHT + 1;
		int width = gridSizeWidth - itemToInsert.WIDTH + 1;

		for (int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				if(CheckAvailableSpace(x, y, itemToInsert.WIDTH, itemToInsert.HEIGHT) == true)
				{
					return new Vector2Int(x, y);
				}
			}
		}
		return null;
	}
}
