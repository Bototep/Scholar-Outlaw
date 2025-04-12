using UnityEngine;
using UnityEngine.UI;

public class WebGLUIController : MonoBehaviour
{
	private CanvasScaler canvasScaler;
	private RectTransform canvasRect;

	private void Awake()
	{
		canvasScaler = GetComponent<CanvasScaler>();
		canvasRect = GetComponent<RectTransform>();
		AdjustForWebGL();
	}

	private void AdjustForWebGL()
	{
#if UNITY_WEBGL
		canvasScaler.referenceResolution = new Vector2(
			Mathf.RoundToInt(canvasScaler.referenceResolution.x),
			Mathf.RoundToInt(canvasScaler.referenceResolution.y)
		);

		if (IsMobileBrowser())
		{
			canvasScaler.matchWidthOrHeight = 0;
		}
#endif
	}

	private bool IsMobileBrowser()
	{
#if UNITY_WEBGL
		return Application.platform == RuntimePlatform.WebGLPlayer &&
			   (Screen.width <= 800 || Screen.height <= 800);
#else
        return false;
#endif
	}
}