using System;

namespace UnityEditor.Media
{
	public struct MediaRational
	{
		public int numerator;

		public int denominator;

		public MediaRational(int num)
		{
			this.numerator = num;
			this.denominator = 1;
		}
	}
}
