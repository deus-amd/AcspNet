﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AcspNet.Modules;
using Moq;
using NUnit.Framework;
using Simplify.Templates;

namespace AcspNet.Tests.Modules
{
	[TestFixture]
	public class TemplateFactoryTests
	{
		private Mock<IEnvironment> _environment;
		private Mock<ILanguageManagerProvider> _languageManagerProvider;
		private Mock<ILanguageManager> _languageManager;
		
		[TestFixtureSetUp]
		public void Initialize()
		{
			var files = new Dictionary<string, MockFileData> { { "C:/WebSites/FooSite/Templates/Foo.tpl", "Dummy data" } };

			Template.FileSystem = new MockFileSystem(files);

			_environment = new Mock<IEnvironment>();
			_languageManagerProvider = new Mock<ILanguageManagerProvider>();
			_languageManager = new Mock<ILanguageManager>();

			_environment.SetupGet(x => x.TemplatesPhysicalPath).Returns("C:/WebSites/FooSite/Templates/");
			_languageManagerProvider.Setup(x => x.Get()).Returns(_languageManager.Object);
			_languageManager.SetupGet(x => x.Language).Returns("en");
		}

		[Test]
		public void Load_NullFileName_ArgumentNullExceptionThrown()
		{
			// Assign
			var tf = new TemplateFactory(_environment.Object, _languageManagerProvider.Object, "en");

			// Act
			tf.Setup();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => tf.Load(null));
		}

		[Test]
		public void Load_NoCache_TemplateLoadedCorrectly()
		{
			// Assign
			var tf = new TemplateFactory(_environment.Object, _languageManagerProvider.Object, "en");

			// Act

			tf.Setup();
			var data = tf.Load("Foo.tpl");

			// Assert
			Assert.AreEqual("Dummy data", data.Get());
		}

		[Test]
		public void Load_NameWithoutTpl_TemplateLoadedCorrectly()
		{
			// Assign
			var tf = new TemplateFactory(_environment.Object, _languageManagerProvider.Object, "en");

			// Act

			tf.Setup();
			var data = tf.Load("Foo");

			// Assert
			Assert.AreEqual("Dummy data", data.Get());
		}

		[Test]
		public void Load_WithCache_TemplateLoadedCorrectly()
		{
			// Assign
			var tf = new TemplateFactory(_environment.Object, _languageManagerProvider.Object, "en", true);

			// Act

			tf.Setup();
			var data = tf.Load("Foo.tpl");

			// Asset
			Assert.AreEqual("Dummy data", data.Get());

			// Assign
			Template.FileSystem = new Mock<IFileSystem>().Object;

			// Act
			data = tf.Load("Foo.tpl");

			// Assert
			Assert.AreEqual("Dummy data", data.Get());
		}

		[Test]
		public void Load_FromManifestEnabled_CalledCorrectlyPathFixedWithDots()
		{
			// Assign
			var tf = new TemplateFactory(_environment.Object, _languageManagerProvider.Object, "en", true, true);

			// Act

			tf.Setup();
			var result = tf.Load("Templates/Test.tpl");

			// Assert
			Assert.AreEqual("Hello!", result.Get());
		}
	}
}