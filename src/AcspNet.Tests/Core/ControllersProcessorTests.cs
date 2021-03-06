﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Claims;
using AcspNet.Core;
using AcspNet.Meta;
using AcspNet.Routing;
using AcspNet.Tests.TestEntities;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using Simplify.DI;

namespace AcspNet.Tests.Core
{
	[TestFixture]
	public class ControllersProcessorTests
	{
		private ControllersProcessor _processor;
		private Mock<IControllersAgent> _agent;
		private Mock<IControllerExecutor> _controllersExecutor;

		private readonly IDIContainerProvider _containerProvider = null;
		private Mock<IOwinContext> _context;

		private ControllerMetaData _metaData;
		private readonly IDictionary<string, Object> _routeParameters = new ExpandoObject();

		[SetUp]
		public void Initialize()
		{
			_agent = new Mock<IControllersAgent>();
			_controllersExecutor = new Mock<IControllerExecutor>();
			_processor = new ControllersProcessor(_agent.Object, _controllersExecutor.Object);

			_context = new Mock<IOwinContext>();

			_metaData = new ControllerMetaData(typeof(TestController1),
				new ControllerExecParameters(new ControllerRouteInfo("/foo/bar")));

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult(true, _routeParameters));
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData
			});

			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Ok);

			_context.SetupGet(x => x.Request.Path).Returns(new PathString("/foo/bar"));
			_context.SetupGet(x => x.Request.Method).Returns("GET");
			_context.SetupGet(x => x.Authentication).Returns(new Mock<IAuthenticationManager>().Object);
		}

		[Test]
		public void ProcessRequest_NoControllersMatchedNo404Controller_404Returned()
		{
			// Assign
			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Http404, result);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
		}

		[Test]
		public void ProcessRequest_NoControllersMatchedButHave404Controller_404ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Ok, result);
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void ProcessRequest_NoControllersMatchedButHave404ControllerRawResult_404ControllerExecutedRawReturned()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			_controllersExecutor.Setup(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>())).Returns(ControllerResponseResult.RawOutput);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.RawOutput, result);
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void ProcessRequest_NoControllersMatchedButHave404ControllerRedirect_404ControllerExecutedRedirectReturned()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult());
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			_controllersExecutor.Setup(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>())).Returns(ControllerResponseResult.Redirect);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Redirect, result);
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);

			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void ProcessRequest_OnlyAnyPageControllerMatchedButHave404Controller_404ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new RouteMatchResult(true));
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http404Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));
			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(true);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Ok, result);
			_agent.Verify(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>()));
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()));

			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
		}

		[Test]
		public void ProcessRequest_StandardControllerMatched_Executed()
		{
			// Assign
			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Ok, result);
			_agent.Verify(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>()));
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)));

			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)), Times.Never);

			_controllersExecutor.Verify(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()));
		}

		[Test]
		public void ProcessRequest_StandardControllersOneIsMatchedNull_ExecutedOnce()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.SetupSequence(
				x => x.MatchControllerRoute(It.IsAny<IControllerMetaData>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new RouteMatchResult(true, _routeParameters))
				.Returns(null);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Ok, result);
			_agent.Verify(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>()), Times.Once);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Once);

			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)), Times.Never);

			_controllersExecutor.Verify(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()), Times.Once);
		}


		[Test]
		public void ProcessRequest_StandardControllerMatchedReturnsRawData_ReturnedRawDataSubsequentNotExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersExecutor.Setup(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters))).Returns(ControllerResponseResult.RawOutput);
			
			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.RawOutput, result);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Once);
		}

		[Test]
		public void ProcessRequest_StandardControllerMatchedReturnsRedirect_ReturnedRedirectSubsequentNotExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersExecutor.Setup(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters))).Returns(ControllerResponseResult.Redirect);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Redirect, result);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Once);
		}

		[Test]
		public void ProcessRequest_StandardAsyncControllerMatchedReturnsRawData_ReturnedRawDataSubsequentExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersExecutor.Setup(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()))
				.Returns(new List<ControllerResponseResult> {ControllerResponseResult.RawOutput});

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.RawOutput, result);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Exactly(2));
		}

		[Test]
		public void ProcessRequest_StandardAsyncControllerMatchedReturnsRedirect_ReturnedRedirectSubsequentExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsAnyPageController(It.IsAny<IControllerMetaData>())).Returns(false);
			_agent.Setup(x => x.GetStandardControllersMetaData()).Returns(() => new List<IControllerMetaData>
			{
				_metaData,
				_metaData
			});

			_controllersExecutor.Setup(x => x.ProcessAsyncControllersResponses(It.IsAny<IDIContainerProvider>()))
				.Returns(new List<ControllerResponseResult> { ControllerResponseResult.Redirect });

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Redirect, result);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof(TestController1)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == _routeParameters)), Times.Exactly(2));
		}

		[Test]
		public void ProcessRequest_NotAuthenticated_ReturnedHttp401()
		{
			// Assign
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.NotAuthenticated);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Http401, result);
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}

		[Test]
		public void ProcessRequest_ForbiddenHave403Controller_403ControllerExecuted()
		{
			// Assign

			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Forbidden);
			_agent.Setup(
				x => x.GetHandlerController(It.Is<HandlerControllerType>(d => d == HandlerControllerType.Http403Handler)))
				.Returns(new ControllerMetaData(typeof(TestController2)));

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Ok, result);
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
			_controllersExecutor.Verify(
				x =>
					x.Execute(It.Is<Type>(t => t == typeof (TestController2)), It.IsAny<IDIContainerProvider>(),
						It.IsAny<IOwinContext>(), It.Is<IDictionary<string, Object>>(d => d == null)));
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}

		[Test]
		public void ProcessRequest_ForbiddenNotHave403Controller_Http403Returned()
		{
			// Assign
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>())).Returns(SecurityRuleCheckResult.Forbidden);

			// Act
			var result = _processor.ProcessControllers(_containerProvider, _context.Object);

			// Assert

			Assert.AreEqual(ControllersProcessorResult.Http403, result);
			_controllersExecutor.Verify(x =>
				x.Execute(It.Is<Type>(t => t == typeof (TestController1)), It.IsAny<IDIContainerProvider>(),
					It.IsAny<IOwinContext>(), It.IsAny<IDictionary<string, Object>>()), Times.Never);
			_agent.Setup(x => x.IsSecurityRulesViolated(It.IsAny<IControllerMetaData>(), It.IsAny<ClaimsPrincipal>()));
		}
	}
}