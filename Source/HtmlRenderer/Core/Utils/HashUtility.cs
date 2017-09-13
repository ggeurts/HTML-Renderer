namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
	internal static class HashUtility
	{
		public static int Hash(int hashCode1, int hashCode2)
		{
			return hashCode1 ^ hashCode2.RotateLeft(16);
		}

		public static int RotateLeft(this int value, int count)
		{
			return (value << count) | (value >> (32 - count));
		}
	}
}