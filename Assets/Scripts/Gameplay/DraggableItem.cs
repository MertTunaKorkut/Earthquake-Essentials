using UnityEngine;
using System.Collections;

/// <summary>
/// Level 1'deki sürüklenebilir eşya davranışı.
/// Mouse/touch ile sürüklenir, çantaya bırakılabilir.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DraggableItem : MonoBehaviour
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

    private void OnMouseDown()
    {
        if (isAccepted || isReturning) return;

        isDragging = true;
        isReturning = false;

        // Sürüklenirken büyüt ve öne getir
        transform.localScale = originalScale * dragScaleMultiplier;
        spriteRenderer.sortingOrder = 100;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        // Sorting order'ı geri al (kabul edilmediyse)
        if (!isAccepted)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
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
