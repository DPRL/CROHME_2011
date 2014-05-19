using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace DataStructures
{
	public class MathExpression
	{
		public enum NodeRelationship
		{
			Unknown,
			Right,
			SuperScript,
			SubScript,
			Below,
			Above,
			Under,
		};
		
		public class MathNode
		{
			public NodeRelationship to_parent = NodeRelationship.Unknown;
			public string id = "";
			public MathNode parent = null;
			public List<MathNode> children = new List<MathNode>();
			
			
		}
		
		public List<Tuple<string, NodeRelationship, string>> edge_list = new List<Tuple<string, NodeRelationship, string>>();
		delegate void BuildTree(MathNode mn, XNode xm);
		public MathExpression (string mathml)
		{
			//Dictionary<string, MathNode> id_to_mathnode = new Dictionary<string, MathNode>();
			XDocument doc = XDocument.Parse(mathml);
			traverse_tree(doc.FirstNode as XElement, "root", NodeRelationship.Right);
		}
		
		private static XNamespace ns = "http://www.w3.org/XML/1998/namespace";
		private string traverse_tree(XElement node, string parent_id, NodeRelationship relationship)
		{
			// get this nodes infko
			string tagname = node.Name.LocalName;
			if(tagname == "math")
			{
				traverse_tree(node.FirstNode as XElement, parent_id, NodeRelationship.Right);
				return "";
			}
			
			string my_id = node.Attribute(ns + "id") == null ? null : (string)(node.Attribute(ns + "id").Value);
			
			// relationsip
			if(my_id != null)
			{
				//Console.WriteLine(my_id + " is " + relationship + " of " + parent_id);
				edge_list.Add(new Tuple<string, NodeRelationship, string>(my_id, relationship, parent_id));	
			}
			
			// handle children
			switch(tagname)
			{
			case "mfrac":
				traverse_tree(node.FirstNode as XElement, my_id, NodeRelationship.Above);
				traverse_tree(node.LastNode as XElement, my_id, NodeRelationship.Below);
				return my_id;
			case "mrow":
				string left_id = traverse_tree(node.FirstNode as XElement, parent_id, relationship);
				traverse_tree(node.LastNode as XElement, left_id, NodeRelationship.Right);
				return left_id;
			case "msup":
				string super_base_id = traverse_tree(node.FirstNode as XElement, parent_id, relationship);
				traverse_tree(node.LastNode as XElement, super_base_id, NodeRelationship.SuperScript);
				return super_base_id;
			case "msub":
				string sub_base_id = traverse_tree(node.FirstNode as XElement, parent_id, relationship);
				traverse_tree(node.LastNode as XElement, sub_base_id, NodeRelationship.SubScript);
				return sub_base_id;
			case "msqrt":
				string first_under = traverse_tree(node.FirstNode as XElement, my_id, NodeRelationship.Under);
				if(node.LastNode != node.FirstNode)
					traverse_tree(node.FirstNode.NextNode as XElement, first_under, NodeRelationship.Right);
				return my_id;
			case "msubsup":
				var child = node.FirstNode;
				string sub_sup_base_id = traverse_tree(child as XElement, parent_id, relationship);
				traverse_tree(child.NextNode as XElement, sub_sup_base_id, NodeRelationship.Below);
				traverse_tree(child.NextNode.NextNode as XElement, sub_sup_base_id, NodeRelationship.Above);
				return sub_sup_base_id;
			case "munderover":
				child = node.FirstNode;
				string under_over_base_id = traverse_tree(child as XElement, parent_id, relationship);
				traverse_tree(child.NextNode as XElement, under_over_base_id, NodeRelationship.Below);
				traverse_tree(child.NextNode.NextNode as XElement, under_over_base_id, NodeRelationship.Above);
				return under_over_base_id;
			case "munder":
				string top_base_id = traverse_tree(node.FirstNode as XElement, parent_id, relationship);
				traverse_tree(node.LastNode as XElement, top_base_id, NodeRelationship.Below);
				return top_base_id;
			}
			// everything esle
			return my_id;
		}
	}
}

