using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CarShooter : MonoBehaviour
{
    public GameObject crosshairUI;
    public Transform firePoint;
    public GameObject hitEffectPrefab;
    public TrailPool trailPool; 
    public float fireRate = 0.2f;
    public float damage = 10f;
    public float range = 500f;
    public float bloomAmount = 0.05f; 
    
    public float lingerTime = 0.05f;
    public float fadeTime = 0.2f;

    public float crosshairKickSize = 1.5f;
    public float crosshairRecoverySpeed = 15f;

    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    public float maxHorizontalAngle = 70f;

    public bool IsAiming { get; private set; }
    public Quaternion AimRotation { get; private set; }

    private float currentYaw;
    private float currentPitch;
    private float nextFireTime;
    private Vector3 crosshairOriginalScale;
    private Camera mainCam;
    private bool wasAiming;
    private EnergySystem energySystem;

    void Start()
    {
        mainCam = Camera.main;
        energySystem = GetComponent<EnergySystem>();

        if (crosshairUI != null)
        {
            crosshairOriginalScale = crosshairUI.transform.localScale;
            crosshairUI.SetActive(false);
        }
        
        AimRotation = transform.rotation;
        currentYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            IsAiming = Mouse.current.rightButton.isPressed;
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(IsAiming);
            crosshairUI.transform.localScale = Vector3.Lerp(crosshairUI.transform.localScale, crosshairOriginalScale, Time.deltaTime * crosshairRecoverySpeed);
        }
        
        if (IsAiming)
        {
            if (!wasAiming)
            {
                currentYaw = transform.eulerAngles.y;
                currentPitch = 0f;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
            currentYaw += mouseDelta.x * mouseSensitivity * 0.1f;
            currentPitch -= mouseDelta.y * mouseSensitivity * 0.1f;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            float carBodyYaw = transform.eulerAngles.y;
            float deltaYaw = Mathf.DeltaAngle(carBodyYaw, currentYaw);
            deltaYaw = Mathf.Clamp(deltaYaw, -maxHorizontalAngle, maxHorizontalAngle);
            currentYaw = carBodyYaw + deltaYaw;

            AimRotation = Quaternion.Euler(currentPitch, currentYaw, 0);

            if (firePoint != null) firePoint.rotation = AimRotation;

            if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
            {
                if (energySystem == null || energySystem.TryConsume(energySystem.shootCost))
                {
                    nextFireTime = Time.time + fireRate;
                    Shoot();
                }
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        wasAiming = IsAiming;
    }

    void Shoot()
    {
        if (crosshairUI != null)
        {
            crosshairUI.transform.localScale = crosshairOriginalScale * crosshairKickSize;
        }

        if (firePoint == null) return;

        Vector3 targetPoint;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit cameraHit, range))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        
        shootDirection += Random.insideUnitSphere * bloomAmount;
        shootDirection.Normalize();

        Vector3 finalHitPosition = firePoint.position + (shootDirection * range);

        if (Physics.Raycast(firePoint.position, shootDirection, out RaycastHit gunHit, range))
        {
            finalHitPosition = gunHit.point;

            if (gunHit.collider.gameObject.layer == 0 && hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, gunHit.point, Quaternion.LookRotation(gunHit.normal));
            }

            Health targetHealth = gunHit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }

        if (trailPool != null)
        {
            BulletTrail trail = trailPool.GetTrail();
            trail.Init(firePoint, finalHitPosition, lingerTime + fadeTime, trailPool.ReturnTrailToPool);
        }
    }
}