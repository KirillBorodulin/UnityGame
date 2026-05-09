using UnityEngine;
using UnityEngine.InputSystem;

public class Fishing : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject bobberPrefab;   // Префаб поплавка
    [SerializeField] private float castDistance = 15f;  // Дальность заброса
    [SerializeField] private Camera playerCamera;       // Камера персонажа

    [Header("Кнопка заброса")]
    [SerializeField] private InputActionReference castAction;  // Кнопка (например, E или ПКМ)

    [Header("Отладка")]
    [SerializeField] private bool showDebugRay = true;   // Показывать луч в сцене
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

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

        // Визуализация луча (будет видно в Scene View)
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * castDistance, Color.blue, 2f);
        }

        // Получаем все объекты на пути луча (не только Water слой)
        if (Physics.Raycast(ray, out hit, castDistance))
        {
            Debug.Log($"🎯 Луч попал в: {hit.collider.gameObject.name} | Слой: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            // Проверяем, попали ли мы в воду (по тегу, имени или слою)
            bool isWater = false;

            // Способ 1: проверка по тегу "Water"
            if (hit.collider.CompareTag("Water"))
                isWater = true;

            // Способ 2: проверка по имени объекта (содержит слово Water)
            if (hit.collider.name.ToLower().Contains("water"))
                isWater = true;

            // Способ 3: проверка по слою (если настроен)
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                isWater = true;

            if (isWater)
            {
                // Попали в воду - создаём поплавок
                Vector3 castPoint = hit.point;
                currentBobber = Instantiate(bobberPrefab, castPoint, Quaternion.identity);
                Debug.Log($"🎣 Заброс! Поплавок в: {castPoint}");

                if (showDebugRay)
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, hitColor, 3f);
            }
            else
            {
                Debug.Log("❌ Попал не в воду! Убедись, что у воды есть коллайдер и тег/слой 'Water'");

                if (showDebugRay)
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, missColor, 3f);
            }
        }
        else
        {
            Debug.Log($"❌ Луч никуда не попал на расстоянии {castDistance}! Увеличь дистанцию или проверь коллайдеры.");

            if (showDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * castDistance, missColor, 3f);
        }
    }

    // Удалить поплавок
    public void RemoveBobber()
    {
        if (currentBobber != null)
            Destroy(currentBobber);
        currentBobber = null;
    }
}
