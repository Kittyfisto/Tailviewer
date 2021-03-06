﻿using System.Collections.Generic;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	internal sealed class LevelFlagsComparer
		: IEqualityComparer<LevelFlags>
	{
		#region Implementation of IEqualityComparer<in LevelFlags>

		public bool Equals(LevelFlags x, LevelFlags y)
		{
			return x == y;
		}

		public int GetHashCode(LevelFlags obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}
}