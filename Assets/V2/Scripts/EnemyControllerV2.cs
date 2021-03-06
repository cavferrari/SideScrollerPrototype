using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

public class EnemyControllerV2 : MonoBehaviour
{
    public float acceleration = 1f;
    public float deceleration = 1f;
    public float maxWalkVelocity = 1f;
    public Transform target;
    public float COVER_TO_STAND_TIME = 1.3f;
    public float KNEELING_TO_STAND_TIME = 1.4f;
    public float COVER_SHOOTING_TIME = 0.4f;
    public float KNEELING_SHOOTING_TIME = 1f;

    private enum State
    {
        IDLE, COVERING, COVERING_SHOOTING_START, COVERING_SHOOTING_END,
        KNEELING, KNEELING_SHOOTING_START, KNEELING_SHOOTING_END,
        CROUCHING
    }
    private Animator animator;
    private Ballistics.Weapon weapon;
    private Recoil recoil;
    private State state = State.IDLE;
    private int velocityHash;
    private int isCoveringHash;
    private int isShootingHash;
    private int isKneelingHash;
    private int isCrouchingHash;
    private int isDeadHash;
    private bool isWalking;
    private bool isWalkingBack;
    private float velocity = 0f;
    private bool isCovering;
    private bool isShooting;
    private bool wasShooting;
    private bool isKneeling;
    private bool wasKneeling;
    private bool isCrouching;
    private bool isDead = false;
    private AimIK aimIk;
    private FullBodyBipedIK fbbIk;
    private bool hasShootAnimationStarted = false;
    private bool isPlayerInRange = false;
    private bool isResponding = false;
    private Vector3 playerPosition;
    private PlayerControllerV2 playerController;
    private bool canKneelingCover = false;
    private int shots = 0;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        weapon = this.GetComponentInChildren<Ballistics.Weapon>();
        recoil = this.GetComponentInChildren<Recoil>();
        aimIk = this.GetComponent<AimIK>();
        fbbIk = this.GetComponent<FullBodyBipedIK>();
        velocityHash = Animator.StringToHash("velocity");
        isCoveringHash = Animator.StringToHash("isCovering");
        isShootingHash = Animator.StringToHash("isShooting");
        isKneelingHash = Animator.StringToHash("isKneeling");
        isCrouchingHash = Animator.StringToHash("isCrouching");
        isDeadHash = Animator.StringToHash("isDead");
    }

    void Update()
    {
        if (!isDead)
        {
            switch (state)
            {
                case State.IDLE:
                    UpdateIdle();
                    break;
                case State.COVERING:
                    UpdateCovering();
                    break;
                case State.COVERING_SHOOTING_START:
                    UpdateCoveringShootingStart();
                    break;
                case State.COVERING_SHOOTING_END:
                    UpdateCoveringShootingEnd();
                    break;
                case State.KNEELING:
                    UpdateKneeling();
                    break;
                case State.KNEELING_SHOOTING_START:
                    UpdateKneelingShootingStart();
                    break;
                case State.KNEELING_SHOOTING_END:
                    UpdateKneelingShootingEnd();
                    break;
                case State.CROUCHING:
                    UpdateCrouching();
                    break;
                default:
                    break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayerInRange = true;
            playerController = other.transform.root.gameObject.GetComponent<PlayerControllerV2>();
            if (playerController.IsCovering())
            {
                playerPosition = other.gameObject.transform.position;
                playerPosition.z = 0.232f;
            }
            else
            {
                playerPosition = other.gameObject.transform.position;
                playerPosition.z = 0f;
            }
        }
        if (other.tag == "KneelingCover")
        {
            canKneelingCover = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayerInRange = false;
        }
        if (other.tag == "KneelingCover")
        {
            canKneelingCover = false;
        }
    }

    public bool IsCovering()
    {
        return isCovering;
    }


    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            SetIkWeight(0f);
            animator.SetTrigger(isDeadHash);
        }
    }

    private void UpdateIdle()
    {
        if (this.transform.position.z > -0.06f)
        {
            this.transform.Translate(this.transform.forward * Time.deltaTime * 0.5f);
            if (this.transform.position.z < -0.06f)
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -0.06f);
            }
        }
        if (IsIkActive() && isCovering)
        {
            SetIkWeight(0f);
            animator.SetBool(isCoveringHash, true);
            isResponding = false;
            state = State.COVERING;
        }
        else if (IsIkActive() && isKneeling)
        {
            if (canKneelingCover)
            {
                SetIkWeight(0f);
                animator.SetBool(isKneelingHash, true);
                state = State.KNEELING;
            }
            else
            {
                animator.SetBool(isCrouchingHash, true);
                state = State.CROUCHING;
            }
        }
        else
        {
            if (isPlayerInRange && !isResponding && this.transform.position.z == -0.06f)
            {
                StartCoroutine(Respond());
            }
            if (isShooting)
            {
                StartCoroutine(WaitAndShoot(0.3f));
            }
            UpdateVelocity(isWalking, isWalkingBack, maxWalkVelocity);
            animator.SetFloat(velocityHash, velocity);
            this.transform.Translate(-this.transform.right * velocity * 0.01f);
        }
    }

    private void UpdateCovering()
    {
        if (isCovering)
        {
            if (this.transform.position.z < 0.329f)
            {
                this.transform.Translate(-this.transform.forward * Time.deltaTime * 0.5f);
                if (this.transform.position.z > 0.329f)
                {
                    this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.329f);
                }
            }
            if (isPlayerInRange && !isResponding)
            {
                StartCoroutine(Respond());
            }
            if (isShooting)
            {
                StartCoroutine(WaitAndSetIkWeigth(COVER_SHOOTING_TIME, 1f));
                animator.SetBool(isShootingHash, true);
                state = State.COVERING_SHOOTING_START;
            }
        }
        else
        {
            StartCoroutine(WaitAndSetIkWeigth(COVER_TO_STAND_TIME, 1f));
            animator.SetBool(isCoveringHash, false);
            state = State.IDLE;
        }
    }

    private void UpdateCoveringShootingStart()
    {
        if (!hasShootAnimationStarted)
        {
            hasShootAnimationStarted = IsAnimationPlaying("CoverShooting");
        }
        else
        {
            if (!IsAnimationPlaying("CoverShooting"))
            {
                StartCoroutine(WaitAndShoot(0.2f));
                state = State.COVERING_SHOOTING_END;
            }
        }
    }

    private void UpdateCoveringShootingEnd()
    {
        if (!wasShooting)
        {
            SetIkWeight(0f);
            animator.SetBool(isShootingHash, false);
            state = State.COVERING;
        }
    }

    private void UpdateKneeling()
    {
        if (isKneeling)
        {
            if (isPlayerInRange && !isResponding)
            {
                StartCoroutine(Respond());
            }
            if (isShooting)
            {
                StartCoroutine(WaitAndSetIkWeigth(KNEELING_SHOOTING_TIME, 1f));
                animator.SetBool(isShootingHash, true);
                state = State.KNEELING_SHOOTING_START;
            }
        }
        else
        {
            StartCoroutine(WaitAndSetIkWeigth(KNEELING_TO_STAND_TIME, 1f));
            animator.SetBool(isKneelingHash, false);
            state = State.IDLE;
        }
    }

    private void UpdateKneelingShootingStart()
    {
        if (!hasShootAnimationStarted)
        {
            hasShootAnimationStarted = IsAnimationPlaying("KneelingShoot");
        }
        else
        {
            if (!IsAnimationPlaying("KneelingShoot"))
            {
                StartCoroutine(WaitAndShoot(0.2f));
                state = State.KNEELING_SHOOTING_END;
            }
        }
    }

    private void UpdateKneelingShootingEnd()
    {
        if (!wasShooting)
        {
            SetIkWeight(0f);
            animator.SetBool(isShootingHash, false);
            state = State.KNEELING;
        }
    }

    private void UpdateCrouching()
    {
        if (isPlayerInRange && !isResponding)
        {
            StartCoroutine(Respond());
        }
        if (isShooting)
        {
            StartCoroutine(WaitAndShoot(0.3f));
        }
        if (!isKneeling)
        {
            animator.SetBool(isCrouchingHash, false);
            state = State.IDLE;
        }
    }

    private IEnumerator WaitAndShoot(float time)
    {
        Shoot();
        isShooting = false;
        yield return new WaitForSeconds(time);
        hasShootAnimationStarted = false;
        wasShooting = false;
    }

    private bool IsAnimationPlaying(string animationName)
    {
        return IsAnimatorPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }

    private bool IsAnimatorPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length > animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private IEnumerator WaitAndSetIkWeigth(float time, float weight)
    {
        yield return new WaitForSeconds(time);
        SetIkWeight(weight);
    }

    private IEnumerator Respond()
    {
        isResponding = true;
        shots = UnityEngine.Random.Range(1, 4);
        yield return new WaitForSeconds(2f);
        while (shots > 0)
        {
            target.position = new Vector3(playerPosition.x, UnityEngine.Random.Range(0.5f, 1.7f), playerPosition.z);
            isShooting = true;
            yield return new WaitForSeconds(1f);
            shots -= 1;
        }
        int choice = UnityEngine.Random.Range(1, 4);
        if (choice == 1)
        {
            isCovering = false;
            isKneeling = false;
        }
        else if (choice == 2)
        {
            isKneeling = false;
            isCovering = true;
        }
        else
        {
            isKneeling = true;
            isCovering = false;
        }
        isResponding = false;
    }

    private void SetIkWeight(float weight)
    {
        if (aimIk.solver.GetIKPositionWeight() != weight)
        {
            aimIk.solver.SetIKPositionWeight(weight);
            fbbIk.solver.SetIKPositionWeight(weight);
        }
    }

    private bool IsIkActive()
    {
        if (aimIk.solver.GetIKPositionWeight() == 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Shoot()
    {
        recoil.Fire(2f);
        weapon.ShootBullet(weapon.PhysicalBulletSpawnPoint.forward);
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

    private void GetShootInput()
    {
        if (!wasShooting)
        {
            isShooting = Input.GetMouseButtonDown(0);
        }
        if (isShooting) wasShooting = true;
    }
}
