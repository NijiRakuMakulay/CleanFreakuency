using UnityEngine;
using Photon.Pun;

public class DisassemblyActivator : MonoBehaviourPunCallbacks
{
    public bool DisassemblyReady;
    DisassemblyTable_MP disassemblyTable;

    void Awake() { disassemblyTable = GetComponentInParent<DisassemblyTable_MP>(); }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DisassemblyReady = true;
            disassemblyTable.GetDisassembler(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DisassemblyReady = false;
            disassemblyTable.RemoveDisassembler();
        }
    }
}
