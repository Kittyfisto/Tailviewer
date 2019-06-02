using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace Tailviewer.Settings.Bookmarks
{
	public sealed class Bookmarks
		: List<BookmarkSettings>
		, IBookmarks
		, ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Task _saveTask;

		private readonly string _fileName;
		private string _fileFolder;

		static Bookmarks()
		{
			_saveTask = Task.FromResult(42);
		}

		public static Bookmarks Create()
		{
			string fileName = Path.Combine(Constants.AppDataLocalFolder, "bookmarks.xml");
			return new Bookmarks(fileName);
		}

		public Bookmarks(string fileName)
		{
			if (Path.IsPathRooted(fileName))
			{
				_fileName = fileName;
				_fileFolder = Path.GetDirectoryName(_fileName);
			}
			else
			{
				_fileFolder = Directory.GetCurrentDirectory();
				_fileName = Path.Combine(_fileFolder, fileName);
			}
		}

		public bool Save()
		{
			try
			{
				using (var stream = new MemoryStream())
				{
					var settings = new XmlWriterSettings
					{
						Indent = true,
						IndentChars = "  ",
						NewLineChars = "\r\n",
						NewLineHandling = NewLineHandling.Replace
					};
					using (XmlWriter writer = XmlWriter.Create(stream, settings))
					{
						writer.WriteStartElement("xml");

						Save(writer);

						writer.WriteEndElement();
					}

					if (!Directory.Exists(_fileFolder))
						Directory.CreateDirectory(_fileFolder);

					using (var file =
						new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						var length = (int) stream.Position;
						file.Write(stream.GetBuffer(), 0, length);
						file.SetLength(length);
					}

					return true;
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return false;
			}
		}

		public void SaveAsync()
		{
			var config = Clone();
			_saveTask = _saveTask.ContinueWith(unused => config.Save());
		}

		public void Restore()
		{
			if (!File.Exists(_fileName))
				return;

			try
			{
				using (FileStream stream = File.OpenRead(_fileName))
				using (XmlReader reader = XmlReader.Create(stream))
				{
					var bookmarks = new List<BookmarkSettings>();

					while (reader.Read())
					{
						switch (reader.Name)
						{
							case "bookmark":
								var bookmark = new BookmarkSettings();
								bookmark.Restore(reader);
								bookmarks.Add(bookmark);
								break;
						}
					}

					Clear();
					Capacity = bookmarks.Count;
					AddRange(bookmarks);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void Remove(IEnumerable<BookmarkSettings> removed)
		{
			foreach(var bookmark in removed)
			{
				Remove(bookmark);
			}
		}

		public IEnumerable<BookmarkSettings> All
		{
			get { return this.ToList(); }
		}

		private void Save(XmlWriter writer)
		{
			foreach (BookmarkSettings bookmark in this)
			{
				writer.WriteStartElement("bookmark");
				bookmark.Save(writer);
				writer.WriteEndElement();
			}
		}

		#region Implementation of ICloneable

		public Bookmarks Clone()
		{
			var clone = new Bookmarks(_fileName);
			clone.AddRange(this.Select(x => x.Clone()));
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}