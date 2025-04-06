using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    private float timeOffset;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void Update()
    {
        // Rotate the coin
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Make the coin bob up and down
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Car"))
        {
            isCollected = true;
            // Play coin collection sound with current coins collected
            SVS.AudioPlayer.instance.PlayCoinCollectionSound(CoinManager.Instance.GetCoinsCollected() + 1);
            
            // Notify the CoinManager that this coin was collected
            CoinManager.Instance.CollectCoin();
            // Destroy the coin
            Destroy(gameObject);
        }
    }
} 