using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float m_speed = 5f;
    [SerializeField] private Camera m_camera;
    // Update is called once per frame
    void Update()
    {
        float angle = Time.deltaTime * m_speed;
        transform.Rotate(Vector3.up, angle);

        if (m_camera) { m_camera.gameObject.transform.LookAt(transform.position); }
    }
}
