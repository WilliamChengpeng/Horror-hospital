using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("相机与射线参数")]
    public Camera playerCamera;
    public float detectRange = 5f;        // 射线长度
    public float castRadius = 0.25f;      // SphereCast 半径
    public LayerMask interactableMask;    // ✅ 可交互层（排除玩家）

    [Header("UI 控制器")]
    public GameObject interactionUI;   // ✅ 用于控制提示面板（白色 Panel）

    private IInteract curInteractObj;

    [Header("交互设置")]
    [SerializeField] private float interactTimeRequired = 2f; // 需要按住的时间（秒）

    private float interactProgress = 0f; // 当前进度（0-1）
    private float UITimer = 0f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        InteractBySphereCast();

        if (interactionUI.transform.parent.Find("Text").GetComponent<Text>().gameObject.activeSelf)
        {
            UITimer += Time.deltaTime;
        }
        if(UITimer > 1.5f)
        {
            UITimer = 0;
            interactionUI.transform.parent.Find("Text").GetComponent<Text>().gameObject.SetActive(false);
        }
    }

    private void InteractBySphereCast()
    {
        Vector3 rayOrigin = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        Ray ray = new Ray(rayOrigin, playerCamera.transform.forward);
        RaycastHit hit;

        bool hitSomething = Physics.SphereCast(ray, castRadius, out hit, detectRange, interactableMask);
        Debug.DrawRay(ray.origin, ray.direction * detectRange, hitSomething ? Color.green : Color.red);

        if (hitSomething)
        {
            IInteract tempInteractObj = hit.transform.GetComponent<IInteract>();

            if (tempInteractObj != null)
            {
                if (interactionUI != null)
                    interactionUI.SetActive(true);

                if (Input.GetKey(KeyCode.F))
                {
                    // 增加进度
                    interactProgress += Time.deltaTime / interactTimeRequired;
                    interactionUI.GetComponent<Image>().fillAmount = interactProgress;

                    // 进度完成，执行交互
                    if (interactProgress >= 1f)
                    {
                        tempInteractObj.OnInteract();
                        var showText = interactionUI.transform.parent.Find("Text").GetComponent<Text>();

                        showText.gameObject.SetActive(true);
                        showText.text = tempInteractObj.Description();

                        interactProgress = 0f;
                    }
                }
                else
                {
                    // 没有按住F键时重置进度
                    if (interactProgress > 0f)
                    {
                        interactProgress = 0f;
                    }
                }

                curInteractObj = tempInteractObj;
                return;
            }
        }

        if (curInteractObj != null)
        {
            curInteractObj = null;
            if (interactionUI != null)
                interactionUI.SetActive(false);

            // 重置进度
            interactProgress = 0f;
        }
    }
}