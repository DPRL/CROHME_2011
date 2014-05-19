using System;
using DataStructures;
using System.IO;
using System.Collections.Generic;
using Parser;

namespace adjacency_stats
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			List<string> filenames = new List<string>();
			using (FileStream fs = new FileStream(args[0], FileMode.Open))
			{
				StreamReader sr = new StreamReader(fs);
				for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
					filenames.Add(line);
			}
			Dictionary<string, string> truth_to_layout = new Dictionary<string, string>();
			// load symbol classes info
			using (FileStream fs = new FileStream(args[1], FileMode.Open))
			{
				StreamReader sr = new StreamReader(fs);
				for (string s = sr.ReadLine(); s.Trim() != "##LAYOUT"; s = sr.ReadLine()) ;

				for (string s = sr.ReadLine(); s != null; s = sr.ReadLine())
				{
					s = s.Trim();
					string[] tokens = s.Split(' ', '\t');
					for (int k = 1; k < tokens.Length; k++)
						truth_to_layout[tokens[k]] = tokens[0];
				}
			}


			Console.WriteLine("String:ChildTruth,Class:Relationship,String:ParentTruth,String:Child,String:Parent,Feature:Top,Feature:Bottom,Feature:Left,Feature:Right");
			foreach(string s in filenames)
			{
				InkML inkml = InkML.NewFromFile(s);
				// parse the math expression tree
				MathExpression me = new MathExpression(inkml.mathml_expression.mathml);
				
				foreach(var edge in me.edge_list)
				{
					// root node is irrelevant here
					if(edge.Item3 == "root")
						continue;
					
					InkML.TraceGroup child = inkml.mathml_id_to_tracegroup[edge.Item1];
					MathExpression.NodeRelationship relationship = edge.Item2;
					InkML.TraceGroup parent = inkml.mathml_id_to_tracegroup[edge.Item3];
					
					Console.Write(child.truth);
					Console.Write(", ");
					Console.Write(relationship);
					Console.Write(", ");					
					Console.Write(parent.truth);
					Console.Write(", ");

					Console.Write(truth_to_layout[child.truth]);
					Console.Write(", ");

					Console.Write(truth_to_layout[parent.truth]);
					Console.Write(", ");
					
					
					AABB child_box = child.aabb;
					AABB parent_box = parent.aabb;
					
					float top = (child_box.Top - parent_box.Top) / parent_box.Height;
					float bottom = (child_box.Bottom - parent_box.Top) / parent_box.Height;
					float left = (child_box.Left - parent_box.Right) / parent_box.Width;
					float right = (child_box.Right - parent_box.Right) / parent_box.Width;
					Console.Write(top);
					Console.Write(", ");
					Console.Write(bottom);
					Console.Write(", ");
					Console.Write(left);
					Console.Write(", ");
					Console.Write(right);

					Console.WriteLine();
				}
			}
		}
	}
}
