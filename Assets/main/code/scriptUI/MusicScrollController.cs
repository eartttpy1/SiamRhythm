using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MusicScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewportCenter; // จุดอ้างอิงกึ่งกลาง
    public float snapSpeed = 10f;       // ความเร็วในการดูด

    private bool isSnapping = false;

    void Update()
    {
        // ถ้าผู้เล่นหยุดรูด และปล่อยนิ้วออกจากจอแล้ว
        if (scrollRect.velocity.magnitude < 200f && !Input.GetMouseButton(0) && !isSnapping)
        {
            SnapToNearest();
        }
    }

    void SnapToNearest()
    {
        float closestDistance = float.MaxValue;
        Transform closestItem = null;

        // วนลูปหา Item ที่ใกล้จุดศูนย์กลางที่สุด
        foreach (Transform child in content)
        {
            float dist = Mathf.Abs(child.position.y - viewportCenter.position.y);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestItem = child;
            }
        }

        if (closestItem != null)
        {
            // หยุดแรงส่งเพื่อให้การดูดนิ่งขึ้น
            scrollRect.velocity = Vector2.zero;
            StartCoroutine(SnapCoroutine(closestItem));
        }
    }

    IEnumerator SnapCoroutine(Transform target)
    {
        isSnapping = true;

        // ค่อยๆ เลื่อน Content จนกว่า Target จะมาอยู่ตรงกลาง
        while (Mathf.Abs(target.position.y - viewportCenter.position.y) > 0.5f)
        {
            float offset = viewportCenter.position.y - target.position.y;
            content.position += new Vector3(0, offset * Time.deltaTime * snapSpeed, 0);
            yield return null;
        }

        // ดูดจนเป๊ะแล้ว
        content.position = new Vector3(content.position.x, content.position.y + (viewportCenter.position.y - target.position.y), content.position.z);
        isSnapping = false;
    }
}