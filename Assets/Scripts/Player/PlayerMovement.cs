using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float sprintSpeed = 8f;  // Speed when sprinting
	public bool canMove = true;
	public GameObject inventoryPanel;

	[Header("Sound Detection")]
	public GameObject soundDetector; // Assign the child GameObject in Inspector
	public float sprintSoundRadius = 5f;

	private Rigidbody2D rb;
	public Vector2 movement;
	private bool isSprinting;
	private bool wasSprintingLastFrame; // To track state changes

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		if (soundDetector != null)
		{
			soundDetector.SetActive(false); // Ensure it starts disabled
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			ToggleInventory();
		}

		// Store previous sprint state
		wasSprintingLastFrame = isSprinting;

		// Check for sprint input only if player can move
		if (canMove)
		{
			isSprinting = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0;
		}
		else
		{
			isSprinting = false;
		}

		// Only update sound detector when state changes
		if (isSprinting != wasSprintingLastFrame)
		{
			UpdateSoundDetector();
		}
	}

	private void FixedUpdate()
	{
		if (canMove)
		{
			movement.x = Input.GetAxisRaw("Horizontal");
			movement.y = Input.GetAxisRaw("Vertical");
			movement = movement.normalized;
		}
		else
		{
			movement = Vector2.zero;
			if (isSprinting) // Force stop sprinting if can't move
			{
				isSprinting = false;
				UpdateSoundDetector();
			}
		}

		float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
		rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
	}

	private void UpdateSoundDetector()
	{
		if (soundDetector != null)
		{
			soundDetector.SetActive(isSprinting);

			// Optional: Adjust collider radius if needed
			CircleCollider2D collider = soundDetector.GetComponent<CircleCollider2D>();
			if (collider != null)
			{
				collider.radius = sprintSoundRadius;
			}
		}
	}

	public void ToggleInventory()
	{
		if (inventoryPanel != null)
		{
			bool openingInventory = !inventoryPanel.activeSelf;

			if (openingInventory)
			{
				// Normal open behavior
				inventoryPanel.SetActive(true);
			}
			else
			{
				// Close inventory and clean up any held items
				InventoryController.Instance.CloseInventory();
			}

			canMove = !inventoryPanel.activeSelf;

			if (inventoryPanel.activeSelf && isSprinting)
			{
				isSprinting = false;
				UpdateSoundDetector();
			}
		}
	}
}