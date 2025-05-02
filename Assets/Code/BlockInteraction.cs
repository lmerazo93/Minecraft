using UnityEngine;
using System.Collections.Generic; // Required for lists

public class BlockInteraction : MonoBehaviour
{
    [System.Serializable] // Make this class serializable
    public class BlockType
    {
        public string name;
        public GameObject prefab;
    }

    public BlockType[] blockTypes; // Array of block types
    public float reachDistance = 5f;         // Max distance to interact with blocks
    public float placementOffset = 0.1f;       // Offset to place blocks on top (REDUCED!)
    public bool useGrid = true;           // Enable/disable grid-based placement
    public Vector3Int gridSize = new Vector3Int(1, 1, 1);  // Grid size
    public float blockMoveSpeed = 10f;         // Speed of block movement
    public float blockRotationSpeed = 200f;      // Speed of block rotation
    public LayerMask placementLayerMask; // Only allow placement on these layers
    public LayerMask interactionLayerMask;

    private Transform selectedBlock;
    private Vector3 targetPosition;          // Target position for smooth movement
    private Quaternion targetRotation;        // Target rotation for smooth rotation
    private Vector3 touchStartPos;          // Store the initial touch position
    private bool isRotating = false;
    private bool isDeleting = false;
    private bool isDragging = false; // Track if we are dragging
    private int selectedBlockTypeIndex = 0; // Index of the selected block type

    private Rigidbody selectedRigidbody; // Store the Rigidbody

    void Update()
    {
        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
        else if (Input.touchCount == 2)
        {
            HandleTwoTouches();
        }
        else
        {
            selectedBlock = null; // Reset if no touches
            isRotating = false;
            isDeleting = false;
            isDragging = false;
        }

        // Smoothly move and rotate the selected block
        if (selectedBlock != null)
        {
            selectedBlock.position = Vector3.MoveTowards(selectedBlock.position, targetPosition, blockMoveSpeed * Time.deltaTime);
            selectedBlock.rotation = Quaternion.RotateTowards(selectedBlock.rotation, targetRotation, blockRotationSpeed * Time.deltaTime);
        }
    }

    void HandleSingleTouch()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position; // Store initial touch position

            if (Physics.Raycast(ray, out hit, reachDistance, interactionLayerMask)) // Use interactionLayerMask
            {
                if (hit.collider.CompareTag("DraggableBlock"))
                {
                    selectedBlock = hit.transform;
                    targetPosition = selectedBlock.position; // Initialize target position
                    targetRotation = selectedBlock.rotation; // Initialize target rotation;

                    // Get and store the rigidbody, set kinematic
                    selectedRigidbody = hit.collider.GetComponent<Rigidbody>(); //get from the hit
                    if (selectedRigidbody != null)
                    {
                        selectedRigidbody.isKinematic = true; // Make it kinematic while dragging
                        selectedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // More precise collision
                    }
                    isDragging = true;
                }
            }
            else if (Physics.Raycast(ray, out hit, reachDistance, placementLayerMask)) //spawn block if raycast does not hit draggable
            {
                Vector3 spawnPosition = hit.point + hit.normal * placementOffset;
                if (useGrid)
                {
                    spawnPosition = SnapToGrid(spawnPosition);
                }
                // Instantiate the selected block type
                GameObject newBlock = Instantiate(blockTypes[selectedBlockTypeIndex].prefab, spawnPosition, Quaternion.identity);
                selectedBlock = newBlock.transform; //set selected block to the new block.
                targetPosition = newBlock.transform.position;
                targetRotation = newBlock.transform.rotation;
                selectedRigidbody = newBlock.GetComponent<Rigidbody>();
                if (selectedRigidbody != null)
                {
                    selectedRigidbody.isKinematic = true;
                    selectedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;  // More precise collision
                }
                isDragging = true;
            }
        }
        else if (touch.phase == TouchPhase.Moved && selectedBlock != null && isDragging) //check for isDragging
        {
            if (Physics.Raycast(ray, out hit, reachDistance, placementLayerMask)) // Use placementLayerMask
            {
                Vector3 newTargetPosition = hit.point + hit.normal * placementOffset; // Raycast and add normal

                if (useGrid)
                {
                    newTargetPosition = SnapToGrid(newTargetPosition);
                }
                targetPosition = newTargetPosition; // Set the target position for smooth movement.
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (selectedRigidbody != null)
            {
                selectedRigidbody.isKinematic = false; // Make it non-kinematic when released.
                selectedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; //default
            }
            selectedBlock = null;
            isDragging = false;
        }
    }

    void HandleTwoTouches()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        Vector2 prevTouch1Pos = touch1.position - touch1.deltaPosition;
        Vector2 prevTouch2Pos = touch2.position - touch2.deltaPosition;

        // Prevent conflicts with dragging
        if (selectedBlock != null && Vector2.Distance(touch1.position, touchStartPos) < 10f && Vector2.Distance(touch2.position, touchStartPos) < 10f)
        {
            return; // If it was a small movement, do nothing
        }

        // Rotation (Twist)
        Vector2 prevDir = (prevTouch2Pos - prevTouch1Pos).normalized;
        Vector2 currentDir = (touch2.position - touch1.position).normalized;
        float angle = Vector2.SignedAngle(prevDir, currentDir);
        if (selectedBlock != null && !isDragging) // Only rotate if not dragging
        {
            targetRotation = selectedBlock.rotation * Quaternion.AngleAxis(-angle, Vector3.up); // Set target rotation
            isRotating = true;
        }

        // Deletion (Pinch)
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float prevDistance = Vector2.Distance(prevTouch1Pos, prevTouch2Pos);
        if (prevDistance > 0 && currentDistance < prevDistance - 20 && !isRotating && !isDragging && selectedBlock != null) //no isDragging
        {
            Destroy(selectedBlock.gameObject);
            selectedBlock = null;
            isDeleting = true;
        }
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridSize.x) * gridSize.x;
        int y = Mathf.RoundToInt(position.y / gridSize.y) * gridSize.y;
        int z = Mathf.RoundToInt(position.z / gridSize.z) * gridSize.z;
        return new Vector3(x, y, z);
    }
}

