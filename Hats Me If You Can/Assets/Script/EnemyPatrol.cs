using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private Transform target;
    private float waitCounter = 0f;
    private bool isWaiting = false;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
            {
                isWaiting = false;
                target = (target == pointA) ? pointB : pointA;
            }
            return;
        }

        // Move toward target, stay grounded (Y axis locked)
        Vector3 moveTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

        // Face movement direction
        Vector3 direction = (moveTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // If close enough to the target point, pause and switch
        if (Vector3.Distance(transform.position, moveTarget) < 0.1f)
        {
            isWaiting = true;
            waitCounter = waitTime;
        }
    }
}
