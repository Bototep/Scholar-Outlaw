using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
	public Animator animator;
	public SpriteRenderer spriteRenderer;
	private PlayerMovement playerMovement;
	private Vector2 lastMovementDirection = Vector2.down;

	private void Start()
	{
		playerMovement = GetComponent<PlayerMovement>();
		if (animator == null) animator = GetComponent<Animator>();
		if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (playerMovement == null || !playerMovement.canMove)
		{
			PlayIdleAnimation();
			return;
		}

		UpdateAnimation();
	}

	private void PlayIdleAnimation()
	{
		if (Mathf.Abs(lastMovementDirection.y) > Mathf.Abs(lastMovementDirection.x))
		{
			if (lastMovementDirection.y > 0)
				animator.Play("IdleBack");
			else
				animator.Play("Idle");
		}
		else
		{
			animator.Play("IdleSide");
			spriteRenderer.flipX = lastMovementDirection.x < 0;
		}
	}

	private void UpdateAnimation()
	{
		Vector2 movement = playerMovement.movement;
		bool isWalking = movement.magnitude > 0.1f;

		if (isWalking)
		{
			lastMovementDirection = movement;
			PlayWalkAnimation(movement);
		}
		else
		{
			PlayIdleAnimation();
		}
	}

	private void PlayWalkAnimation(Vector2 movement)
	{
		if (movement.y != 0)
		{
			if (movement.y > 0)
				animator.Play("WalkUp");
			else
				animator.Play("WalkDown");
		}
		else 
		{
			animator.Play("WalkSide");
			spriteRenderer.flipX = movement.x < 0;
		}
	}
}