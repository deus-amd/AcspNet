﻿using System;
using System.Collections.Generic;
using AcspNet.Modules;

namespace AcspNet.Core
{
	/// <summary>
	/// Provides string table items setter
	/// </summary>
	public class StringTableItemsSetter : IStringTableItemsSetter
	{
		private const string StringTablePrefix = "StringTable.";

		private readonly IDataCollector _dataCollector;
		private readonly IStringTable _stringTable;

		/// <summary>
		/// Initializes a new instance of the <see cref="StringTableItemsSetter"/> class.
		/// </summary>
		/// <param name="dataCollector">The data collector.</param>
		/// <param name="stringTable">The string table.</param>
		public StringTableItemsSetter(IDataCollector dataCollector, IStringTable stringTable)
		{
			_dataCollector = dataCollector;
			_stringTable = stringTable;
		}

		/// <summary>
		/// Sets this items from string table to data collector.
		/// </summary>
		public void Set()
		{
			foreach (var item in (IDictionary<string, Object>)_stringTable.Items)
				_dataCollector.Add(StringTablePrefix + item.Key, item.Value.ToString());
		}
	}
}