using UnityEngine;
using UnityEngine.InputSystem;

public class Fishing : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject bobberPrefab;   // Префаб поплавка
    [SerializeField] private float castDistance = 15f;  // Дальность заброса
    [SerializeField] private Camera playerCamera;       // Камера персонажа

    [Header("Кнопки")]
    [SerializeField] private InputActionReference castAction;     // Заброс (E или ПКМ)
    [SerializeField] private InputActionReference hookAction;     // Подсечка (ЛКМ)

    [Header("Настройки поклёвки")]
    [SerializeField] private float minBiteTime = 3f;    // Минимальное время до поклёвки
    [SerializeField] private float maxBiteTime = 8f;    // Максимальное время до поклёвки
    [SerializeField] private float hookWindowTime = 1f; // Время на подсечку (сек)
    [SerializeField] private FightingMinigame fightingMinigame;

    private GameObject currentBobber;  // Текущий поплавок
    private float biteTimer;           // Таймер до поклёвки
    private bool isWaitingForBite;     // Ждём поклёвку?
    private bool isBiting;             // Идёт поклёвка прямо сейчас?
    private float hookTimer;           // Таймер окна подсечки

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
    }

    private void Update()
    {
        // ОБРАБОТКА ЗАБРОСА
        if (castAction != null && castAction.action.triggered && currentBobber == null)
        {
            CastFishingRod();
        }

        // ОБРАБОТКА ПОДСЕЧКИ (только если идёт поклёвка)
        if (hookAction != null && hookAction.action.triggered && isBiting)
        {
            Hook();
        }

        // ЛОГИКА ОЖИДАНИЯ ПОКЛЁВКИ
        if (isWaitingForBite)
        {
            biteTimer -= Time.deltaTime;
            if (biteTimer <= 0f)
            {
                StartBite();
            }
        }

        // ЛОГИКА ОКНА ПОДСЕЧКИ
        if (isBiting)
        {
            hookTimer -= Time.deltaTime;
            if (hookTimer <= 0f)
            {
                MissBite();  // Время вышло - рыба ушла
            }
        }
    }

    // ЗАБРОС
    private void CastFishingRod()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, castDistance))
        {
            if (hit.collider.CompareTag("Water"))
            {
                currentBobber = Instantiate(bobberPrefab, hit.point, Quaternion.identity);
                Debug.Log($"🎣 Заброс! Ждём поклёвку...");

                // Запускаем ожидание поклёвки
                isWaitingForBite = true;
                isBiting = false;
                biteTimer = Random.Range(minBiteTime, maxBiteTime);
            }
        }
    }

    // НАЧАЛО ПОКЛЁВКИ (поплавок ныряет)
    private void StartBite()
    {
        isWaitingForBite = false;
        isBiting = true;
        hookTimer = hookWindowTime;

        // Анимация: поплавок ныряет под воду
        if (currentBobber != null)
        {
            // Опускаем поплавок вниз (имитация ныряния)
            currentBobber.transform.position += Vector3.down * 0.3f;
            Debug.Log($"🐟 ПОКЛЁВКА! Нажми ЛКМ чтобы подсечь! ({hookWindowTime} сек)");
        }
    }

    // УСПЕШНАЯ ПОДСЕЧКА
    private void Hook()
    {
        isBiting = false;
        Debug.Log($"✅ ПОДСЕЧКА! Начинаем вываживание!");

        // ЗАПУСКАЕМ МИНИ-ИГРУ
        if (fightingMinigame != null)
        {
            fightingMinigame.StartMinigame();
        }

        RemoveBobber(); // Убираем поплавок
    }

    // ПРОМАХНУЛИСЬ - рыба ушла
    private void MissBite()
    {
        isBiting = false;
        Debug.Log($"❌ Рыба ушла! Поплавок всплывает...");

        // Анимация: поплавок всплывает обратно
        if (currentBobber != null)
        {
            currentBobber.transform.position += Vector3.up * 0.3f;
        }

        // Запускаем новый цикл ожидания
        isWaitingForBite = true;
        biteTimer = Random.Range(minBiteTime, maxBiteTime);
    }

    // Удалить поплавок (рыба поймана или вышли из игры)
    public void RemoveBobber()
    {
        if (currentBobber != null)
            Destroy(currentBobber);
        currentBobber = null;
        isWaitingForBite = false;
        isBiting = false;
    }
}
