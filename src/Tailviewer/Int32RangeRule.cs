using System.Globalization;
using System.Windows.Controls;

namespace Tailviewer
{
	/// <summary>
	/// </summary>
	/// <remarks>
	///     TODO: Move to Metrolib
	/// </remarks>
	public sealed class Int32RangeRule
		: ValidationRule
	{
		/// <summary>
		///     The minimum possible value.
		///     A value only passes validation if it is equal or greater than the given value.
		/// </summary>
		public int Minimum { get; set; }

		/// <summary>
		///     The maximum possible value.
		///     A value only passes validation if it is equal or less than the given value.
		/// </summary>
		public int Maximum { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var content = value as string;
			if (content != null)
			{
				int parsedValue;
				if (int.TryParse(content, NumberStyles.Integer, cultureInfo, out parsedValue))
					if (parsedValue >= Minimum && parsedValue <= Maximum)
						return new ValidationResult(isValid: true, errorContent: null);
			}

			return new ValidationResult(isValid: false, errorContent: string.Format("Expected an integer between {0} and {1}", Minimum, Maximum));
		}
	}
}