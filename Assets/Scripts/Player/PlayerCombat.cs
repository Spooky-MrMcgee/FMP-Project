using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] GameObject firePoint;
    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] float damage;
    [SerializeField] public Weapon activeWeapon;
    public float focusTime;
    public GameObject enemyToAttack;
    public static PlayerCombat Instance;
    PlayerInputs PlayerActions;
    [SerializeField] LayerMask playerMask, enemyMask;
    public event Action<GameObject> AimingAtEnemy;
    public bool currentlyAiming, aimingAtEnemy;
    [SerializeField] GameObject bloodSplatter;
    GameObject targetToFireAt;
    RaycastHit[] boxHit;
    bool canFire = true;

    public event Action<GameObject, float> enemyAttacked;

    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Player.PlayerAim.performed += ctx => Aim(ctx);
        PlayerActions.Player.PlayerAim.canceled += ctx => Aim(ctx);
        PlayerActions.Player.PlayerAttack.performed += ctx => Fire(ctx);
        PlayerActions.Enable();

        activeWeapon = GetComponentInChildren<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentlyAiming)
            return;

        Ray ray = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //Animator
        playerAnimator.SetBool("isAiming", true);

        if (Physics.Raycast(ray, out hit, 1000, playerMask))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 dir = (hit.point - transform.position).normalized;
            Quaternion lookDir = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookDir, 2 * Time.deltaTime);
        }

        Vector3 target = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        Ray targetRay = PlayerMovement.PlayerMove.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 1000, enemyMask))
        {
            aimingAtEnemy = true;
            if (enemyToAttack == hit.transform.gameObject)
                focusTime -= Time.deltaTime;
            else
                focusTime = 3f;

            enemyToAttack = hit.transform.gameObject;
            targetToFireAt = hit.transform.Find("Target").gameObject;
            AimingAtEnemy?.Invoke(targetToFireAt);
        }
        else
        {
            focusTime = 3f;
            aimingAtEnemy = false;
        }
        boxHit = Physics.BoxCastAll(target.normalized, new Vector3(5, 5, 5), transform.forward, transform.rotation, 1000, enemyMask);
    }

    private void Aim(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            currentlyAiming = true;
        }
        if (ctx.canceled)
        {
            focusTime = 3f;
            currentlyAiming = false;
        }
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (!aimingAtEnemy || (activeWeapon.currentAmmo == 0 && activeWeapon.needsAmmo) || !canFire)
            return;
        if (activeWeapon.needsAmmo)
            activeWeapon.RemoveAmmo();
        
        TrailRenderer newBulletTrail = Instantiate(bulletTrail, firePoint.transform.position, Quaternion.identity);
        Debug.Log(activeWeapon.weaponDamage / (focusTime + 1));
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
        yield return new WaitForSeconds(activeWeapon.weaponSpeed);
        canFire = true;
    }
}
