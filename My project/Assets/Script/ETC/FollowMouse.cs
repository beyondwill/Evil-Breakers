using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    // 변수
    private float width;
    private float height;

    private void Start()
    {
        width = gameObject.GetComponent<RectTransform>().rect.width;
        height = gameObject.GetComponent<RectTransform>().rect.height;
        Debug.Log(width + height);
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        // 화면 경계 제한
        mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width - width);
        mousePos.y = Mathf.Clamp(mousePos.y, height, Screen.height);

        // 화면 -> 월드 변환
        mousePos.z = 10f;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        transform.position = worldPos;
    }
}