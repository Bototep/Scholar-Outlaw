using UnityEngine;

public class ItemPickup : MonoBehaviour
{
	public ItemData itemData;
	[SerializeField] private LayerMask playerLayer;
	private bool isPlayerInRange = false;

	[System.Obsolete]
	private void Update()
	{
		if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
		{
			TryPickUpItem();
		}
	}

	[System.Obsolete]
	private void TryPickUpItem()
	{
		PlayerMovement player = FindObjectOfType<PlayerMovement>();
		InventoryController inventory = InventoryController.Instance;

		if (player == null || inventory == null) return;

		if (player.inventoryPanel != null && !player.inventoryPanel.activeSelf)
		{
			player.ToggleInventory();
		}

		InventoryItem inventoryItem = Instantiate(inventory.itemPrefab).GetComponent<InventoryItem>();
		inventoryItem.Set(itemData);

		inventory.selectedItem = inventoryItem;
		inventory.rectTransform = inventoryItem.GetComponent<RectTransform>();
		inventory.rectTransform.SetParent(inventory.canvasTransform);

		Destroy(gameObject); 
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
		{
			isPlayerInRange = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
		{
			isPlayerInRange = false;
		}
	}
}