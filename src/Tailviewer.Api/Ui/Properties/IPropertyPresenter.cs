using System;
using System.ComponentModel;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Properties
{
	/// <summary>
	///    Responsible for presenting the value of an <see cref="IReadOnlyPropertyDescriptor"/> to the end-user
	///    and to allow the end-user to change its value in case it's an <see cref="IPropertyDescriptor"/>
	/// </summary>
	public interface IPropertyPresenter
		: INotifyPropertyChanged
	{
		/// <summary>
		///     The human readable name of this property.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		///     
		/// </summary>
		object Value { get; }

		/// <summary>
		///     Is called by Tailviewer when the property's value has changed.
		/// </summary>
		/// <remarks>
		///     This method will be called from time to time by Tailviewer on the UI thread-only.
		///     Implementations should not block for long here or they will degrade UI performance massively.
		/// </remarks>
		/// <param name="newValue"></param>
		void Update(object newValue);

		/// <summary>
		///    This event should be fired by this implementation whenever the user has changed the property and signaled the intention
		///    to have this new value be applied to the underlying log file.
		/// </summary>
		event Action<object> OnValueChanged;
	}
}
