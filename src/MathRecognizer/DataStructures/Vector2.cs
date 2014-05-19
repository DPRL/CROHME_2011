namespace DataStructures
{
	public struct Vector2
	{
		public float x;
		public float y;
		
		public Vector2(float in_x, float in_y)
		{
			this.x = in_x;
			this.y = in_y;
		}
		
		public override string ToString ()
		{
			return "(" + x + ", " + y + ")";
		}
		
		public static Vector2 operator+(Vector2 left, Vector2 right)
		{
			Vector2 result = new Vector2();
			result.x = left.x + right.x;
			result.y = left.y + right.y;
			return result;
		}
		
		public static Vector2 operator-(Vector2 left, Vector2 right)
		{
			Vector2 result = new Vector2();
			result.x = left.x - right.x;
			result.y = left.y - right.y;
			return result;
		}
		
		public static Vector2 operator*(Vector2 left, float scalar)
		{
			return new Vector2(left.x * scalar, left.y * scalar);
		}
		
		public static Vector2 operator*(float scalar, Vector2 right)
		{
			return new Vector2(right.x * scalar, right.y * scalar);
		}
		
		public static Vector2 operator/(Vector2 left, float scalar)
		{
			return new Vector2(left.x / scalar, left.y / scalar);	
		}
	}
}

