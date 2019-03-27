namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     Describes how the data of a column is to be interpreted, if known.
	/// </summary>
	public enum ColumnType
	{
		/// <summary>
		///     The data contained in this column can't be interpreted.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     The name of the thread or its thread id, if no name is available.
		/// </summary>
		Thread,

		/// <summary>
		///     Used to output the WindowsIdentity for the currently active user.
		/// </summary>
		Username,

		/// <summary>
		///     Used to output the user name for the currently active user (Principal.Identity.Name).
		/// </summary>
		Identity,

		/// <summary>
		///     Used to output the application supplied message associated with the logging event.
		/// </summary>
		Message,

		/// <summary>
		///     Used to output the level of the logging event.
		/// </summary>
		Level,

		/// <summary>
		///     The column contains the name of the logger.
		/// </summary>
		Logger,

		/// <summary>
		/// 
		/// </summary>
		Newline,

		#region Time

		/// <summary>
		///     Used to output the date of the logging event in the local time zone.
		/// </summary>
		Date,

		/// <summary>
		///     Used to output the date of the logging event in universal time.
		/// </summary>
		UtcDate,

		/// <summary>
		///     Used to output the number of milliseconds elapsed since the start of the application until the creation of the logging event.
		/// </summary>
		Timestamp,

		#endregion

		#region Caller Information

		/// <summary>
		///     Used to output the file name where the logging request was issued.
		/// </summary>
		File,

		/// <summary>
		///     Used to output the fully qualified type name of the caller issuing the logging request.
		/// </summary>
		Type,

		/// <summary>
		///     Used to output location information of the caller which generated the logging event.
		/// </summary>
		Location,

		/// <summary>
		///     Used to output the line number from where the logging request was issued.
		/// </summary>
		Line,

		/// <summary>
		///     Used to output the method name where the logging request was issued.
		/// </summary>
		Method

		#endregion
	}
}