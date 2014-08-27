﻿using System;
using System.Threading.Tasks;
using AcspNet.Bootstrapper;
using AcspNet.Core;
using AcspNet.Diagnostics;
using AcspNet.Modules;
using Microsoft.Owin;
using Simplify.DI;

namespace AcspNet.Owin
{
	/// <summary>
	/// AcspNet engine root
	/// </summary>
	public class AcspNetOwinMiddleware : OwinMiddleware
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AcspNetOwinMiddleware"/> class.
		/// </summary>
		/// <param name="next">The next middleware.</param>
		/// <param name="bootstrapper">The bootstrapper.</param>
		public AcspNetOwinMiddleware(OwinMiddleware next, BaseAcspNetBootstrapper bootstrapper)
			: base(next)
		{
			bootstrapper.Register();
		}

		/// <summary>
		/// Process an individual request.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override Task Invoke(IOwinContext context)
		{
			using (var scope = DIContainer.Current.BeginLifetimeScope())
			{
				try
				{
					// Starts execution measurement
					scope.Container.Resolve<IStopwatchProvider>().StartMeasurement();

					// Setup providers

					var acspNetContextProvider = scope.Container.Resolve<IAcspNetContextProvider>();
					var languageManagerProvider = scope.Container.Resolve<ILanguageManagerProvider>();

					acspNetContextProvider.Setup(context);
					languageManagerProvider.Setup(context);

					// Run request process pipeline

					var requestHandler = scope.Container.Resolve<IRequestHandler>();
					return requestHandler.ProcessRequest(scope.Container, context);
				}
				catch (Exception e)
				{
					return
						context.Response.WriteAsync(ExceptionInfoPageGenerator.Generate(e,
							scope.Container.Resolve<IAcspNetSettings>().HideExceptionDetails));
				}
			}
		}
	}
}