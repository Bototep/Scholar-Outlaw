using UnityEngine;

public class SoundDetector : MonoBehaviour
{
	private void LateUpdate()
	{
		// Reset position to parent's center (0,0,0 in local space)
		transform.localPosition = Vector3.zero;

		// Optional: Reset rotation if needed
		transform.localRotation = Quaternion.identity;

		// Optional: Reset scale if needed
		transform.localScale = Vector3.one;
	}
}