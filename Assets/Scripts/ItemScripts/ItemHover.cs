using UnityEngine;

// Copilot: "How can I make an item hover up and down
public class ItemHover : MonoBehaviour
{
    public float amplitude = 0.25f;   
    public float frequency = 2f;      
    public float rotationSpeed = 50f; 

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Hover motion
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0, yOffset, 0);

        // rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
