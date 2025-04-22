using UnityEngine;
using System.Collections;
using TMPro;

public class LocationRewardManagerMVP : MonoBehaviour
{
    public double testLatitude = 34.0608; 
    public double testLongitude = -117.6550; // Example: Ontario, CA
    public TMP_Text rewardTextUI;

    private bool locationServiceInitialized = false;
    private bool rewardClaimed = false;
    private string awardedReward = "";

    void Start() => StartCoroutine(InitializeLocationService());

    IEnumerator InitializeLocationService()
    {
        if (!Input.location.isEnabledByUser) Debug.Log("Location services not enabled by user.");
        Input.location.Start();
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
            yield return new WaitForSeconds(1);
        }
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location.");
            Input.location.Stop();
            yield break;
        }
        Debug.Log("Location service initialized.");
        locationServiceInitialized = true;
    }

    void Update()
    {
        if (locationServiceInitialized && !rewardClaimed)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                Debug.Log("Location service running. Awarding simulated reward.");
                AwardSimulatedReward();
                rewardClaimed = true;
                Input.location.Stop();
                if (rewardTextUI != null) rewardTextUI.text = "You found a: " + awardedReward + "!";
            }
            /*
            else if (locationServiceInitialized && !rewardClaimed)
            {
                float distance = CalculateDistance(Input.location.lastData.latitude, Input.location.longitude, testLatitude, testLongitude);
                if (distance < 1000)
                {
                    Debug.Log("Near McDonald's. Awarding simulated reward.");
                    AwardSimulatedReward();
                    rewardClaimed = true;
                    Input.location.Stop();
                    if (rewardTextUI != null) rewardTextUI.text = "You found a: " + awardedReward + "!";
                }
            }
            */
        }
    }

    float CalculateDistance(double lat1, double lon1, double lat2, double lon2) => Mathf.Sqrt(Mathf.Pow((float)(lat1 - lat2), 2) + Mathf.Pow((float)(lon1 - lon2), 2)) * 111000f;

    void AwardSimulatedReward()
    {
        string[] possibleRewards = { "Exclusive Creeper Skin", "McDonald's Block", "Golden Pickaxe", "Chicken Nugget Pet" };
        awardedReward = possibleRewards[Random.Range(0, possibleRewards.Length)];
        Debug.Log("Awarded: " + awardedReward);
    }
}
