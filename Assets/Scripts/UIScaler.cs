using UnityEngine;
using UnityEngine.UI;

public class UIScaler : MonoBehaviour
{
    [SerializeField] RectTransform canvasRect;
    RectTransform rectTransform;
    public float sim;
    Vector2 size;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        float widthRatio = canvasRect.rect.width / Screen.width;
        float heightRatio = canvasRect .rect.height / Screen.height;

        float offsetTop = (Screen.safeArea.yMax - Screen.height) * heightRatio;
        float offsetButtom = Screen.safeArea.yMin * heightRatio;
        float offsetLeft = Screen.safeArea.xMin * widthRatio;
        float offsetRight = (Screen.safeArea.xMax - Screen.width) * widthRatio;

        rectTransform.offsetMax = new Vector2(offsetRight, offsetTop);
        rectTransform.offsetMin = new Vector2(offsetLeft, offsetButtom);
        CanvasScaler canvasScaler = canvasRect.GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x,
            canvasScaler.referenceResolution.y + Mathf.Abs(offsetTop) + Mathf.Abs(offsetButtom));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
