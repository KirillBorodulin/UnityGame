using UnityEngine;
using UnityEngine.InputSystem;

public class Fishing : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private float castDistance = 15f;
    [SerializeField] private Camera playerCamera;

    [Header("Кнопки")]
    [SerializeField] private InputActionReference castAction;
    [SerializeField] private InputActionReference hookAction;

    [Header("Настройки поклёвки")]
    [SerializeField] private float minBiteTime = 3f;
    [SerializeField] private float maxBiteTime = 8f;
    [SerializeField] private float hookWindowTime = 1f;

    [Header("Вываживание")]
    [SerializeField] private FishingFightDistance fightSystem;

    private GameObject currentBobber;
    private float biteTimer;
    private bool isWaitingForBite;
    private bool isBiting;
    private float hookTimer;
    private Vector3 lastCastPosition;  // ЗАПОМИНАЕМ ПОЗИЦИЮ ЗАБРОСА

    private void OnEnable()
    {
        if (castAction != null) castAction.action.Enable();
        if (hookAction != null) hookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (castAction != null) castAction.action.Disable();
        if (hookAction != null) hookAction.action.Disable();
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (fightSystem != null)
        {
            fightSystem.OnWin += OnFightWin;
            fightSystem.OnLose += OnFightLose;
        }
    }

    private void OnDestroy()
    {
        if (fightSystem != null)
        {
            fightSystem.OnWin -= OnFightWin;
            fightSystem.OnLose -= OnFightLose;
        }
    }

    private void Update()
    {
        bool isFighting = fightSystem != null && fightSystem.IsFighting();

        if (castAction != null && castAction.action.triggered && currentBobber == null && !isBiting && !isFighting)
        {
            CastFishingRod();
        }

        if (hookAction != null && hookAction.action.triggered && isBiting && !isFighting)
        {
            Hook();
        }

        if (isWaitingForBite && !isFighting)
        {
            biteTimer -= Time.deltaTime;
            if (biteTimer <= 0f)
            {
                StartBite();
            }
        }

        if (isBiting && !isFighting)
        {
            hookTimer -= Time.deltaTime;
            if (hookTimer <= 0f)
            {
                MissBite();
            }
        }
    }

    private void CastFishingRod()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, castDistance))
        {
            if (hit.collider.CompareTag("Water"))
            {
                lastCastPosition = hit.point;  // ЗАПОМИНАЕМ ПОЗИЦИЮ
                currentBobber = Instantiate(bobberPrefab, lastCastPosition, Quaternion.identity);
                isWaitingForBite = true;
                isBiting = false;
                biteTimer = Random.Range(minBiteTime, maxBiteTime);
            }
        }
    }

    private void StartBite()
    {
        isWaitingForBite = false;
        isBiting = true;
        hookTimer = hookWindowTime;

        if (currentBobber != null)
        {
            currentBobber.transform.position += Vector3.down * 0.3f;
        }
    }

    private void Hook()
    {
        isBiting = false;

        // Сохраняем позицию поплавка перед удалением
        Vector3 bobberPos = currentBobber != null ? currentBobber.transform.position : lastCastPosition;
        RemoveBobber();

        if (fightSystem != null)
        {
            float randomWeight = Random.Range(0.5f, 2.5f);
            // ПЕРЕДАЁМ ПОЗИЦИЮ ЗАБРОСА
            fightSystem.StartFighting(bobberPos, randomWeight);
        }
    }

    private void MissBite()
    {
        isBiting = false;

        if (currentBobber != null)
        {
            currentBobber.transform.position += Vector3.up * 0.3f;
        }

        isWaitingForBite = true;
        biteTimer = Random.Range(minBiteTime, maxBiteTime);
    }

    private void OnFightWin()
    {
        Debug.Log("Рыба поймана!");
    }

    private void OnFightLose()
    {
        Debug.Log("Рыба ушла...");
    }

    public void RemoveBobber()
    {
        if (currentBobber != null)
            Destroy(currentBobber);
        currentBobber = null;
    }
}