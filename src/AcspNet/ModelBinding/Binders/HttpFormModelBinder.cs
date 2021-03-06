﻿using System.Collections.Generic;
using System.Linq;
using AcspNet.ModelBinding.Binders.Parsers;

namespace AcspNet.ModelBinding.Binders
{
	/// <summary>
	/// Provides form data to object (model) binding
	/// </summary>
	public class HttpFormModelBinder : IModelBinder
	{
		/// <summary>
		/// Binds specified form data to model.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public void Bind<T>(ModelBinderEventArgs<T> args)
		{
			if (args.Context.Request.ContentType.Contains("application/x-www-form-urlencoded"))
				args.SetModel(
					ListToModelParser.Parse<T>(args.Context.Form.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value)).ToList()));
		}
	}
}