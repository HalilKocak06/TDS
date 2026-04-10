using UnityEngine;

[CreateAssetMenu(menuName = "TDS/Dialog Graph", fileName = "DialogGraph")]
public class DialogueGraphSO : ScriptableObject
{
    public string startNodeId; //start node Id'si.
    public DialogueNodeData[] nodes; //Node listesi

}