using UnityEngine;
using UnityEngine.InputSystem;

public class Fishing : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject bobberPrefab;   // Префаб поплавка
    [SerializeField] private LayerMask waterLayer;      // Слой воды
    [SerializeField] private float castDistance = 15f;  // Дальность заброса
    [SerializeField] private Camera playerCamera;       // Камера персонажа

    [Header("Кнопка заброса")]
    [SerializeField] private InputActionReference castAction;  // Кнопка (например, E или ПКМ)

    private GameObject currentBobber;  // Текущий поплавок в воде

    private void OnEnable()
    {
        if (castAction != null)
            castAction.action.Enable();
    }

    private void OnDisable()
    {
        if (castAction != null)
            castAction.action.Disable();
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        // Если нажали кнопку и нет поплавка в воде
        if (castAction != null && castAction.action.triggered && currentBobber == null)
        {
            CastFishingRod();
        }
    }

    private void CastFishingRod()
    {
        // Пускаем луч из центра камеры вперёд
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, castDistance, waterLayer))
        {
            // Попали в воду - создаём поплавок
            Vector3 castPoint = hit.point;
            currentBobber = Instantiate(bobberPrefab, castPoint, Quaternion.identity);
            Debug.Log($"Заброс! Поплавок в: {castPoint}");
        }
        else
        {
            Debug.Log("❌ Не попал в воду!");
        }
    }

    // Удалить поплавок (когда рыба поймана или ушла)
    public void RemoveBobber()
    {
        if (currentBobber != null)
            Destroy(currentBobber);
        currentBobber = null;
    }
}
