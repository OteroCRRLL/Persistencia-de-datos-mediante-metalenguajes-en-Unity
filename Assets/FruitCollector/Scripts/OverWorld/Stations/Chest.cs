using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D), typeof(Animator))]
public sealed class Chest : MonoBehaviour, IInteractable
{
    public static readonly int ANIMATOR_OPENED_HASH = Animator.StringToHash("Opened");

    [SerializeField] private string chestId = "chest_01";
    [SerializeField] private int maxInventorySlots = 10;

    private EInteractionState InteractionState;
    private Collider2D triggerCollider;
    private Animator animator;
    private IInteractor currentInteractor;

    public string ChestId => chestId;
    public InventoryContainer Inventory { get; private set; }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        animator = GetComponent<Animator>();
        Inventory = new InventoryContainer(maxInventorySlots);
    }


    public void Interact(IInteractor interactor)
    {
        if (interactor == null) return;

        Debug.Log($"Chest '{chestId}' interacted by {interactor.Transform.name}");
        currentInteractor = interactor;
        
        if (!animator.GetBool(ANIMATOR_OPENED_HASH)) Open();
        else Close();
    }


    private void Open()
    {
        InteractionState = EInteractionState.INTERACTING;
        animator.SetBool(ANIMATOR_OPENED_HASH, true);

        PlayerInventory playerInventory = currentInteractor.Transform.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            TransferUIManager.Instance.OpenTransferUI(playerInventory, this);
        }
    }


    private void Close()
    {
        InteractionState = EInteractionState.FINISHED;
        animator.SetBool(ANIMATOR_OPENED_HASH, false);
        currentInteractor = null;
        
        if (TransferUIManager.Instance != null && TransferUIManager.Instance.IsOpen)
        {
            TransferUIManager.Instance.CloseTransferUI();
        }
    }


    public EInteractionState GetInteractionState()
    {
        return InteractionState;
    }
}