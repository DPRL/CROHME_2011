using System;

namespace DataStructures
{
	public class Tuple<T>
	{
	    public Tuple(T item1)
	    {
	        Item1 = item1;
	    }
	    
	    public T Item1 { get; set; }
	}
	
	public class Tuple<T, T2> 
	        : Tuple<T>
	{
	    public Tuple(T item1, T2 item2)
	        : base(item1)
	    {
	        Item2 = item2;
	    }
	    
	    public T2 Item2 { get; set; }
	}
	
	public class Tuple<T, T2, T3> 
	        : Tuple<T, T2>
	{
	    public Tuple(T item1, T2 item2, T3 item3)
	        : base(item1, item2)
	    {
	        Item3 = item3;
	    }
	
	    public T3 Item3 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4> 
	        : Tuple<T, T2, T3>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4)
	        : base(item1, item2, item3)
	    {
	        Item4 = item4;
	    }
	
	    public T4 Item4 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5> 
	        : Tuple<T, T2, T3, T4>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, T5 item5)
	        : base(item1, item2, item3, item4)
	    {
	        Item5 = item5;
	    }
	
	    public T5 Item5 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5, T6> 
	        : Tuple<T, T2, T3, T4, T5>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
	        : base(item1, item2, item3, item4, item5)
	    {
	        Item6 = item6;
	    }
	
	    public T6 Item6 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5, T6, T7> 
	        : Tuple<T, T2, T3, T4, T5, T6>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
	        : base(item1, item2, item3, item4, item5, item6)
	    {
	        Item7 = item7;
	    }
	
	    public T7 Item7 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5, T6, T7, T8> 
	        : Tuple<T, T2, T3, T4, T5, T6, T7>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
	        : base(item1, item2, item3, item4, item5, item6, item7)
	    {
	        Item8 = item8;
	    }
	
	    public T8 Item8 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5, T6, T7, T8, T9> 
	        : Tuple<T, T2, T3, T4, T5, T6, T7, T8>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, 
	                 T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
	        : base(item1, item2, item3, item4, item5, item6, item7, item8)
	    {
	        Item9 = item9;
	    }
	
	    public T9 Item9 { get; set; }
	}
	
	public class Tuple<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> 
	        : Tuple<T, T2, T3, T4, T5, T6, T7, T8, T9>
	{
	    public Tuple(T item1, T2 item2, T3 item3, T4 item4, 
	                 T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
	        : base(item1, item2, item3, item4, item5, item6, item7, item8, item9)
	    {
	        Item10 = item10;
	    }
	
	    public T10 Item10 { get; set; }
	}
}

