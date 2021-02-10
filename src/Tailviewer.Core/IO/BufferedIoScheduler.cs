using System;
using Metrolib;

namespace Tailviewer.Core.IO
{
	/// <summary>
	///    This class is an <see cref="IIoScheduler"/> which keeps a portion of the read data in an in-memory buffer of fixed size.
	/// </summary>
	internal sealed class BufferedIoScheduler
		: IIoScheduler
	{
		private readonly Size _maximumBufferSize;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="maximumBufferSize"></param>
		public BufferedIoScheduler(Size maximumBufferSize)
		{
			_maximumBufferSize = maximumBufferSize;
		}

		#region Implementation of IDisposable

		/// <inheritdoc />
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public ITextFileReader OpenReadText(string fileName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}