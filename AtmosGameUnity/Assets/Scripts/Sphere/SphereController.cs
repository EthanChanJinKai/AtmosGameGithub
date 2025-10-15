using UnityEngine;


[RequireComponent(typeof(Rigidbody))] 
public class SphereController : MonoBehaviour
{

    public float moveSpeed = 10.0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

        rb.AddForce (movement * moveSpeed);
    }
}
