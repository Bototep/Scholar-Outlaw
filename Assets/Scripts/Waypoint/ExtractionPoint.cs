using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtractionPoint : MonoBehaviour
{
	[Header("Scene Settings")]
	public int targetSceneIndex = 2; // Default to scene index 2

	[Header("Visual Feedback")]
	public ParticleSystem enterEffect; // Optional visual effect
	public AudioClip enterSound;       // Optional sound effect

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Check if the entering object is the player
		if (other.CompareTag("Player"))
		{
			// Play effects if assigned
			if (enterEffect != null)
				Instantiate(enterEffect, transform.position, Quaternion.identity);

			if (enterSound != null)
				AudioSource.PlayClipAtPoint(enterSound, transform.position);

			// Load the target scene
			SceneManager.LoadScene(targetSceneIndex);
		}
	}

	// Visualize the trigger area in editor
	private void OnDrawGizmos()
	{
		Collider2D collider = GetComponent<Collider2D>();
		if (collider == null) return;

		Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green

		if (collider is BoxCollider2D box)
		{
			Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
		}
		else if (collider is CircleCollider2D circle)
		{
			Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
		}
	}
}