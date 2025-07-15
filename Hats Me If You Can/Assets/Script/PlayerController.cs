using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyPlayerWithSprintAndStamina : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 8f;
    public float crouchSpeed = 3f;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";
    public string crouchParam = "IsCrouching";

    [Header("References")]
    public Transform cameraTransform;

    [Header("Item Holding & Throwing")]
    public Transform handPosition;
    public GameObject itemPrefab;
    public float throwForce = 10f;
    public Vector3 normalHoldOffset = Vector3.zero;
    public Vector3 aimHoldOffset = new Vector3(0f, 0.1f, 0.4f);
    public float aimSmoothSpeed = 5f;

    [Header("Throw Arc Settings")]
    public LayerMask arcCollisionMask;
    public int arcResolution = 30;
    public float arcTimeStep = 0.1f;
    public float lineWidth = 0.05f;

    Rigidbody rb;
    CapsuleCollider col;
    LineRenderer arcRenderer;

    float originalHeight;
    Vector3 originalCenter;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;
    int jumpCount;
    const int maxJumps = 2;
    float currentStamina;

    bool isHidden = false;
    bool isAiming;

    GameObject heldItem;
    Rigidbody heldRb;
    Collider heldCol;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentStamina = maxStamina;

        originalHeight = col.height;
        originalCenter = col.center;

        // Spawn item
        if (itemPrefab && handPosition)
        {
            heldItem = Instantiate(itemPrefab, handPosition);
            heldItem.transform.localPosition = normalHoldOffset;
            heldItem.transform.localRotation = Quaternion.identity;

            heldRb = heldItem.GetComponent<Rigidbody>();
            heldCol = heldItem.GetComponent<Collider>();

            if (heldRb) heldRb.isKinematic = true;
            if (heldCol) heldCol.enabled = false;
        }

        // Arc Renderer Setup
        arcRenderer = gameObject.AddComponent<LineRenderer>();
        arcRenderer.positionCount = arcResolution;
        arcRenderer.startWidth = arcRenderer.endWidth = lineWidth;
        arcRenderer.material = new Material(Shader.Find("Sprites/Default"));
        arcRenderer.material.color = Color.white;
        arcRenderer.enabled = false;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        if (isGrounded) jumpCount = 0;

        // Crouch
        isCrouching = Input.GetKey(KeyCode.C);
        animator.SetBool(crouchParam, isCrouching);

        if (isCrouching)
        {
            col.height = originalHeight * 0.5f;
            col.center = originalCenter - new Vector3(0, originalHeight * 0.25f, 0);
        }
        else
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps && !isCrouching)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }

        // Fall boost
        if (!isGrounded && rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * 3f * Time.deltaTime;

        // Stamina
        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isCrouching;
        if (isSprinting && rb.linearVelocity.magnitude > 0.1f)
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
        else if (!isSprinting && isGrounded)
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);

        // Animator speed
        Vector3 flatVel = rb.linearVelocity; flatVel.y = 0;
        animator.SetFloat(speedParam, flatVel.magnitude);

        // Aiming
        isAiming = Input.GetMouseButton(1);
        if (heldItem)
        {
            Vector3 targetOffset = isAiming ? aimHoldOffset : normalHoldOffset;
            heldItem.transform.localPosition = Vector3.Lerp(heldItem.transform.localPosition, targetOffset, Time.deltaTime * aimSmoothSpeed);
        }

        // Show arc while aiming
        if (isAiming && heldItem)
        {
            arcRenderer.enabled = true;
            DrawThrowArc();
        }
        else
        {
            arcRenderer.enabled = false;
        }

        // Throw
        if (Input.GetMouseButtonDown(0) && heldItem)
        {
            ThrowHeldItem();
        }
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : moveSpeed);

        Vector3 fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();
        Vector3 move = (right * x + fwd * z).normalized;

        Vector3 targetVel = move * speed;
        Vector3 velChange = targetVel - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(velChange, ForceMode.VelocityChange);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime));
        }
    }

    void ThrowHeldItem()
    {
        heldItem.transform.SetParent(null);
        heldRb.isKinematic = false;
        heldRb.useGravity = true;
        heldCol.enabled = true;

        float finalForce = isAiming ? throwForce * 1.2f : throwForce;
        Vector3 throwDirection = (transform.forward + transform.up * 0.2f).normalized;
        heldRb.AddForce(throwDirection * finalForce, ForceMode.Impulse);

        heldItem = null;
        heldRb = null;
        heldCol = null;
        arcRenderer.enabled = false;
    }

    void DrawThrowArc()
    {
        if (!heldItem || !heldRb) return;

        Vector3[] points = new Vector3[arcResolution];
        Vector3 startPos = handPosition.position + transform.forward * 0.1f;
        Vector3 startVel = transform.forward * (isAiming ? throwForce * 1.2f : throwForce);

        for (int i = 0; i < arcResolution; i++)
        {
            float t = i * arcTimeStep;
            Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
            points[i] = point;

            if (i > 0)
            {
                if (Physics.Linecast(points[i - 1], points[i], out RaycastHit hit, arcCollisionMask))
                {
                    points[i] = hit.point;
                    for (int j = i + 1; j < arcResolution; j++)
                        points[j] = hit.point;
                    break;
                }
            }
        }

        arcRenderer.positionCount = arcResolution;
        arcRenderer.SetPositions(points);
    }

    public bool IsCrouching() => isCrouching;

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        Debug.Log("Hidden: " + isHidden);
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = hidden ? Color.gray : Color.white;
    }
}
