using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// МИНИ-ИГРА ВЫВАЖИВАНИЯ РЫБЫ
// Нужно кликать мышкой, чтобы удерживать полоску посередине
public class FightingMinigame : MonoBehaviour
{
    [Header("Настройки полоски")]
    [SerializeField] private float startPosition = 0.5f;      // Стартовая позиция (0=лево, 1=право)
    [SerializeField] private float fishPullSpeed = 0.3f;      // Сила с которой рыба тянет влево
    [SerializeField] private float clickPushSpeed = 0.5f;     // Сила клика (толкает вправо)
    [SerializeField] private float leftFailZone = 0.1f;       // Красная зона слева (0-0.2)
    [SerializeField] private float rightFailZone = 0.9f;      // Красная зона справа (0.8-1)
    [SerializeField] private float successDuration = 5f;      // Сколько секунд нужно удерживать

    [Header("Кнопка")]
    [SerializeField] private InputActionReference clickAction; // Кнопка для кликов (ЛКМ)

    [Header("UI (создастся автоматически)")]
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color failColor = Color.red;

    // Состояние игры
    private bool isMinigameActive = false;
    private float currentPosition;          // Текущая позиция полоски (0-1)
    private float successTimer;              // Таймер успешного удержания
    private bool isFishCaught = false;       // Рыба уже поймана?

    // UI элементы
    private Canvas canvas;
    private Image backgroundBar;
    private Image fillBar;
    private RectTransform fillRect;

    private void OnEnable()
    {
        if (clickAction != null)
            clickAction.action.Enable();
    }

    private void OnDisable()
    {
        if (clickAction != null)
            clickAction.action.Disable();
    }

    private void Start()
    {
        CreateUI();
        HideUI();
    }

    private void Update()
    {
        if (!isMinigameActive) return;
        if (isFishCaught) return;

        // 1. Рыба тянет влево (к нулю)
        currentPosition -= fishPullSpeed * Time.deltaTime;

        // 2. Обработка кликов
        if (clickAction != null && clickAction.action.triggered)
        {
            OnClick();
        }

        // 3. Обновляем UI
        UpdateUI();

        // 4. Проверка на проигрыш (срыв)
        if (currentPosition <= leftFailZone)
        {
            FailMinigame("Рыба перетянула! Срыв!");
            return;
        }

        if (currentPosition >= rightFailZone)
        {
            FailMinigame("Слишком быстро кликаешь! Леска порвалась!");
            return;
        }

        // 5. Проверка на победу
        if (IsInSafeZone())
        {
            successTimer += Time.deltaTime;
            if (successTimer >= successDuration)
            {
                WinMinigame();
            }
        }
        else
        {
            successTimer = 0f; // Сбрасываем таймер, если вышел из зоны
        }
    }

    private void OnClick()
    {
        // Клик толкает полоску вправо
        currentPosition += clickPushSpeed;
        currentPosition = Mathf.Clamp01(currentPosition);

        // Маленькая визуальная отдача
        if (fillRect != null)
        {
            fillRect.localScale = new Vector3(1.05f, 1.05f, 1f);
            Invoke(nameof(ResetScale), 0.05f);
        }
    }

    private void ResetScale()
    {
        if (fillRect != null)
            fillRect.localScale = Vector3.one;
    }

    private bool IsInSafeZone()
    {
        float safeZoneLeft = leftFailZone + 0.15f;
        float safeZoneRight = rightFailZone - 0.15f;
        return currentPosition >= safeZoneLeft && currentPosition <= safeZoneRight;
    }

    // ЗАПУСК МИНИ-ИГРЫ (вызывается из скрипта рыбалки после подсечки)
    public void StartMinigame()
    {
        isMinigameActive = true;
        isFishCaught = false;
        currentPosition = startPosition;
        successTimer = 0f;

        ShowUI();
        Debug.Log("🎣 НАЧАЛО ВЫВАЖИВАНИЯ! Кликай мышкой, удерживай зелёную зону!");
    }

    private void WinMinigame()
    {
        isMinigameActive = false;
        isFishCaught = true;
        HideUI();
        Debug.Log("✅ РЫБА ПОЙМАНА! Отличная работа!");

        // TODO: добавить рыбу в инвентарь, начислить опыт
    }

    private void FailMinigame(string reason)
    {
        isMinigameActive = false;
        HideUI();
        Debug.Log($"❌ {reason}");

        // TODO: убрать поплавок, рыба уплыла
    }

    // СОЗДАНИЕ UI
    // СОЗДАНИЕ UI (исправленная версия без шрифта)
    private void CreateUI()
    {
        // Создаём Canvas
        GameObject canvasObj = new GameObject("FightingCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // Создаём фон полоски
        GameObject bgObj = new GameObject("BarBackground");
        bgObj.transform.SetParent(canvas.transform);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.85f);
        bgRect.anchorMax = new Vector2(0.5f, 0.85f);
        bgRect.sizeDelta = new Vector2(400, 40);
        bgRect.anchoredPosition = Vector2.zero;

        backgroundBar = bgObj.AddComponent<Image>();
        backgroundBar.color = Color.gray;

        // Создаём заполняемую полоску
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(bgObj.transform);
        fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = new Vector2(0, 0);
        fillRect.anchoredPosition = Vector2.zero;

        fillBar = fillObj.AddComponent<Image>();
        fillBar.color = safeColor;

        // Вместо текста создадим простую рамку (чтобы не было ошибок со шрифтом)
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(bgObj.transform);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;

        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = Color.white;
        borderImage.type = Image.Type.Sliced;

        // Добавляем простой текст через GameObject (встроенный шрифт работает)
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(canvas.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.78f);
        textRect.anchorMax = new Vector2(0.5f, 0.78f);
        textRect.sizeDelta = new Vector2(400, 30);

        // Используем стандартный шрифт Unity (работает без ошибок)
        Text hintText = textObj.AddComponent<Text>();
        hintText.text = "КЛИКАЙ МЫШКОЙ! УДЕРЖИВАЙ ЗЕЛЁНУЮ ЗОНУ";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 16;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = Color.white;

        canvasObj.SetActive(false);
    }

    private void ShowUI()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    private void HideUI()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (fillRect == null) return;

        // Обновляем ширину полоски
        fillRect.anchorMax = new Vector2(currentPosition, 1);

        // Меняем цвет в зависимости от положения
        if (currentPosition <= leftFailZone || currentPosition >= rightFailZone)
        {
            fillBar.color = failColor;
        }
        else if (IsInSafeZone())
        {
            fillBar.color = safeColor;
        }
        else
        {
            fillBar.color = warningColor;
        }
    }
}