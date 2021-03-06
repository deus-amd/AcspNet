﻿using System;

namespace AcspNet.Attributes
{
	/// <summary>
	/// Set controller execution priority
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PriorityAttribute : Attribute
	{
		/// <summary>
		/// Gets the priority.
		/// </summary>
		/// <value>
		/// The priority.
		/// </value>
		public int Priority { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PriorityAttribute"/> class.
		/// </summary>
		/// <param name="priority">The execution priority.</param>
		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}
	}
}