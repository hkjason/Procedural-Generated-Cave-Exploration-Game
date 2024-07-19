using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    public List<AStarNode> data;

    public PriorityQueue()
    {
        this.data = new List<AStarNode>();
    }

    public void Enqueue(AStarNode item)
    {
        data.Add(item);
        item.qIdx = data.Count - 1; // Child index

        UpdateItem(item);
    }

    public AStarNode Dequeue()
    {
        int lastIndex = data.Count - 1;
        AStarNode frontItem = data[0]; // The root

        data[0] = data[lastIndex];
        data[0].qIdx = 0;
        data.RemoveAt(lastIndex);

        lastIndex--;
        int parentIndex = 0;

        while (true)
        {
            int leftChildIndex = 2 * parentIndex + 1;
            if (leftChildIndex > lastIndex) break;

            int rightChildIndex = leftChildIndex + 1;
            if (rightChildIndex <= lastIndex && data[rightChildIndex].CompareTo(data[leftChildIndex]) < 0)
            {
                leftChildIndex = rightChildIndex;
            }

            if (data[parentIndex].CompareTo(data[leftChildIndex]) <= 0) break;

            AStarNode temp = data[parentIndex];
            data[parentIndex] = data[leftChildIndex];
            data[parentIndex].qIdx = parentIndex;
            data[leftChildIndex] = temp;
            data[leftChildIndex].qIdx = leftChildIndex;
            parentIndex = leftChildIndex;
        }

        return frontItem;
    }

    public void UpdateItem(AStarNode node)
    {
        int parentIndex = (node.qIdx - 1) / 2;

        while (node.qIdx > 0 && data[node.qIdx].CompareTo(data[parentIndex]) < 0)
        {
            data[node.qIdx] = data[parentIndex];
            data[parentIndex] = node;

            data[node.qIdx].qIdx = node.qIdx;
            node.qIdx = parentIndex;
            parentIndex = (node.qIdx - 1) / 2;
        }
    }

    public bool TryGetNode(Vector3Int location, out AStarNode node)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].loc == location)
            {
                node = data[i];
                return true;
            }
        }
        node = null;
        return false;
    }

    public int Count
    {
        get { return data.Count; }
    }
}