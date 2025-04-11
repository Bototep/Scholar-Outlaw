using UnityEngine;
using UnityEngine.SceneManagement;
using static SoundManager;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float sprintSpeed = 8f;
	public bool canMove = true;
	public GameObject inventoryPanel;

	[Header("Sound Detection")]
	public GameObject soundDetector;
	public float sprintSoundRadius = 5f;

	[Header("Footstep Sounds")]
	public AudioClip footstepSound;
	[Range(0f, 1f)] public float footstepVolume = 0.5f;
	[Range(0.1f, 3f)] public float walkPitch = 1f;
	[Range(0.1f, 3f)] public float sprintPitch = 1.2f;
	public float footstepIntervalWalk = 0.5f;
	public float footstepIntervalSprint = 0.3f;

	private Rigidbody2D rb;
	public Vector2 movement;
	private bool isSprinting;
	private bool wasSprintingLastFrame;
	private AudioSource footstepSource;
	private float nextFootstepTime;
	private bool wasMovingLastFrame;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		if (soundDetector != null)
		{
			soundDetector.SetActive(false);
		}

		footstepSource = gameObject.AddComponent<AudioSource>();
		footstepSource.clip = footstepSound;
		footstepSource.volume = footstepVolume;
		footstepSource.loop = false;
		footstepSource.playOnAwake = false;
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

		HandleFootstepSounds();
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

	private void HandleFootstepSounds()
	{
		bool isMoving = movement.magnitude > 0.1f && canMove;

		if (isMoving != wasMovingLastFrame)
		{
			if (isMoving)
			{
				PlayFootstep();
				nextFootstepTime = Time.time + (isSprinting ? footstepIntervalSprint : footstepIntervalWalk);
			}
			else
			{
				footstepSource.Stop();
			}
		}

		if (isMoving && Time.time >= nextFootstepTime)
		{
			PlayFootstep();
			nextFootstepTime = Time.time + (isSprinting ? footstepIntervalSprint : footstepIntervalWalk);
		}

		wasMovingLastFrame = isMoving;
	}

	private void PlayFootstep()
	{
		if (footstepSound == null) return;

		footstepSource.pitch = isSprinting ? sprintPitch : walkPitch;
		footstepSource.PlayOneShot(footstepSound);
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

			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.Play(SoundType.Bonus);
			}

			if (inventoryPanel.activeSelf && isSprinting)
			{
				isSprinting = false;
				UpdateSoundDetector();
			}
		}
	}

	public void LoadGameOverScene(bool killedByEnemy)
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

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Enemy"))
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.Play(SoundType.Death);
			}

			if (InventoryController.Instance != null)
			{
				InventoryController.Instance.ForceSetScore(0);
			}

			LoadGameOverScene(true);
		}
		else if (other.CompareTag("ExtractionPoint"))
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.Play(SoundType.Pickup);
			}
			LoadGameOverScene(false);
		}
	}
}