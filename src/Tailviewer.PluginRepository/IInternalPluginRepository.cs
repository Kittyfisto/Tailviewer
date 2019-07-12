using System;
using System.Collections.Generic;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Entities;

namespace Tailviewer.PluginRepository
{
	public interface IInternalPluginRepository
		: IPluginRepository
	{
		Guid AddUser(string username, string email);
		void RemoveUser(string username);
		IEnumerable<User> GetAllUsers();
		bool TryGetAccessToken(string username, out Guid accessToken);

		void PublishPlugin(byte[] plugin, string accessToken, string publishTimestamp);
		long CountPlugins();
		void RemovePlugin(string id, string version);
		IReadOnlyList<PublishedPluginDescription> GetAllPlugins();
	}
}
