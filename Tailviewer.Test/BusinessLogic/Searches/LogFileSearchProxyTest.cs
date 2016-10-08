using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Searches;

namespace Tailviewer.Test.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class LogFileSearchProxyTest
	{
		private Mock<ILogFileSearch> _search;
		private Mock<ILogFileSearchListener> _listener;
		private List<LogMatch> _matches;
		private List<ILogFileSearchListener> _listeners;

		[SetUp]
		public void Setup()
		{
			_search = new Mock<ILogFileSearch>();
			_search.Setup(x => x.Matches).Returns(Enumerable.Empty<LogMatch>());
			_listeners = new List<ILogFileSearchListener>();
			_search.Setup(x => x.AddListener(It.IsAny<ILogFileSearchListener>()))
			       .Callback((ILogFileSearchListener listener) =>
				       {
					       _listeners.Add(listener);
						   listener.OnSearchModified(_search.Object, _search.Object.Matches.ToList());
				       });
			_search.Setup(x => x.RemoveListener(It.IsAny<ILogFileSearchListener>()))
			       .Callback((ILogFileSearchListener listener) => _listeners.Remove(listener));

			_listener = new Mock<ILogFileSearchListener>();
			_matches = new List<LogMatch>();
			_listener.Setup(x => x.OnSearchModified(It.IsAny<ILogFileSearch>(), It.IsAny<List<LogMatch>>()))
			         .Callback((ILogFileSearch sender, List<LogMatch> matches) =>
				         {
					         _matches.Clear();
					         _matches.AddRange(matches);
				         });
		}

		[Test]
		public void TestCtor1()
		{
			var proxy = new LogFileSearchProxy(_search.Object);
			proxy.InnerSearch.Should().BeSameAs(_search.Object);
			proxy.Matches.Should().NotBeNull();
			proxy.Matches.Should().BeEmpty();
			proxy.Count.Should().Be(0);
		}

		[Test]
		public void TestCtor2()
		{
			var proxy = new LogFileSearchProxy(_search.Object);
			_search.Verify(x => x.AddListener(It.IsAny<ILogFileSearchListener>()), Times.Once);
		}

		[Test]
		public void TestCtor3()
		{
			var proxy = new LogFileSearchProxy();
			proxy.InnerSearch.Should().BeNull();
			proxy.Matches.Should().NotBeNull();
			proxy.Matches.Should().BeEmpty();
			proxy.Count.Should().Be(0);
		}

		[Test]
		public void TestInnerSearch1()
		{
			var proxy = new LogFileSearchProxy(_search.Object);
			proxy.InnerSearch = null;
			_search.Verify(x => x.RemoveListener(It.IsAny<ILogFileSearchListener>()), Times.Once);
		}

		[Test]
		public void TestInnerSearch2()
		{
			var proxy = new LogFileSearchProxy();
			EmitSearchModified(new[] {new LogMatch(5, new LogLineMatch(4, 1))});

			proxy.AddListener(_listener.Object);
			proxy.InnerSearch = _search.Object;

			_matches.Should().Equal(new[] {new LogMatch(5, new LogLineMatch(4, 1))});
		}

		[Test]
		public void TestAddListener1()
		{
			var proxy = new LogFileSearchProxy(_search.Object);
			proxy.AddListener(_listener.Object);
			_matches.Should().BeEmpty();

			EmitSearchModified(new[] {new LogMatch(0, new LogLineMatch(5, 10))});
			_matches.Should().Equal(new[] {new LogMatch(0, new LogLineMatch(5, 10))});
		}

		[Test]
		public void TestAddListener2()
		{
			var proxy = new LogFileSearchProxy(_search.Object);
			EmitSearchModified(new[] { new LogMatch(0, new LogLineMatch(5, 10)) });

			proxy.AddListener(_listener.Object);
			_matches.Should().Equal(new[] { new LogMatch(0, new LogLineMatch(5, 10)) });
		}

		public void EmitSearchModified(IEnumerable<LogMatch> matches)
		{
			_search.Setup(x => x.Matches).Returns(matches.ToList);
			foreach (var listener in _listeners)
			{
				listener.OnSearchModified(_search.Object, matches.ToList());
			}
		}
	}
}