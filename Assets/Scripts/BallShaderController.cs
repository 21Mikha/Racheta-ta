using UnityEngine;

public class BallShaderController : MonoBehaviour
{
    private Rigidbody rb;
    private Material ballMaterial;

    [SerializeField] private string velocityProperty = "_Velocity";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Pass velocity to the shader
        if (ballMaterial.HasProperty(velocityProperty))
        {
            ballMaterial.SetVector(velocityProperty, rb.velocity);
        }
    }
}