<Query Kind="Program" />

void Main()
{	
	var freq = GetFrequenciesFromText("A_DEAD_DAD_CEDED_A_BAD_BABE_A_BEADED_ABACA_BED");

	var nodeList = freq
		.OrderBy(x => x.Value)
		.Select(x => new KeyValuePair<int, INode>(x.Value, new LeafNode(x.Key, x.Value)))
		.ToList();
	
	var tree = RunSort(nodeList);
	
	var rootNode = new SumNode(tree[0].Value, tree[1].Value);
	Print(rootNode);
}

void Print(INode node, int indentLevel = 0)
{
	var indent = "";
	for (var i = 0; i < indentLevel; i++)
	{
		indent += "\t\t";
	}
	indent += "";
	
	if (node is LeafNode)
	{
		var leafNode = (LeafNode)node;
		Console.WriteLine($"{indent}{leafNode.Char}: {leafNode.Frequency}");
		return;
	}
	else if (node is SumNode)
	{
		var sumNode = (SumNode)node;
		Console.WriteLine($"{indent}{sumNode.Label}: {sumNode.Sum}");
		Print(sumNode.LeftChild, indentLevel + 1);
		Print(sumNode.RightChild, indentLevel + 1);
	}
}

List<KeyValuePair<int, INode>> RunSort(List<KeyValuePair<int, INode>> nodeList)
{
	//capture then remove the top two items from the ordered list
	var first = nodeList[0];
	var second = nodeList[1];
	
	nodeList.RemoveAt(0);
	nodeList.RemoveAt(0);
	
	//create a SumNode that joins them
	var sumNode = new SumNode(first.Value, second.Value);	
	
	//add back to nodeList
	nodeList.Add(new KeyValuePair<int, INode>(sumNode.Sum, sumNode));
	
	//re-sort nodeList
	return nodeList.Count > 2 
		? RunSort(nodeList.OrderBy(x => x.Key).ToList()) 
		: nodeList.OrderBy(x => x.Key).ToList();
}

List<KeyValuePair<char, int>> GetFrequenciesFromText(string input)
{
	var values = new Dictionary<char, int>();
	foreach (char c in input)
	{
		if (values.ContainsKey(c))
		{
			values[c]++;
		}
		else
		{
			values[c] = 1;
		}
	}
	
	return values
		.Select(x => new KeyValuePair<char, int>(x.Key, x.Value))
		.ToList();
}

public class SumNode : INode
{
	private int? _sum;
	private string _label;
	
	public int Sum
	{
		get 
		{
			if (!_sum.HasValue)
			{
				_sum = LeftChild.GetSum() + RightChild.GetSum();
			}
			return _sum.Value;
		}
	}
	
	public string Label
	{
		get
		{
			if (_label == null)
			{
				_label = LeftChild.GetLabel() + RightChild.GetLabel();
			}
			return _label;
		}
	}
	
	public INode LeftChild { get; }
	public INode RightChild { get; }
	
	public SumNode(INode leftChild, INode rightChild)
	{
		LeftChild = leftChild;
		RightChild = rightChild;
	}
}

public class LeafNode : INode
{
	public int Frequency { get; }
	public char Char { get; }
	
	public LeafNode(char @char, int frequency)
	{
		Char = @char;
		Frequency = frequency;
	}
}

public interface INode {}

public static class NodeExtensions
{
	public static string GetLabel(this INode node)
	{
		return node is LeafNode ? ((LeafNode)node).Char.ToString() : ((SumNode)node).Label;
	}
	
	public static int GetSum(this INode node)
	{
		return node is LeafNode ? ((LeafNode)node).Frequency : ((SumNode)node).Sum;
	}
}