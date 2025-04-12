using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ForceScale : MonoBehaviour
{
	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		ResetScale();
	}

	private void Update()
	{
		if (rectTransform.localScale != Vector3.one)
		{
			ResetScale();
		}
	}

	private void ResetScale()
	{
		Quaternion currentRotation = rectTransform.localRotation;
		rectTransform.localScale = Vector3.one;
		rectTransform.localRotation = currentRotation;
	}

	public void ForceResetScale()
	{
		ResetScale();
	}
}