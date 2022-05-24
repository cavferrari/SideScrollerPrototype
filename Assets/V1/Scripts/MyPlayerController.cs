using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

public class MyPlayerController : MonoBehaviour
{
    public float acceleration = 0.1f;
    public float deceleration = 0.2f;
    public float maxWalkVelocity = 1f;
    public float maxRunVelocity = 1f;
    public float coverAcceleration = 1f;
    public float coveringPoint = 0.65f;
    public float animationToRealMultiplier = 0.01f;
    public Transform[] coverPoints;
    public bool isCovering;
    public bool isShooting;
    public bool isKneeling;

    private Animator animator;
    private int velocityHash;
    private int isCoveringHash;
    private int isShootingHash;
    private int isKneelingHash;
    private float velocity = 0f;
    private AimIK aimIk;
    private LimbIK limbIK;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        aimIk = this.GetComponent<AimIK>();
        limbIK = this.GetComponent<LimbIK>();
        velocityHash = Animator.StringToHash("velocity");
        isCoveringHash = Animator.StringToHash("isCovering");
        isShootingHash = Animator.StringToHash("isShooting");
        isKneelingHash = Animator.StringToHash("isKneeling");
        aimIk.solver.SetIKPositionWeight(0f);
    }

    void Update()
    {
        bool isFowardPress = Input.GetKey(KeyCode.D);
        bool isBackwardsPress = Input.GetKey(KeyCode.A);
        isCovering = (CanCover()) ? Input.GetKey(KeyCode.W) : false;
        isShooting = Input.GetMouseButton(0);
        isKneeling = Input.GetKey(KeyCode.S);

        float currenMaxVelocity = (isBackwardsPress) ? maxWalkVelocity : maxRunVelocity;

        UpdateVelocity(isFowardPress, isBackwardsPress, currenMaxVelocity);

        if (isShooting)
        {
            if (isCovering)
            {
                StartCoroutine(WaitAndShoot(0.9f, 1f));
            }
            else
            {
                aimIk.solver.SetIKPositionWeight(1f);
            }
        }
        else
        {
            aimIk.solver.SetIKPositionWeight(0f);
        }

        if (isCovering)
        {
            if (!isShooting) limbIK.solver.SetIKPositionWeight(0f);
            else limbIK.solver.SetIKPositionWeight(1f);
            if (this.transform.position.x > -coveringPoint)
            {
                this.transform.Translate(-this.transform.right * Time.deltaTime * coverAcceleration);
            }
        }
        else
        {
            limbIK.solver.SetIKPositionWeight(1f);
            if (this.transform.position.x < -0.14f)
            {
                this.transform.Translate(this.transform.right * Time.deltaTime * coverAcceleration * 3f);
            }
        }

        animator.SetBool(isCoveringHash, isCovering);
        animator.SetFloat(velocityHash, velocity);
        animator.SetBool(isShootingHash, isShooting);
        animator.SetBool(isKneelingHash, isKneeling);

        if (!isCovering && !isKneeling)
        {
            this.transform.Translate(this.transform.forward * velocity * animationToRealMultiplier);
        }
    }

    private void UpdateVelocity(bool isFowardPress, bool isBackwardsPress, float currentMaxVelocity)
    {
        if (isFowardPress && velocity < currentMaxVelocity)
        {
            velocity += Time.deltaTime * acceleration;
            if (velocity > currentMaxVelocity)
            {
                velocity = currentMaxVelocity;
            }
        }
        else if (isBackwardsPress && velocity > -currentMaxVelocity)
        {
            velocity -= Time.deltaTime * acceleration;
            if (velocity < -currentMaxVelocity)
            {
                velocity = -currentMaxVelocity;
            }
        }

        if (!isFowardPress && velocity > 0f)
        {
            velocity -= Time.deltaTime * deceleration;
            if (velocity < 0f)
            {
                velocity = 0f;
            }
        }
        if (!isBackwardsPress && velocity < 0f)
        {
            velocity += Time.deltaTime * deceleration;
            if (velocity > 0f)
            {
                velocity = 0f;
            }
        }
    }

    private bool CanCover()
    {
        if (this.transform.position.z > coverPoints[0].position.z && this.transform.position.z < coverPoints[1].position.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator WaitAndShoot(float time, float weight)
    {
        yield return new WaitForSeconds(time);
        aimIk.solver.SetIKPositionWeight(weight);
    }
    /* void OnDrawGizmos()
    {
        Color color;
        color = Color.green;
        // local up
        DrawHelperAtCenter(this.transform.up, color, 2f);

        color = Color.yellow;
        // global up
        DrawHelperAtCenter(Vector3.up, color, 1f);

        color = Color.blue;
        // local forward
        DrawHelperAtCenter(this.transform.forward, color, 2f);

        color = Color.cyan;
        // global forward
        DrawHelperAtCenter(Vector3.forward, color, 1f);

        color = Color.red;
        // local right
        DrawHelperAtCenter(this.transform.right, color, 2f);

        color = Color.magenta;
        // global right
        DrawHelperAtCenter(Vector3.right, color, 1f);
    }

    private void DrawHelperAtCenter(
                       Vector3 direction, Color color, float scale)
    {
        Gizmos.color = color;
        Vector3 destination = transform.position + direction * scale;
        Gizmos.DrawLine(transform.position, destination);
    } */
    /* if (transform.eulerAngles.y < 180f)
           {
               this.transform.Rotate(0f, angularVelocity, 0f, Space.World);
           }
           if (transform.eulerAngles.y > 180f)
           {
               transform.eulerAngles = new Vector3(0f, 180f, 0f);
           } */
    /* if (transform.eulerAngles.y > 0f)
    {
        this.transform.Rotate(0f, -angularVelocity, 0f, Space.World);
    }
    if (transform.eulerAngles.y < 0f)
    {
        transform.eulerAngles = Vector3.zero;
    } */
}
