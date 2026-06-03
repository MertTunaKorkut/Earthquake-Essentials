using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

/// <summary>
/// Level 1'deki sürüklenebilir eşya davranışı.
/// Mouse/touch ile sürüklenir, çantaya bırakılabilir.
/// Yeni Input System uyumlu: EventSystem interface'leri kullanır.
/// Gereksinim: Sahnede EventSystem + Kamerada Physics2DRaycaster olmalı.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Item Verisi")]
    [Tooltip("Bu eşyanın ScriptableObject verisi")]
    public ItemData itemData;

    [Header("Sürükleme Ayarları")]
    [Tooltip("Sürüklerken obje ne kadar büyüsün")]
    public float dragScaleMultiplier = 1.15f;

    [Tooltip("Geri dönüş animasyon hızı")]
    public float returnSpeed = 10f;

    // Dahili değişkenler
    private Vector3 startPosition;
    private Vector3 originalScale;
    private int originalSortingOrder;
    private SpriteRenderer spriteRenderer;
    private bool isDragging = false;
    private bool isReturning = false;
    private bool isAccepted = false;
    private Camera mainCamera;

    /// <summary>
    /// Eşyanın şu an sürüklenip sürüklenmediğini döndürür.
    /// </summary>
    public bool IsDragging => isDragging;

    /// <summary>
    /// Eşya bırakıldığında (mouse/touch kalktığında) tetiklenir.
    /// BackpackDropZone bu event'i dinleyerek bırakma anını yakalar.
    /// </summary>
    public event Action<DraggableItem> OnDropped;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        originalScale = transform.localScale;
    }

    private void Start()
    {
        startPosition = transform.position;
        originalSortingOrder = spriteRenderer.sortingOrder;

        // ItemData'dan sprite'ı ata (eğer atanmışsa)
        if (itemData != null && itemData.itemSprite != null)
        {
            spriteRenderer.sprite = itemData.itemSprite;
        }
    }

    private void Update()
    {
        // Yanlış seçimde başlangıç pozisyonuna geri dön
        if (isReturning)
        {
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * returnSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * returnSpeed);

            if (Vector3.Distance(transform.position, startPosition) < 0.05f)
            {
                transform.position = startPosition;
                transform.localScale = originalScale;
                isReturning = false;
            }
        }
    }

    /// <summary>
    /// Pointer (mouse/touch) basıldığında çağrılır.
    /// Eski OnMouseDown'ın EventSystem karşılığı.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isAccepted || isReturning) return;

        isDragging = true;
        isReturning = false;

        Debug.Log(this.name + " is being dragged");
        // Sürüklenirken büyüt ve öne getir
        transform.localScale = originalScale * dragScaleMultiplier;
        spriteRenderer.sortingOrder = 100;
    }

    /// <summary>
    /// Pointer sürüklenirken her frame çağrılır.
    /// Eski OnMouseDrag'ın EventSystem karşılığı.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;
        transform.position = worldPos;
    }

    /// <summary>
    /// Pointer bırakıldığında çağrılır.
    /// Eski OnMouseUp'ın EventSystem karşılığı.
    /// OnDropped event'ini tetikleyerek BackpackDropZone'a haber verir.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        // Bırakıldığını herkese haber ver (BackpackDropZone dinliyor)
        OnDropped?.Invoke(this);

        // Sorting order'ı geri al (kabul edilmediyse)
        if (!isAccepted)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Eşya çantaya kabul edildiğinde çağrılır.
    /// Eşya küçülüp kaybolur.
    /// </summary>
    public void Accept()
    {
        isAccepted = true;
        isDragging = false;
        spriteRenderer.sortingOrder = originalSortingOrder;

        // Küçülüp kaybol
        StartCoroutine(AcceptAnimation());
    }

    private IEnumerator AcceptAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 currentScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(currentScale, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Eşya reddedildiğinde çağrılır.
    /// Eşya başlangıç pozisyonuna geri döner.
    /// </summary>
    public void Reject()
    {
        isDragging = false;
        isReturning = true;
        spriteRenderer.sortingOrder = originalSortingOrder;
    }
}
