using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageScanMiniGameMVP : MonoBehaviour
{
    public ARTrackedImageManager imageTrackingManager;
    public string targetImageName = "MinecraftCard";
    public GameObject buildingWorldPrefab;
    public Transform spawnPoint;

    private GameObject currentBuildingWorld;
    private bool buildingWorldSpawned = false;

    void OnEnable()
    {
        if (imageTrackingManager != null) imageTrackingManager.trackedImagesChanged += OnTrackedImagesChanged;
        else Debug.LogError("AR Image Tracking Manager is not assigned!");
    }

    void OnDisable()
    {
        if (imageTrackingManager != null) imageTrackingManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            if (trackedImage.referenceImage.name == targetImageName && !buildingWorldSpawned)
                SpawnBuildingWorld(trackedImage.transform);
    }

    void SpawnBuildingWorld(Transform cardTransform)
    {
        buildingWorldSpawned = true;
        if (buildingWorldPrefab != null)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : cardTransform.position;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            currentBuildingWorld = Instantiate(buildingWorldPrefab, spawnPosition, spawnRotation);
            currentBuildingWorld.transform.SetParent(cardTransform, true);
        }
        else Debug.LogError("Building World Prefab is not assigned!");
    }

    public void DestroyBuildingWorld()
    {
        buildingWorldSpawned = false;
        if (currentBuildingWorld != null) Destroy(currentBuildingWorld);
        currentBuildingWorld = null;
    }
}
