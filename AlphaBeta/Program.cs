#define Fail_Soft
#define AB_SSS_DUAL
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
					NegaMaxNode.p[index].index = index;
				}
			}

			NegaMaxNode.function = NegaMaxNode.Function.AB_SSS;
			float alpha = 30, beta = 40;
			switch (NegaMaxNode.function)
			{
				case NegaMaxNode.Function.BF:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
				case NegaMaxNode.Function.Alpha_beta:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
				case NegaMaxNode.Function.F2:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1], alpha, beta)}");
					break;
#if Fail_Soft
				case NegaMaxNode.Function.F3:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1], alpha, beta)}");
					break;
#endif
#if AB_SSS_DUAL
				case NegaMaxNode.Function.AB_SSS:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
				case NegaMaxNode.Function.AB_DUAL:
					Debug.Log($"Answer: {NegaMaxNode.Run(NegaMaxNode.p[1])}");
					break;
#endif
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
			BF, Alpha_beta, F2
#if Fail_Soft
			, F3
#endif
#if AB_SSS_DUAL
			, AB_SSS, AB_DUAL
#endif
		};
		public static Function function;

		/// <summary>
		/// All Nodes
		/// </summary>
		public static List<NegaMaxNode> p;

		public float? value = null;

		public int index;

#if AB_SSS_DUAL
		/// <summary>
		/// AB-SSS TT
		/// </summary>
		static HashSet<NegaMaxNode> Retrive = new HashSet<NegaMaxNode>();

		/// <summary>
		/// upper limit and lower limit of node
		/// </summary>
		float? f_Plus = null, f_Minus = null;
#endif
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
				case Function.Alpha_beta:
					return F2(position, float.MinValue, float.MaxValue);
				case Function.F2:
					return F2(position, alpha, beta);
#if Fail_Soft
				case Function.F3:
					return F3(position, alpha, beta);
#endif
#if AB_SSS_DUAL
				case Function.AB_SSS:
					Retrive.Clear();
					beta = 1e8f; // dG
					int i = 0;
					do
					{
						alpha = beta; // dR = dG
						Debug.Log($"\nPASS {++i}: dR = {alpha} dG = {beta}");
						beta = AB(position, alpha - 1, alpha);
					} while (alpha != beta);
					return beta;

				case Function.AB_DUAL:
					Retrive.Clear();
					alpha = -1e8f; // dG
					i = 0;
					do
					{
						beta = alpha; // dR = dG
						Debug.Log($"\nPASS {++i}: dR = {beta}, dG = {alpha}");
						alpha = AB(position, beta, beta + 1);
					} while (alpha != beta);
					return alpha;
#endif
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
		/// NegaMax / Alpha-beta with Fail-hard
		/// </summary>
		static float F2(NegaMaxNode position, float alpha, float beta)
		{
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
			{
				Debug.Log($"Node {position.index}: M = {H(position)} Alpha = {alpha}, Beta = {beta}");
				return H(position);
			}

			float m = alpha; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float t = -F2(position.children[i], -beta, -m); // recursive

				if (t > m)
					position.value = m = t;

				if (m >= beta)
				{
					if (i < d - 1)
						Debug.Log($"[Cutoff] Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
					else
						Debug.Log($"Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
					return m;
				}
			}
			Debug.Log($"Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
			return m;
		}

#if Fail_Soft
		/// <summary>
		/// Alpha-beta with Fail-soft
		/// </summary>
		static float F3(NegaMaxNode position, float alpha, float beta)
		{
			++time;
			int d = position.CountChild();

			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
			{
				Debug.Log($"Node {position.index}: M = {H(position)} Alpha = {alpha}, Beta = {beta}");
				return H(position);
			}

			float m = float.MinValue; // will be the max value
			for (int i = 0; i < d; i++)
			{
				float t = -F3(position.children[i], -beta, -MathF.Max(m, alpha)); // recursive

				if (t > m)
					position.value = m = t;

				if (m >= beta)
				{
					if (i < d - 1)
						Debug.Log($"[Cutoff] Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
					else
						Debug.Log($"Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
					return m;
				}
			}
			Debug.Log($"Node {position.index}: M = {m} Alpha = {alpha}, Beta = {beta}");
			return m;
		}
#endif
#if AB_SSS_DUAL
		/// <summary>
		/// Alpha-beta in AB-SSS* / AB-DUAL
		/// </summary>
		static float AB(NegaMaxNode position, float alpha, float beta)
		{
			float g = 0;

			/* Check if position is in TT and has been searched to sufﬁcient depth */
			if (Retrive.Contains(position))
				if (position.f_Plus <= alpha || ((position.f_Plus == position.f_Minus)&&position.f_Plus != null) )
				{
					Debug.Log($"Node {position.index}: fP = {position.f_Plus} Alpha = {alpha}, Beta = {beta}");
					return (float)position.f_Plus;
				}
				else if (position.f_Minus >= beta)
				{
					Debug.Log($"Node {position.index}: fM = {position.index} Alpha = {alpha}, Beta = {beta}");
					return (float)position.f_Minus;
				}

			int d = position.CountChild();
			/* Reached the maximum search depth */
			if (d == 0
				|| position.depth == cutoffThreshold
				|| time > timeLimit)
				position.f_Minus = position.f_Plus = g = (float)position.value;
			else
			{
				g = float.MinValue;

				for (int i = 0; i < d && g < beta; i++)
				{
					var c = position.children[i];

					g = MathF.Max(g, -AB(c, -beta, -alpha));
					alpha = MathF.Max(alpha, g);
				}
				if (g < beta)
					position.f_Plus = g;
				if (g > alpha)
					position.f_Minus = g;
			}
			Retrive.Add(position);

			Debug.Log($"Node {position.index}: G = {g} Alpha = {alpha}, Beta = {beta}");
			return g;
		}

#endif
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
