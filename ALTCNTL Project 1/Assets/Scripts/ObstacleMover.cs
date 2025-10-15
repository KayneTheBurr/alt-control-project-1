using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [HideInInspector] public float speed = 3f;
    [HideInInspector] public float despawnY = -6f;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }
}
