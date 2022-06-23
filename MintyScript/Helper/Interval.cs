using System;
namespace Com.Gamegestalt.MintyScript
{
	public struct Interval
	{
		public float from;
		public float to;



		public override bool Equals(Object obj)
		{
			if ((obj == null) || !this.GetType().Equals(obj.GetType()))
			{
				return false;
			}
			else
			{
				Interval i = (Interval)obj;
				return float.Equals(from, i.from)
				&& float.Equals(to, i.to);
			}
		}

		public override int GetHashCode()
		{
			return (from.GetHashCode() << 2) ^ to.GetHashCode();
		}

		public static bool operator ==(Interval interval1, Interval interval2)
		{
			return float.Equals(interval1.from, interval2.from)
				&& float.Equals(interval1.to, interval2.to);
		}

		public static bool operator !=(Interval interval1, Interval interval2)
		{
			return !float.Equals(interval1.from, interval2.from)
				|| !float.Equals(interval1.to, interval2.to);
		}
	}

}
