using Photon.Pun;
using UnityEngine;

public class DisassemblyTable_MP : MonoBehaviourPunCallbacks
{
    public Transform placePoint;

    public Camera playerCamera;
    public Camera tableCamera;
    public GameObject Disassembler;
    public FPS_Controller fpsController;
    public PickupController pickupController;

    public DisassemblyManager_MP disassemblyManager;
    public DisassemblyActivator disassemblyActivator;
    private GameObject currentObject;

    void OnTriggerEnter(Collider other)
    {
        if (currentObject != null) return;
        SpecialTrash special = other.GetComponent<SpecialTrash>();

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (special != null && rb != null)
        {
            currentObject = other.gameObject;
            if (disassemblyActivator.DisassemblyReady) { StartDisassembly(rb); }
        }
    }

    public void GetDisassembler(GameObject PlantMan)
    {
        Disassembler = PlantMan;
        fpsController = Disassembler.GetComponent<FPS_Controller>();
        pickupController = Disassembler.GetComponentInChildren<PickupController>();
        playerCamera = Disassembler.GetComponentInChildren<Camera>();
    }

    public void RemoveDisassembler()
    {
        fpsController = null;
        pickupController = null;
        playerCamera = null;
        Disassembler = null;
    }

    void StartDisassembly(Rigidbody rb)
    {
        if (Disassembler.GetComponent<PhotonView>().IsMine)
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

            disassemblyManager.BeginDisassembly(rb.gameObject, Disassembler.GetComponent<PhotonView>());
        }
    }

    public void EndDisassembly()
    {
        if (Disassembler.GetComponent<PhotonView>().IsMine)
        {
            playerCamera.gameObject.SetActive(true);
            tableCamera.gameObject.SetActive(false);

            fpsController.canMove = true;
            fpsController.disassemblyMode = false;
            
        }
        currentObject = null;
        RemoveDisassembler();
        
    }
}