using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     This class implements one of the layers of isolation between a third-party plugin
	///     and tailviewer: The ILogFile interface prohibits exceptions to occur, however we cannot
	///     rely on that to never happen. This implementation guarantuees the nothrow contract and also
	///     writes warnings to the log file.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateNoThrowLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class NoThrowLogSource
		: ILogSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogSource _logSource;
		private readonly string _pluginName;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Takes ownership of the given <paramref name="logSource" /> and disposes it when disposed of.
		/// </remarks>
		/// <param name="logSource">The log file to represent</param>
		/// <param name="pluginName">
		///     The plugin from which <paramref name="logSource" /> has been created, is used to blame that specific
		///     plugin the log file output
		/// </param>
		public NoThrowLogSource(ILogSource logSource, string pluginName)
		{
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));
			if (pluginName == null)
				throw new ArgumentNullException(nameof(pluginName));

			_logSource = logSource;
			_pluginName = pluginName;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			try
			{
				_logSource.Dispose();
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				try
				{
					return _logSource.Columns;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return new IColumnDescriptor[0];
				}
			}
		}

		/// <inheritdoc />
		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			try
			{
				_logSource.AddListener(listener, maximumWaitTime, maximumLineCount);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void RemoveListener(ILogSourceListener listener)
		{
			try
			{
				_logSource.RemoveListener(listener);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				try
				{
					return _logSource.Properties;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return new IReadOnlyPropertyDescriptor[0];
				}
			}
		}

		/// <inheritdoc />
		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			try
			{
				return _logSource.GetProperty(property);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return property.DefaultValue;
			}
		}

		/// <inheritdoc />
		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			try
			{
				return _logSource.GetProperty(property);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return property.DefaultValue;
			}
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			try
			{
				_logSource.SetProperty(property, value);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			try
			{
				_logSource.SetProperty(property, value);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetAllProperties(IPropertiesBuffer destination)
		{
			try
			{
				_logSource.GetAllProperties(destination);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				throw;
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			try
			{
				_logSource.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			try
			{
				_logSource.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			try
			{
				return _logSource.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return new LogLineIndex();
			}
		}

		private void BlameExceptionOnPlugin(Exception exception)
		{
			Log.WarnFormat("Plugin {0} threw an exception: {1}", _pluginName, exception);
		}
	}
}