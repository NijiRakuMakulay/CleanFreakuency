using UnityEngine;

public class DisassemblyTable : MonoBehaviour
{
    public Transform placePoint;

    public Camera playerCamera;
    public Camera tableCamera;

    public FPS_Controller fpsController;
    public PickupController pickupController;

    public DisassemblyManager disassemblyManager;

    private GameObject currentObject;

    void OnTriggerEnter(Collider other)
    {
        if (currentObject != null)
            return;

        SpecialTrash special =
            other.GetComponent<SpecialTrash>();

        Rigidbody rb =
            other.GetComponent<Rigidbody>();

        if (special != null && rb != null)
        {
            currentObject =
                other.gameObject;

            StartDisassembly(rb);
        }
    }

    void StartDisassembly(Rigidbody rb)
    {
        pickupController.DropHeldExternally();

        rb.isKinematic = true;
        rb.useGravity = false;

        rb.transform.position = placePoint.position;

        rb.transform.rotation = placePoint.rotation;

        playerCamera.gameObject.SetActive(false);
        tableCamera.gameObject.SetActive(true);

        fpsController.canMove = false;
        fpsController.disassemblyMode = true;

        disassemblyManager.BeginDisassembly(
            rb.gameObject
        );
    }

    public void EndDisassembly()
    {
        playerCamera.gameObject.SetActive(true);
        tableCamera.gameObject.SetActive(false);

        fpsController.canMove = true;
        fpsController.disassemblyMode = false;

        currentObject = null;
    }
}