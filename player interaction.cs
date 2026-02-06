using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteractionSystem : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Camera playerCamera;

    [Header("아이템 조작 설정")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float distanceScrollSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 100f;

    [Header("인벤토리")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("UI 요소")]
    [SerializeField] private GameObject defaultUI;
    [SerializeField] private GameObject itemLookUI;
    [SerializeField] private GameObject itemWithInteractUI;
    [SerializeField] private GameObject interactableLookUI;
    [SerializeField] private GameObject itemHoldRotateUI;
    [SerializeField] private GameObject itemHoldDistanceUI;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemWithInteractNameText;
    [SerializeField] private TextMeshProUGUI interactableNameText;

    [Header("디버그")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private bool showDebugLog = false;

    private LayerMask combinedLayerMask;
    private GameObject currentHeldItem;
    private GameObject currentLookedObject;
    private float currentItemDistance = 2f;
    private Rigidbody heldItemRigidbody;
    private Vector3 itemRotation = Vector3.zero;

    private enum UIState
    {
        Default,
        LookingAtItem,
        LookingAtItemWithInteract,
        LookingAtInteractable,
        HoldingItemRotating,
        HoldingItemDistance
    }

    private UIState currentUIState = UIState.Default;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        combinedLayerMask = itemLayer | interactableLayer;

        if (showDebugLog)
        {
            Debug.Log($"=== 레이어 설정 ===");
            Debug.Log($"Item Layer: {itemLayer.value}");
            Debug.Log($"Interactable Layer: {interactableLayer.value}");
        }

        UpdateUI();
    }

    void Update()
    {
        if (currentHeldItem == null)
        {
            DetectInteractables();
            HandlePickupAndInteraction();
        }
        else
        {
            HandleHeldItem();
        }
    }

    void DetectInteractables()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.yellow);
        }

        if (Physics.Raycast(ray, out hit, interactionRange, combinedLayerMask, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }

            if (currentHeldItem != null && hitObject == currentHeldItem)
            {
                if (currentLookedObject != null)
                {
                    currentLookedObject = null;
                    currentUIState = UIState.Default;
                    UpdateUI();
                }
                return;
            }

            if (hitObject == currentLookedObject)
            {
                return;
            }

            currentLookedObject = hitObject;

            Item item = currentLookedObject.GetComponent<Item>();
            if (item == null)
            {
                item = currentLookedObject.GetComponentInParent<Item>();
            }

            if (item != null)
            {
                if (item.canInteractBeforePickup)
                {
                    currentUIState = UIState.LookingAtItemWithInteract;
                    if (itemWithInteractNameText != null)
                        itemWithInteractNameText.text = item.itemName;
                }
                else
                {
                    currentUIState = UIState.LookingAtItem;
                    if (itemNameText != null)
                        itemNameText.text = item.itemName;
                }

                UpdateUI();
                return;
            }

            InteractableObject interactable = currentLookedObject.GetComponent<InteractableObject>();
            if (interactable == null)
            {
                interactable = currentLookedObject.GetComponentInParent<InteractableObject>();
            }

            if (interactable != null)
            {
                currentUIState = UIState.LookingAtInteractable;
                if (interactableNameText != null)
                    interactableNameText.text = interactable.objectName;

                UpdateUI();
                return;
            }
        }
        else
        {
            if (currentLookedObject != null)
            {
                currentLookedObject = null;
                currentUIState = UIState.Default;
                UpdateUI();
            }
        }
    }

    void HandlePickupAndInteraction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if ((currentUIState == UIState.LookingAtItem ||
                 currentUIState == UIState.LookingAtItemWithInteract) &&
                currentLookedObject != null)
            {
                Item item = currentLookedObject.GetComponent<Item>();
                if (item != null && item.canBePickedUp)
                {
                    PickupItem(currentLookedObject);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"========================================");
            Debug.Log($"[PlayerInteraction] E키 눌림!");
            Debug.Log($"currentLookedObject: {(currentLookedObject != null ? currentLookedObject.name : "null")}");
            Debug.Log($"currentUIState: {currentUIState}");
            Debug.Log($"========================================");

            if (currentLookedObject != null)
            {
                if (currentUIState == UIState.LookingAtItemWithInteract)
                {
                    Item item = currentLookedObject.GetComponent<Item>();
                    if (item != null)
                    {
                        Debug.Log($"[PlayerInteraction] Item.Interact() 호출: {item.itemName}");
                        item.Interact();
                    }
                }

                else if (currentUIState == UIState.LookingAtInteractable)
                {
                    InteractableObject interactable = currentLookedObject.GetComponent<InteractableObject>();

                    // ⭐ 부모에서도 찾기 (148-151번 줄과 동일하게)
                    if (interactable == null)
                    {
                        interactable = currentLookedObject.GetComponentInParent<InteractableObject>();
                    }

                    if (interactable != null)
                    {
                        Debug.Log($"[PlayerInteraction] InteractableObject.Interact() 호출: {interactable.objectName}");
                        interactable.Interact();
                    }
                    else
                    {
                        Debug.LogError($"[PlayerInteraction] InteractableObject 컴포넌트 없음!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlayerInteraction] 상호작용 불가 상태: {currentUIState}");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerInteraction] currentLookedObject가 null!");
            }
        }
    }

    void PickupItem(GameObject item)
    {
        currentHeldItem = item;
        heldItemRigidbody = item.GetComponent<Rigidbody>();

        if (heldItemRigidbody != null)
        {
            heldItemRigidbody.isKinematic = true;
            heldItemRigidbody.useGravity = false;
        }

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null)
            itemScript = item.GetComponentInParent<Item>();

        if (itemScript != null)
        {
            itemScript.OnPickedUp(playerCamera, currentItemDistance);
        }

        if (playerInventory != null)
        {
            playerInventory.SetCanSwitchSlot(false);
        }

        currentItemDistance = 2f;
        itemRotation = item.transform.eulerAngles;
        currentUIState = UIState.HoldingItemDistance;
        UpdateUI();

        Debug.Log($"✓ 아이템 줍기: {item.name}");
    }

    void HandleHeldItem()
    {
        UIState newState;

        if (Input.GetMouseButton(1))
        {
            newState = UIState.HoldingItemRotating;
            RotateHeldItem();
        }
        else
        {
            newState = UIState.HoldingItemDistance;
        }

        if (newState != currentUIState)
        {
            currentUIState = newState;
            UpdateUI();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentItemDistance += scroll * distanceScrollSpeed;
            currentItemDistance = Mathf.Clamp(currentItemDistance, minDistance, maxDistance);

            Item itemScript = currentHeldItem.GetComponent<Item>();
            if (itemScript == null)
                itemScript = currentHeldItem.GetComponentInParent<Item>();

            if (itemScript != null)
            {
                itemScript.UpdateDistance(currentItemDistance);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            DropItem();
        }
    }

    void RotateHeldItem()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        itemRotation.y += mouseX;
        itemRotation.x -= mouseY;

        currentHeldItem.transform.eulerAngles = itemRotation;
    }

    void DropItem()
    {
        if (currentHeldItem == null) return;

        if (heldItemRigidbody != null)
        {
            heldItemRigidbody.isKinematic = false;
            heldItemRigidbody.useGravity = true;
        }

        Item itemScript = currentHeldItem.GetComponent<Item>();
        if (itemScript == null)
            itemScript = currentHeldItem.GetComponentInParent<Item>();

        if (itemScript != null)
        {
            itemScript.OnDropped();
        }

        if (playerInventory != null)
        {
            playerInventory.SetCanSwitchSlot(true);
        }

        Debug.Log($"✓ 아이템 놓기: {currentHeldItem.name}");

        currentHeldItem = null;
        heldItemRigidbody = null;
        currentUIState = UIState.Default;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (defaultUI != null) defaultUI.SetActive(false);
        if (itemLookUI != null) itemLookUI.SetActive(false);
        if (itemWithInteractUI != null) itemWithInteractUI.SetActive(false);
        if (interactableLookUI != null) interactableLookUI.SetActive(false);
        if (itemHoldRotateUI != null) itemHoldRotateUI.SetActive(false);
        if (itemHoldDistanceUI != null) itemHoldDistanceUI.SetActive(false);

        switch (currentUIState)
        {
            case UIState.Default:
                if (defaultUI != null) defaultUI.SetActive(true);
                break;

            case UIState.LookingAtItem:
                if (itemLookUI != null) itemLookUI.SetActive(true);
                break;

            case UIState.LookingAtItemWithInteract:
                if (itemWithInteractUI != null) itemWithInteractUI.SetActive(true);
                break;

            case UIState.LookingAtInteractable:
                if (interactableLookUI != null) interactableLookUI.SetActive(true);
                break;

            case UIState.HoldingItemRotating:
                if (itemHoldRotateUI != null) itemHoldRotateUI.SetActive(true);
                break;

            case UIState.HoldingItemDistance:
                if (itemHoldDistanceUI != null) itemHoldDistanceUI.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 현재 들고 있는 아이템인지 확인
    /// </summary>
    public bool IsHoldingItem()
    {
        if (currentHeldItem == null)
        {
            Debug.Log($"[InteractionSystem] IsHoldingItem() - currentHeldItem is null → False");
            return false;
        }

        // Item 컴포넌트의 isHeld 확인
        Item itemScript = currentHeldItem.GetComponent<Item>();
        if (itemScript == null)
            itemScript = currentHeldItem.GetComponentInParent<Item>();

        if (itemScript != null)
        {
            Debug.Log($"[InteractionSystem] IsHoldingItem() - {currentHeldItem.name}, isHeld: {itemScript.isHeld} → {itemScript.isHeld}");
            return itemScript.isHeld;
        }

        Debug.Log($"[InteractionSystem] IsHoldingItem() - {currentHeldItem.name}, Item script 없음 → True");
        return true;
    }

    /// <summary>
    /// 현재 들고 있는 아이템 가져오기
    /// </summary>
    public GameObject GetCurrentHeldItem()
    {
        Debug.Log($"[InteractionSystem] GetCurrentHeldItem() - 리턴: {(currentHeldItem != null ? currentHeldItem.name : "null")}");
        return currentHeldItem;
    }

    /// <summary>
    /// 인벤토리에 아이템 저장 (비활성화)
    /// </summary>
    public void StoreItemToInventory()
    {
        if (currentHeldItem == null)
            return;

        Debug.Log($"[InteractionSystem] StoreItemToInventory() - {currentHeldItem.name} 저장");

        // Item 스크립트의 isHeld만 false로 변경 (OnDropped는 호출하지 않음)
        Item itemScript = currentHeldItem.GetComponent<Item>();
        if (itemScript == null)
            itemScript = currentHeldItem.GetComponentInParent<Item>();

        if (itemScript != null)
        {
            itemScript.isHeld = false; // ⭐ 직접 설정 (OnDropped 호출 안 함)
        }

        currentHeldItem = null;
        heldItemRigidbody = null;

        if (playerInventory != null)
        {
            playerInventory.SetCanSwitchSlot(true);
        }

        currentUIState = UIState.Default;
        UpdateUI();

        Debug.Log("[InteractionSystem] 아이템 저장 완료");
    }

    /// <summary>
    /// 인벤토리에서 아이템 꺼내기
    /// </summary>
    public void TakeItemFromInventory(GameObject item)
    {
        if (item == null)
            return;

        Debug.Log($"[InteractionSystem] TakeItemFromInventory() - {item.name} 꺼내기");

        currentHeldItem = item;
        heldItemRigidbody = item.GetComponent<Rigidbody>();

        if (heldItemRigidbody != null)
        {
            heldItemRigidbody.isKinematic = true;
            heldItemRigidbody.useGravity = false;
        }

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null)
            itemScript = item.GetComponentInParent<Item>();

        if (itemScript != null)
        {
            // Item.Start()가 실행되지 않았을 수 있으므로 직접 설정
            itemScript.isHeld = true;
            itemScript.playerCamera = playerCamera;
            itemScript.targetDistance = currentItemDistance;

            // Collider를 Trigger로 설정
            Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
            foreach (Collider col in itemColliders)
            {
                col.isTrigger = true;
            }

            Debug.Log($"[InteractionSystem] 아이템 수동 설정 완료");
        }

        if (playerInventory != null)
        {
            playerInventory.SetCanSwitchSlot(false);
        }

        currentItemDistance = 2f;
        itemRotation = item.transform.eulerAngles;
        currentUIState = UIState.HoldingItemDistance;
        UpdateUI();

        Debug.Log($"[InteractionSystem] 꺼내기 완료: {item.name}");
    }
}