using System;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfo
		: IEquatable<QuickInfo>
	{
		public readonly string Message;
		public readonly DateTime? Timestamp;

		public QuickInfo(string message)
			: this(message, null)
		{}

		public QuickInfo(string message, DateTime? timestamp)
		{
			Message = message;
			Timestamp = timestamp;
		}

		public bool Equals(QuickInfo other)
		{
			if (ReferenceEquals(objA: null, objB: other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Message, other.Message) && Timestamp.Equals(other.Timestamp);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is QuickInfo && Equals((QuickInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Message != null ? Message.GetHashCode() : 0) * 397) ^ Timestamp.GetHashCode();
			}
		}

		public static bool operator ==(QuickInfo left, QuickInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(QuickInfo left, QuickInfo right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return Message ?? string.Empty;
		}
	}
}