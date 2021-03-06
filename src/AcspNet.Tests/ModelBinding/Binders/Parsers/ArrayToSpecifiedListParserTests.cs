﻿using System.Collections.Generic;
using AcspNet.ModelBinding;
using AcspNet.ModelBinding.Binders.Parsers;
using AcspNet.Tests.TestEntities;
using NUnit.Framework;

namespace AcspNet.Tests.ModelBinding.Binders.Parsers
{
	[TestFixture]
	public class ArrayToSpecifiedListParserTests
	{
		[Test]
		public void IsTypeValidForParsing_IntList_True()
		{
			Assert.IsTrue(ArrayToSpecifiedListParser.IsTypeValidForParsing(typeof(IList<int>)));
		}

		[Test]
		public void IsTypeValidForParsing_EnumList_True()
		{
			Assert.IsTrue(ArrayToSpecifiedListParser.IsTypeValidForParsing(typeof(IList<TestEnum>)));
		}

		[Test]
		public void IsTypeValidForParsing_UndefinedType_False()
		{
			Assert.IsFalse(ArrayToSpecifiedListParser.IsTypeValidForParsing(typeof(TestController1)));
		}

		[Test]
		public void IsTypeValidForParsing_UndefinedGenericType_False()
		{
			Assert.Throws<ModelBindingException>(() => ArrayToSpecifiedListParser.IsTypeValidForParsing(typeof(IList<TestController1>)));
		}

		[Test]
		public void ParseUndefined_EnumList_ParsedCorrectly()
		{
			// Act
			var result = (IList<TestEnum>)ArrayToSpecifiedListParser.ParseUndefined(new[] { "2", "1" }, typeof(IList<TestEnum>));

			// Assert

			Assert.AreEqual(TestEnum.Value2, result[0]);
			Assert.AreEqual(TestEnum.Value1, result[1]);
		}
	}
}