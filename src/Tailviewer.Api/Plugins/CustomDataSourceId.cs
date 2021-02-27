using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     Uniquely identifies a <see cref="ICustomDataSourcePlugin" /> implementation.
	/// </summary>
	/// <remarks>
	///     The string itself should be unique enough so the chances that another plugin picks the same value is very small.
	///     Further more, once a plugin is released to the public, then this value shouldn't be changed anymore because it
	///     is used to properly restore a previously added custom data source when tailviewer is restarted.
	/// </remarks>
	public sealed class CustomDataSourceId
	{
		/// <summary>
		/// </summary>
		public readonly string Id;

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public CustomDataSourceId(string id)
		{
			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("The id should neither be null, nor empty", "id");

			Id = id;
		}

		#region Overrides of Object

		/// <inheritdoc />
		public override string ToString()
		{
			return Id;
		}

		#endregion

		#region Equality members

		private bool Equals(CustomDataSourceId other)
		{
			return string.Equals(Id, other.Id);
		}

		/// <summary>
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is CustomDataSourceId other && Equals(other);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return Id != null ? Id.GetHashCode() : 0;
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(CustomDataSourceId left, CustomDataSourceId right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(CustomDataSourceId left, CustomDataSourceId right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
} 