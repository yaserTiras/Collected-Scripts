using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VehiclesController : Singleton<VehiclesController>
{
    [SerializeField] private GroupEffect[] groupEffects;

    public int THRESHOLD = 2;
    public Action onVehiclePassed;

    private Queue<Node> nodes;
    private bool isReleasing = false;
    Vector3 startFrom;
    Vector3 to;
    VehicleColorValue c;
    private int passedVehiclesCount = 0;
    List<List<Node>> list = new List<List<Node>>();


    protected override void Awake()
    {
        base.Awake();
        nodes = new Queue<Node>();
    }

    public async void VehicleHitted(List<Node> nodes)
    {
        list.Clear();
        list = GetChainsFromList(nodes);

        if (list.Count <= 0)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            await CheckPath(list[i]);
        }
    }

    public async void CarHitted(Node node, VehicleColorValue c)
    {
        List<Node> list = new List<Node>();

        list = GetChainWithIgnore(node);

        if (list.Count <= 0)
            return;
        CarDirection dir = list[0].direction;
        list.Sort((node1, node2) =>
        {
            switch (node1.direction)  // Assuming node1's direction is representative of the whole list
            {
                case CarDirection.Forward:
                    return node2.transform.position.z.CompareTo(node1.transform.position.z);

                case CarDirection.Backward:
                    return node1.transform.position.z.CompareTo(node2.transform.position.z);

                case CarDirection.Right:
                    return node2.transform.position.x.CompareTo(node1.transform.position.x);

                case CarDirection.Left:
                    return node1.transform.position.x.CompareTo(node2.transform.position.x);

                default:
                    return 0; // Default case if there are other directions or none match
            }
        });

        Node blockageNode = GetEdgeNodeByDirection(list, dir);

        InputManager.instance.SetInput(false);
        switch (c)
        {
            case VehicleColorValue.Rainbow:
                await PaintRainbow(list);
                break;
            case VehicleColorValue.ReversDirection:
                await Flip(list);
                break;
            default:
                break;
        }
        InputManager.instance.SetInput(true);

        LevelBuilder.instance.CheckUp();
    }

    private async Task PaintRainbow(List<Node> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            list[i].ChangeColor(ColorsController.instance.GetRainbowColor(), false);
            await Task.Delay(100);
        }
    }

    private async Task Flip(List<Node> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            await list[i].Flip();
        }
    }

    public async Task CheckPath(List<Node> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return;

        CarDirection dir = nodes[0].direction;
        await Task.Yield();

        nodes.Sort((node1, node2) =>
        {
            switch (node1.direction)  
            {
                case CarDirection.Forward:
                    return node2.transform.position.z.CompareTo(node1.transform.position.z);

                case CarDirection.Backward:
                    return node1.transform.position.z.CompareTo(node2.transform.position.z);

                case CarDirection.Right:
                    return node2.transform.position.x.CompareTo(node1.transform.position.x);

                case CarDirection.Left:
                    return node1.transform.position.x.CompareTo(node2.transform.position.x);

                default:
                    return 0;
            }
        });

        Node blockageNode = GetEdgeNodeByDirection(nodes, dir);
        if (blockageNode == null) 
        {
            InputManager.instance.SetInput(false);
            for (int i = 0; i < nodes.Count; i++)
            {
                this.nodes.Enqueue(nodes[i]);
                if (nodes[i].Status == NodeStatus.InField)
                    nodes[i].SetStatus(NodeStatus.Releasing);
            }
            startFrom = nodes[0].car.position + Vector3.up * 3 + nodes[0].car.forward * 1.5f;
            to = nodes[nodes.Count - 1].car.position + Vector3.up * 3 - nodes[nodes.Count - 1].car.forward * 1.5f;
            c = nodes[0].color;

            for (int i = 0; i < groupEffects.Length; i++)
            {
                if (!groupEffects[i].IsPlaying)
                {
                    await groupEffects[i].Launch(startFrom, to, c);
                    break;
                }
            }
            AudioManager.instance.PlayAudio(AudioType.VehicleOut);
            if (!isReleasing)
                ReleaseCars();
            InputManager.instance.SetInput(true);
        }
    }

    public void UpdateData()
    {
        LevelBuilder.instance.UpdateData();
    }

    private Node GetEdgeNodeByDirection(List<Node> nodes, CarDirection direction)
    {
        Node currentNode = nodes[0];

        while (currentNode != null)
        {
            Node nextNode = GetNextNodeByDirection(currentNode, direction);

            if (nextNode == null) 
            {
                return null;
            }

            if (!nextNode.HasReleased())
            {
                return nextNode;
            }

            currentNode = nextNode;
        }

        return null;
    }

    private Node GetNextNodeByDirection(Node currentNode, CarDirection direction)
    {
        switch (direction)
        {
            case CarDirection.Forward:
                return currentNode.up;
            case CarDirection.Backward:
                return currentNode.down;
            case CarDirection.Right:
                return currentNode.right;
            case CarDirection.Left:
                return currentNode.left;
            default:
                return null;
        }
    }

    public List<List<Node>> GetChainsFromList(List<Node> nodeList)
    {
        List<List<Node>> chains = new List<List<Node>>();
        HashSet<Node> processedNodes = new HashSet<Node>();

        // Iterate through all the nodes in the list
        foreach (Node node in nodeList)
        {
            if (node == null || node.color == VehicleColorValue.White || node.HasReleased()) continue;
            if (!processedNodes.Contains(node))
            {
                List<Node> chain = GetChain(node);
                if (chain.Count > 0)
                {
                    chains.Add(chain);
                    foreach (Node n in chain)
                    {
                        processedNodes.Add(n);
                    }
                }
            }
        }
        return chains;
    }

    private IEnumerable<Node> GetValidNeighbors(Node node)
    {
        List<Node> validNeighbors = new List<Node>();
        if (node == null) return validNeighbors;

        switch (node.direction)
        {
            case CarDirection.Forward:  
                AddIfValidByCriteria(validNeighbors, node, node.up, CarDirection.Forward);
                AddIfValidByCriteria(validNeighbors, node, node.down, CarDirection.Forward);
                break;
            case CarDirection.Right: 
                AddIfValidByCriteria(validNeighbors, node, node.right, CarDirection.Right);
                AddIfValidByCriteria(validNeighbors, node, node.left, CarDirection.Right);
                break;
            case CarDirection.Backward:
                AddIfValidByCriteria(validNeighbors, node, node.up, CarDirection.Backward);
                AddIfValidByCriteria(validNeighbors, node, node.down, CarDirection.Backward);
                break;
            case CarDirection.Left:
                AddIfValidByCriteria(validNeighbors, node, node.left, CarDirection.Left);
                AddIfValidByCriteria(validNeighbors, node, node.right, CarDirection.Left);
                break;
        }
        return validNeighbors;
    }

    private void AddIfValidByCriteria(List<Node> list, Node currentNode, Node neighborNode, CarDirection direction)
    {
        if (neighborNode != null
            && neighborNode.color == currentNode.color
            && neighborNode.direction == direction
            && !neighborNode.HasReleased())
        {
            list.Add(neighborNode);
        }
    }

    private IEnumerable<Node> GetValidNeighborsWithIgnore(Node node)
    {
        List<Node> validNeighbors = new List<Node>();
        if (node == null) return validNeighbors;

        switch (node.direction)
        {
            case CarDirection.Forward:
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.up, CarDirection.Forward);
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.down, CarDirection.Forward);
                break;
            case CarDirection.Right:
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.right, CarDirection.Right);
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.left, CarDirection.Right);
                break;
            case CarDirection.Backward:
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.up, CarDirection.Backward);
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.down, CarDirection.Backward);
                break;
            case CarDirection.Left:
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.left, CarDirection.Left);
                AddIfValidByCriteriaWithIgnore(validNeighbors, node, node.right, CarDirection.Left);
                break;
        }

        return validNeighbors;
    }

    private void AddIfValidByCriteriaWithIgnore(List<Node> list, Node currentNode, Node neighborNode, CarDirection direction)
    {
        if (neighborNode != null
            && neighborNode.direction == direction
            && !neighborNode.HasReleased())
        {
            list.Add(neighborNode);
        }
    }

    public List<Node> GetChain(Node startNode)
    {
        List<Node> chain = new List<Node>();
        HashSet<Node> visitedNodes = new HashSet<Node>();
        Queue<Node> queue = new Queue<Node>();

        if (startNode == null) return chain;

        queue.Enqueue(startNode);
        visitedNodes.Add(startNode);

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();
            if (currentNode == null) continue;
            chain.Add(currentNode);

            foreach (Node neighbor in GetValidNeighbors(currentNode))
            {
                if (!visitedNodes.Contains(neighbor))
                {
                    visitedNodes.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (chain.Count >= THRESHOLD)
            return chain;

        return new List<Node>();
    }

    public List<Node> GetChainWithIgnore(Node startNode)
    {
        List<Node> chain = new List<Node>();
        HashSet<Node> visitedNodes = new HashSet<Node>();
        Queue<Node> queue = new Queue<Node>();

        if (startNode == null) return chain;

        queue.Enqueue(startNode);
        visitedNodes.Add(startNode);
        Node currentNode;
        while (queue.Count > 0)
        {
            currentNode = queue.Dequeue();
            if (currentNode == null) continue;
            chain.Add(currentNode);

            foreach (Node neighbor in GetValidNeighborsWithIgnore(currentNode))
            {
                if (!visitedNodes.Contains(neighbor))
                {
                    visitedNodes.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (chain.Count >= THRESHOLD)
            return chain;

        return new List<Node>();
    }

    public bool AllGivenNodesHaveConnections(List<Node> nodeList)
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            bool iss = HasConnection(nodeList[i]);
            if (!iss)
            {
                nodeList[i].BlinkMaterial();
                return iss;
            }
        }
        return true;
    }

    private bool HasConnection(Node node)
    {
        if (node.HasReleased())
            return true;
        return node.HasConnection();
    }

    public async void ReleaseCars()
    {
        isReleasing = true;
        Node n;
        while (nodes.Count > 0)
        {
            n = nodes.Dequeue();
            if (n.Status == NodeStatus.Releasing)
                await n.Release();
        }

        isReleasing = false;
        LevelBuilder.instance.SetConnections();
        await Task.Yield();
        UpdateData();
    }


    public void VehiclePassed()
    {
        passedVehiclesCount++;
        if (passedVehiclesCount == LevelBuilder.instance.GehiclesCount())
        {
            HCStandards.Game.InvokeEndGame(true, 0f);
        }
        if (onVehiclePassed != null)
            onVehiclePassed();
    }
}
