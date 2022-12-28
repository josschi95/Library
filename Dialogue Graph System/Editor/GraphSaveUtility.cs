using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class GraphSaveUtility
{
    private DialogueGraphView targetGraphView;
    private DialogueContainer containerCache;

    private List<Edge> edges => targetGraphView.edges.ToList();
    private List<DialogueNode> nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = targetGraphView
        };
    }

    public void LoadGraph(string fileName)
    {
        containerCache = Resources.Load<DialogueContainer>(fileName);
        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist.", "Ok");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();

        EditorWindow.GetWindow<DialogueGraphWindow>().OnFileOpened();
        EditorWindow.GetWindow<DialogueInspector>().OnFileOpened(containerCache);
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        if (!edges.Any()) return; //if there are no edges (no connections) then return;

        //Proposed fix to prevent adding/removing ports resulting in them being saved out of order
        Edge[] connectedPorts = edges.Where(edge => edge.input.node != null).OrderByDescending(edge => ((DialogueNode)(edge.output.node)).entryPoint).ToArray();
        //var connectedPorts = edges.Where(x => x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.GUID,
                portname = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var dialogueNode in nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogueNodeData
            {
                Guid = dialogueNode.GUID,
                dialogueText = dialogueNode.dialogueText,
                position = dialogueNode.GetPosition().position,
                isPlayerNode = dialogueNode.isPlayerNode,
                conditions = dialogueNode.conditions
            }); //If I'm going to run into problems, it's likely going to be with conditions here
        }

        //Auto creates resources foldier if it does not exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    //IDK I almost feel like this doesn't belong in this script
    #region - Graph Reconstruction -
    public void ClearGraph()
    {
        //Set entry points guid back from the save. Discard existing grid
        nodes.Find(x => x.entryPoint).GUID = containerCache.nodeLinks[0].baseNodeGuid;

        foreach(var node in nodes)
        {
            if (node.entryPoint) continue;

            //Remove edges that connected to this node
            edges.Where(x => x.input.node == node).ToList() //List<Edge>
                .ForEach(edge => targetGraphView.RemoveElement(edge));

            //Then remove the node
            targetGraphView.RemoveElement(node);
        }
    }

    private void CreateNodes()
    {
        foreach(var nodeData in containerCache.dialogueNodeData)
        {
            //Passing vector2.zero since position will be set later
            var tempNode = targetGraphView.CreateDialogueNode(nodeData.dialogueText, Vector2.zero);
            tempNode.GUID = nodeData.Guid;
            targetGraphView.AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => targetGraphView.AddChoicePort(tempNode, x.portname));
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(containerCache.dialogueNodeData.First(x => 
                    x.Guid == targetNodeGuid).position, targetGraphView.defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        targetGraphView.Add(tempEdge);
    }
    #endregion
}