using System;
using System.Text;
namespace DataStructures
{
	public class AABB : ICloneable
	{

		public Vector2 Center
		{
			get
			{
				return new Vector2((Left + Right) / 2.0f, (Top + Bottom) / 2.0f);
			}
		}
		public Vector2 Radius
		{
			get
			{
				return new Vector2((Right - Left) / 2.0f, (Bottom - Top) / 2.0f);
			}
		}

		public float Left;
		public float Right;
		public float Top;
		public float Bottom;
		
		public float Width
		{
			get
			{
				return Left - Right;
			}
		}
		public float Height
		{
			get
			{
				return Bottom - Top;
			}
		}
		
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<").Append(Left).Append(",").Append(Top).Append("> to <").Append(Right).Append(",").Append(Bottom).Append(">");
			return sb.ToString();
		}
		
		public object Clone() {
			AABB bb = new AABB();
			bb.Left = Left;
			bb.Right = Right;
			bb.Top = Top;
			bb.Bottom = Bottom;
			// bb.stroke = stroke;
			return bb;
		}
	}
}

