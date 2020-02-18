<Query Kind="Program" />

void Main()
{	
	var input = "A_DEAD_DAD_CEDED_A_BAD_BABE_A_BEADED_ABACA_BED";
	Console.WriteLine($"Input text: {input}");
	Console.WriteLine();
	
	//get the count of each character in the input string
	var counts = GetCharacterCountsFromText(input);

	//sort the count list ascending
	var nodeList = counts
		.OrderBy(x => x.Value)
		.Select(x => new KeyValuePair<int, INode>(x.Value, new LeafNode(x.Key, x.Value)))
		.ToList();
	
	//build the tree and return the root node
	var rootNode = BuildTree(nodeList);
	
	//print the tree to the Console
	rootNode.Print();
	Console.WriteLine();
	
	//calculate the binary value of each distinct character using the tree
	Dictionary<char, string> binaryValues = input
		.ToCharArray()
		.Distinct()
		.ToDictionary(x => x, x => rootNode.GetBinary(x));
	
	//print the binary values to the Console
	Console.WriteLine(binaryValues);
	Console.WriteLine();
	
	//use the tree and binary values to compress the input text
	var compressed = string.Join("", 
		input
			.ToCharArray()
			.Select(x => binaryValues[x])
			.ToList()
		);	
	Console.WriteLine($"Compressed value: {compressed}");
	
	//calculate the compression rate based on length
	int inputLengthInBits = (input.Length*8);
	decimal compressionRate = ((decimal)compressed.Length / (decimal)inputLengthInBits) * 100;	
	
	//decompress the text again
	var decompressed = string.Empty;
	while (compressed.Length > 0)
	{
		var @char = Search(rootNode, compressed);
		decompressed += @char;
		
		var binaryValue = binaryValues[@char];
		compressed = compressed.Substring(binaryValue.Length);
	}	
	
	Console.WriteLine($"Decompressed value: {decompressed}");	
	Console.WriteLine($"Compression rate: {Math.Round(compressionRate, 2)}%");
}

char Search(INode node, string compressed)
{
	if (node is LeafNode)
	{
		return ((LeafNode)node).Char;
	}
	else if (node is SumNode)
	{
		return Search(
			compressed[0] == '0'
				? ((SumNode)node).LeftChild
				: ((SumNode)node).RightChild,
			compressed.Substring(1)
		);
	}
	else if (node is RootNode)
	{
		return Search(
			compressed[0] == '0'
				? ((RootNode)node).LeftChild
				: ((RootNode)node).RightChild,
			compressed.Substring(1)
		);
	}
	throw new Exception("Unhandled node type in Search");
}

RootNode BuildTree(List<KeyValuePair<int, INode>> nodeList)
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
	if (nodeList.Count > 2 )
	{
		return BuildTree(nodeList.OrderBy(x => x.Key).ToList());
	}
	else
	{
		var list = nodeList.OrderBy(x => x.Key).ToList();
		return new RootNode(list[0].Value, list[1].Value);
	}
}

List<KeyValuePair<char, int>> GetCharacterCountsFromText(string input)
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

public class RootNode : SumNode, INode
{
	public RootNode(INode leftChild, INode rightChild): base(leftChild, rightChild)
	{}
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
	
	public static void Print(this INode node, int indentLevel = 0)
	{		
		if (node is LeafNode)
		{
			var leafNode = (LeafNode)node;
			Console.WriteLine(ConsoleFormatter.BuildOutput(leafNode.Char.ToString(), leafNode.Frequency, indentLevel));
			return;
		}
		else if (node is SumNode)
		{
			var sumNode = (SumNode)node;
			Console.WriteLine(ConsoleFormatter.BuildOutput(sumNode.Label, sumNode.Sum, indentLevel));
			Print(sumNode.LeftChild, indentLevel + 1);
			Print(sumNode.RightChild, indentLevel + 1);
		}
		else if (node is RootNode)
		{
			var rootNode = (RootNode)node;
			Console.WriteLine(ConsoleFormatter.BuildOutput(rootNode.Label, rootNode.Sum, indentLevel));
			Print(rootNode.LeftChild, indentLevel + 1);
			Print(rootNode.RightChild, indentLevel + 1);
		}
	}
	
	public static string GetBinary(this INode node, char c, string binaryResult = "")
	{		
		if (node is LeafNode)
		{
			var leafNode = (LeafNode)node;
			return binaryResult;
		}
		else if (node is SumNode)
		{
			var sumNode = (SumNode)node;
			return sumNode.LeftChild.GetLabel().Contains(c)
				? GetBinary(sumNode.LeftChild, c, $"{binaryResult}0")
				: GetBinary(sumNode.RightChild, c, $"{binaryResult}1");
		}
		else if (node is RootNode)
		{
			var rootNode = (RootNode)node;
			return rootNode.LeftChild.GetLabel().Contains(c)
				? GetBinary(rootNode.LeftChild, c, $"{binaryResult}0")
				: GetBinary(rootNode.RightChild, c, $"{binaryResult}1");
		}
		throw new Exception("Unsupported node type");
	}
}

public static class ConsoleFormatter
{
	private static string BuildIndent(int indentLevel)
	{
		var indent = "";
		for (var i = 0; i < indentLevel; i++)
		{
			indent += "\t\t";
		}
		return indent;
	}
	
	public static string BuildOutput(string text, int count, int indentLevel)
	{
		return $"{BuildIndent(indentLevel)}'{text}': {count}";
	}
}