using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBeta
{
	class Program
	{
		static void Main(string[] args)
		{
			//var values = new[] { 41, 5, 12, 90, 101, 80, 20, 30, 34, 80, 36, 35, 50, 36, 25, 3 };
			var values = new[] { 4, 8, 3, 5, 1, 7, 2, 9, 3, 9, 2, 3, 5, 4, 2, 8 };

			NegaMaxNode.function = NegaMaxNode.Function.F;

			int depthTotal = 5;
			// use binary heap as example
			{
				int nodeTotal = (int)Math.Pow(2, depthTotal);
				int valueNodeIndex = (int)Math.Pow(2, depthTotal - 1);

				// initialize
				NegaMaxNode.p = new List<NegaMaxNode>(Enumerable.Repeat(new NegaMaxNode(), nodeTotal).ToArray());

				for (int index = 1; index < nodeTotal; index++)
				{
					if (index >= valueNodeIndex)
					{
						// leaf
						NegaMaxNode.p[index].value = values[index - valueNodeIndex];
					}
					else
					{
						// branch
						NegaMaxNode.AddNode(index, index * 2);
						NegaMaxNode.AddNode(index, index * 2 + 1);
					}
				}
			}

			Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");

			for (int i = 0; i < NegaMaxNode.p.Count; i++)
			{
				Debug.Log($"{i}: {NegaMaxNode.p[i].value}");
			}
		}
	}

	class NegaMaxNode
	{
		public enum Function
		{
			BF, F, F2, F3
		};
		public static Function function;

		/// <summary>
		/// All Nodes
		/// </summary>
		public static List<NegaMaxNode> p;

		public float? value = null;
		public float? cutoffValue = null;

		#region NotUsed
		static float time = 1;
		float depth = 1;
		static float cutoffThreshold = -1;
		#endregion

		/// <summary>
		/// Children of a node, should be add automatically, but now it's manually.
		/// </summary>
		List<NegaMaxNode> children = new List<NegaMaxNode>();

		/// <summary>
		/// Spread and count childs, should be add automatically, but now it's bypassed.
		/// </summary>
		/// <returns>Counts of children</returns>
		int CountChild() => children.Count;

		public static void AddNode(int parent, int child)
		{
			NegaMaxNode node = new NegaMaxNode()
			{
				depth = p[parent].depth + 1,
			};

			p[child] = node;

			p[parent].children.Add(node);
		}

		/// <summary>
		/// The value of Node
		/// </summary>
		/// <returns>value of Node</returns>
		static float H(NegaMaxNode position) => (float)position.value;

		/// <summary>
		/// Go through the search tree
		/// </summary>
		public static float Run(NegaMaxNode position)
		{
			switch (function)
			{
				case Function.BF:
					return BF(position);
				case Function.F:
					return (float)F(position);
				case Function.F2:
					return (float)F(position);
				case Function.F3:
					return (float)F(position);
			}
			return 0;
		}

		/// <summary>
		/// Brute-Force method
		/// </summary>
		static float BF(NegaMaxNode position)
		{
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time == 0)
				return H(position);

			float m = float.MinValue; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float t = -BF(position.children[i]); // recursive
				if (t > m)
					m = t;
			}
			position.value = m;
			return m;
		}

		/// <summary>
		/// Normal Alpha-beta
		/// </summary>
		static float? F(NegaMaxNode position, float? cutoffValue = null)
		{
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time == 0)
				return H(position);

			float m = float.MinValue; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float? t = -F(position.children[i], -position.cutoffValue); // recursive

				// cut off
				if (cutoffValue != null)
					if (t > cutoffValue)
						return null;

				if (t > m)
				{
					position.value = position.cutoffValue = m = (float)t;
				}
					
			}
			return m;
		}
	}

	#region Debug.Log
	class Debug
	{
		public static void Log(object obj)
		{
			Console.WriteLine(obj.ToString());
		}
	}
	#endregion

}
