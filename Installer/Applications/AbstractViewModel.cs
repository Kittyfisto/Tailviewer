using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Installer.Exceptions;

namespace Installer.Applications
{
	public abstract class AbstractViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected string FormatErrorMessage(AggregateException exception)
		{
			var builder = new StringBuilder();
			foreach (Exception inner in exception.InnerExceptions)
			{
				FormatErrorMessage(builder, inner);
			}
			return builder.ToString();
		}

		protected void FormatErrorMessage(StringBuilder builder, Exception inner)
		{
			var file = inner as FileIoException;
			if (file != null)
			{
				builder.AppendLine(inner.Message);
			}
			else
			{
				builder.AppendLine(inner.Message);
				builder.AppendLine(inner.StackTrace);
			}
		}

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}