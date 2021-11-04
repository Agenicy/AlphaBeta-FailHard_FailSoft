using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBeta
{
	class Program
	{
		static void Main(string[] args)
		{
			var values = new[] { 41, 5, 12, 90, 101, 80, 20, 30, 34, 80, 36, 35, 50, 36, 25, 3 };
			/*
			var values = new[] { 41, 5, 12, 90, 101, 80, 20, 30, 34, 80, 36, 35, 50, 36, 25,  3,
								21, 47, 65, 96,  32, 11, 74, 14, 59, 88, 46, 67, 32, 45 ,22, 23,
								66, 49, 98, 75, 103, 54, 66, 88, 94, 65, 14, 21, 30,  7, 85, 10,
								30, 20,  1, 85,  17,  0, 23, 29,  4, 37, 41,  8, 53, 12, 61, 67};
			*/

			int depthTotal = (int)(Math.Log(values.Length) / Math.Log(2) + 1);
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

			NegaMaxNode.function = NegaMaxNode.Function.F3;

			switch (NegaMaxNode.function)
			{
				case NegaMaxNode.Function.BF:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
				case NegaMaxNode.Function.F:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
				case NegaMaxNode.Function.F2:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1], float.MinValue, float.MaxValue)}");
					break;
				case NegaMaxNode.Function.F3:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1], float.MinValue, float.MaxValue)}");
					break;
			}

			Debug.Log($"Run time: {NegaMaxNode.time}");

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
		public static float time = 0;
		public static float timeLimit = float.MaxValue;
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
		public static float Run(NegaMaxNode position, float alpha = 0, float beta = 0)
		{
			switch (function)
			{
				case Function.BF:
					return BF(position);
				case Function.F:
					return (float)F(position);
				case Function.F2:
					return (float)F2(position, alpha, beta);
				case Function.F3:
					return (float)F3(position, alpha, beta);
			}
			return 0;
		}

		/// <summary>
		/// Brute-Force method
		/// </summary>
		static float BF(NegaMaxNode position)
		{
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
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
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
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

		/// <summary>
		/// Alpha-beta with Fail-hard
		/// </summary>
		static float? F2(NegaMaxNode position, float alpha, float beta)
		{
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
				return H(position);

			float m = alpha; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float? t = -F2(position.children[i], -beta, -m); // recursive

				if (t > m)
					position.value = m = (float)t;

				if (m >= beta)
					return m;
			}
			return m;
		}

		/// <summary>
		/// Alpha-beta with Fail-soft
		/// </summary>
		static float? F3(NegaMaxNode position, float alpha, float beta)
		{
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
				return H(position);

			float m = float.MinValue; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float? t = -F3(position.children[i], -beta, -MathF.Max(m, alpha)); // recursive

				if (t > m)
					position.value = m = (float)t;

				if (m >= beta)
					return m;
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
