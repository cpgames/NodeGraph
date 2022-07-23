﻿using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Class )]
	public class FlowChartAttribute : Attribute
	{
		public Type ViewModelType = typeof( FlowChartViewModel );

		public FlowChartAttribute()
		{
			if( !typeof( FlowChartViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of FlowChartAttribute must be subclass of FlowChartViewModel" );
		}
	}
}
