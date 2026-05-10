using UnityEngine;
using UnityEngine.InputSystem;

// МИНИ-ИГРА: Перетягивание рыбы на расстояние
public class FishingFightDistance : MonoBehaviour
{
    [Header("Настройки дистанции")]
    [SerializeField] private float startDistance = 5f;        // Стартовая дистанция от игрока
    [SerializeField] private float maxDistance = 15f;         // Макс дистанция (срыв)
    [SerializeField] private float minDistance = 1.5f;        // Мин дистанция (победа)

    [Header("Скорость движения")]
    [SerializeField] private float fishPullSpeed = 3f;        // Скорость рыбы (от игрока)
    [SerializeField] private float playerPullSpeed = 5f;      // Скорость игрока (к себе)

    [Header("Вес рыбы")]
    [SerializeField] private float fishWeight = 1f;

    [Header("Кнопка")]
    [SerializeField] private InputActionReference pullAction;

    [Header("Визуализация")]
    [SerializeField] private GameObject fishPrefab;           // Модель рыбы (префаб)
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float yOffset = 0.2f;            // Высота НАД водой (+ вверх)
    [SerializeField] private float yDepth = 0f;               // Глубина ПОД водой (+ вниз)

    [Header("Режим высоты")]
    [SerializeField] private bool useDepthMode = true;        // true = под водой, false = над водой

    private bool isFighting = false;
    private GameObject currentFish;
    private float currentDistance;
    private bool isPulling;
    private Vector3 hookPosition;

    public System.Action OnWin;
    public System.Action OnLose;

    private void OnEnable()
    {
        if (pullAction != null)
            pullAction.action.Enable();
    }

    private void OnDisable()
    {
        if (pullAction != null)
            pullAction.action.Disable();
    }

    private void Start()
    {
        if (playerTransform == null)
            playerTransform = transform;
    }

    private void Update()
    {
        if (!isFighting) return;

        isPulling = pullAction != null && pullAction.action.ReadValue<float>() > 0;

        float movement = 0f;

        if (isPulling)
        {
            movement = -playerPullSpeed * Time.deltaTime;
        }
        else
        {
            movement = fishPullSpeed * fishWeight * Time.deltaTime;
        }

        currentDistance += movement;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        UpdateFishPosition();

        if (currentDistance <= minDistance)
        {
            Win();
            return;
        }

        if (currentDistance >= maxDistance)
        {
            Lose("Рыба уплыла!");
            return;
        }
    }

    private void UpdateFishPosition()
    {
        if (currentFish == null) return;

        Vector3 direction = (hookPosition - playerTransform.position).normalized;
        Vector3 fishPosition = playerTransform.position + direction * currentDistance;

        // Расчёт высоты (над или под водой)
        if (useDepthMode)
        {
            // Под водой (глубина)
            fishPosition.y = hookPosition.y - yDepth;
        }
        else
        {
            // Над водой
            fishPosition.y = hookPosition.y + yOffset;
        }

        currentFish.transform.position = fishPosition;

        // Поворот рыбы
        if (isPulling)
        {
            Vector3 toPlayer = playerTransform.position - fishPosition;
            toPlayer.y = 0;
            if (toPlayer != Vector3.zero)
                currentFish.transform.rotation = Quaternion.LookRotation(toPlayer);
        }
        else
        {
            if (direction != Vector3.zero)
                currentFish.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // ЗАПУСК МИНИ-ИГРЫ
    public void StartFighting(Vector3 hookWorldPosition, float weight = 1f)
    {
        isFighting = true;
        fishWeight = weight;
        hookPosition = hookWorldPosition;

        startDistance = Vector3.Distance(playerTransform.position, hookPosition);
        currentDistance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        isPulling = false;

        // Создаём рыбу
        if (fishPrefab != null)
        {
            currentFish = Instantiate(fishPrefab);
        }
        else
        {
            currentFish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentFish.GetComponent<Renderer>().material.color = new Color(0.3f, 0.6f, 1f);
            currentFish.transform.localScale = Vector3.one * 0.5f;
            Destroy(currentFish.GetComponent<Collider>());
        }

        UpdateFishPosition();

        // Логируем настройки глубины
        string depthInfo = useDepthMode ? $"ПОД водой (глубина: {yDepth})" : $"НАД водой (высота: {yOffset})";
        Debug.Log($"РЫБА НА КРЮЧКЕ! Вес: {fishWeight} | {depthInfo}");
    }

    private void Win()
    {
        isFighting = false;

        if (currentFish != null)
            Destroy(currentFish);

        Debug.Log("РЫБА ПОЙМАНА!");
        OnWin?.Invoke();
    }

    private void Lose(string reason)
    {
        isFighting = false;

        if (currentFish != null)
            Destroy(currentFish);

        Debug.Log($"((( {reason}");
        OnLose?.Invoke();
    }

    public bool IsFighting()
    {
        return isFighting;
    }
}