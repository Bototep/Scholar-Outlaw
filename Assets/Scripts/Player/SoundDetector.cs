using UnityEngine;

public class SoundDetector : MonoBehaviour
{
	private void LateUpdate()
	{
		transform.localPosition = Vector3.zero;

		transform.localRotation = Quaternion.identity;

		transform.localScale = Vector3.one;
	}
}