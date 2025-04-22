using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    private Vector3 touchOffset;
    private Transform selectedBlock;

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (touch.phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray, out hit))
                    if (hit.collider.CompareTag("DraggableBlock"))
                    {
                        selectedBlock = hit.transform;
                        touchOffset = selectedBlock.position - hit.point;
                    }
            }
            else if (touch.phase == TouchPhase.Moved && selectedBlock != null)
                if (Physics.Raycast(ray, out hit))
                    selectedBlock.position = hit.point + touchOffset;
            else if (touch.phase == TouchPhase.Ended)
                selectedBlock = null;
        }
        else if (Input.touchCount == 2 && selectedBlock != null)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Simple Rotation (Twist)
            Vector2 prevDir = (touch2.position - touch1.position).normalized;
            Vector2 currentDir = (touch2.deltaPosition - touch1.deltaPosition).normalized;
            float angle = Vector2.SignedAngle(prevDir, currentDir);
            selectedBlock.Rotate(Vector3.up, -angle, Space.World);

            // Simple Deletion (Pinch)
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
            if (prevDistance > 0 && currentDistance < prevDistance - 20)
            {
                Destroy(selectedBlock.gameObject);
                selectedBlock = null;
            }
        }
    }
}