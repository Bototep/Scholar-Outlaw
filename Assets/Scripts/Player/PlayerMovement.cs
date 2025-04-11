using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float sprintSpeed = 8f;
	public bool canMove = true;
	public GameObject inventoryPanel;

	[Header("Sound Detection")]
	public GameObject soundDetector;
	public float sprintSoundRadius = 5f;

	private Rigidbody2D rb;
	public Vector2 movement;
	private bool isSprinting;
	private bool wasSprintingLastFrame;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		if (soundDetector != null)
		{
			soundDetector.SetActive(false);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			ToggleInventory();
		}

		wasSprintingLastFrame = isSprinting;

		if (canMove)
		{
			isSprinting = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0;
		}
		else
		{
			isSprinting = false;
		}

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
			if (isSprinting)
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
			inventoryPanel.SetActive(openingInventory);
			canMove = !inventoryPanel.activeSelf;

			if (inventoryPanel.activeSelf && isSprinting)
			{
				isSprinting = false;
				UpdateSoundDetector();
			}
		}
	}

	private void LoadGameOverScene(bool killedByEnemy)
	{
		if (InventoryController.Instance != null)
		{
			if (killedByEnemy)
			{
				InventoryController.Instance.ForceSetScore(0);
			}
			InventoryController.Instance.FinishGame();
		}
		SceneManager.LoadScene(2);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
		{
			LoadGameOverScene(true);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Enemy"))
		{
			LoadGameOverScene(true);
		}
		else if (other.CompareTag("ExtractionPoint"))
		{
			LoadGameOverScene(false);
		}
	}
}