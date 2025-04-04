using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerCombat : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] GameObject firePoint;
    [SerializeField] GameObject bloodSplatter;
    [SerializeField] TrailRenderer bulletTrail;

    [Header("Target Variables")]
    [SerializeField] LayerMask playerMask, enemyMask;
    public bool currentlyAiming, aimingAtEnemy;
    public GameObject enemyToAttack;
    GameObject targetToFireAt;

    [Header("Miscellaneous Variables")]
    [SerializeField] public Weapon activeWeapon;
    public float focusTime;
    public static PlayerCombat Instance;
    PlayerInputs PlayerActions;
    RaycastHit[] boxHit;
    bool canFire = true;

    public event Action<GameObject, float> enemyAttacked;
    public event Action<GameObject> AimingAtEnemy;

    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // Assigns player inputs and assigns current active weapon
        PlayerActions = new PlayerInputs();
        PlayerActions.Player.PlayerAim.performed += ctx => Aim(ctx);
        PlayerActions.Player.PlayerAim.canceled += ctx => Aim(ctx);
        PlayerActions.Player.PlayerAttack.performed += ctx => Fire(ctx);
        PlayerActions.Enable();

        activeWeapon = GetComponentInChildren<Weapon>();
    }
    void Update()
    {
        if (!currentlyAiming)
            return;

        // Aim is handled through a raycast that moves from players mouse to worldspace and only returns details based on whether it hits a layermask that only renders the enemy.
        Ray ray = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, playerMask))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 dir = (hit.point - transform.position).normalized;
            Quaternion lookDir = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            // Player is rotated in the direction of their target.
            transform.rotation = Quaternion.Slerp(transform.rotation, lookDir, 2 * Time.deltaTime);
        }

        Vector3 target = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        Ray targetRay = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        // Focus time is then calculated through the weapons speed, the longer the player holds down a focus time the more damage it deals.
        if (Physics.Raycast(ray, out hit, 1000, enemyMask) && !hit.transform.gameObject.GetComponent<Enemy>().isDead)
        {
            aimingAtEnemy = true;
            if (enemyToAttack == hit.transform.gameObject)
                focusTime -= Time.deltaTime;
            else
                focusTime = 3f;

            enemyToAttack = hit.transform.gameObject;
            RecursiveTargetCheck(hit.transform);
            AimingAtEnemy?.Invoke(targetToFireAt);
        }
        else
        {
            focusTime = 3f;
            aimingAtEnemy = false;
        }
        boxHit = Physics.BoxCastAll(target.normalized, new Vector3(5, 5, 5), transform.forward, transform.rotation, 1000, enemyMask);
    }

    void RecursiveTargetCheck(Transform targetFire)
    {
        // Runs through all children in an Enemy gameobject to find the target reticle to lock onto.
        foreach (Transform t in targetFire)
        {
            if (t.transform.Find("Target") == null)
            {
                RecursiveTargetCheck(t);
            }
            else
            {
                targetToFireAt = t.transform.Find("Target").gameObject;
                break;
            }
        }  
    }

    private void Aim(InputAction.CallbackContext ctx)
    {
        // Uses the player input handler to tell whether the player is or isn't aiming and assigns the appropriate states to coordinate.
        if (ctx.performed)
        {
            PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Aiming;
            currentlyAiming = true;
        }
        if (ctx.canceled)
        {
            if (!PlayerMovement.PlayerMove.isMoving)
                PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Walking;
            else
                PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Idle;
            focusTime = 3f;
            currentlyAiming = false;
        }
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        // Fires a LineRenderer in the enemies direction and deals damage to the enemy, also adds in a VFX bloodsplatter.
        if (!aimingAtEnemy || (activeWeapon.currentAmmo == 0 && activeWeapon.needsAmmo) || !canFire)
            return;
        if (activeWeapon.needsAmmo)
            activeWeapon.RemoveAmmo();
        
        TrailRenderer newBulletTrail = Instantiate(bulletTrail, firePoint.transform.position, Quaternion.identity);
        enemyAttacked?.Invoke(enemyToAttack, (activeWeapon.weaponDamage / (focusTime + 1)));
        canFire = false;
        focusTime += 2f;
        if (focusTime > 3f)
            focusTime = 3f;
        Vector3 dir = (firePoint.transform.position - targetToFireAt.transform.position).normalized;
        Instantiate(bloodSplatter, targetToFireAt.transform.position, transform.rotation);
        StartCoroutine(ShotCooldown());
        StartCoroutine(BulletTrail(newBulletTrail, targetToFireAt.transform.position));
    }

    IEnumerator BulletTrail(TrailRenderer trail, Vector3 endPosition)
    {
        // BulletTrail calculates the position of the LineRenderer so that it reaches its target across a set amount of time.
        float bulletTime = 0;
        float distance = Vector3.Distance(firePoint.transform.position, targetToFireAt.transform.position);
        float startingDistance = distance;
        while (bulletTime < 1)
        {
            trail.transform.position = Vector3.Lerp(trail.transform.position, targetToFireAt.transform.position, bulletTime);
            bulletTime += Time.deltaTime / bulletTrail.time;
            yield return null;
        }

        trail.transform.position = endPosition;
        Destroy(trail.gameObject, trail.time);

    }

    IEnumerator ShotCooldown()
    {
        // Cooldown between each shot.
        yield return new WaitForSeconds(activeWeapon.weaponSpeed);
        canFire = true;
    }
}
