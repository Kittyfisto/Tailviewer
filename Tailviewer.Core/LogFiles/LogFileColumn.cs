using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class LogFileColumn<T>
		: ILogFileColumn<T>
	{
		private readonly object _id;
		private readonly string _name;

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		public LogFileColumn(object id, string name)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			_id = id;
			_name = name;
		}

		/// <inheritdoc />
		public object Id => _id;

		/// <inheritdoc />
		public string Name => _name;
	}
}