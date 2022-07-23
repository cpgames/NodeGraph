﻿using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
	[AttributeUsage( AttributeTargets.Class )]
	public class ConnectorAttribute : Attribute
	{
		public Type ViewModelType = typeof( ConnectorViewModel );

		public ConnectorAttribute()
		{
			if( !typeof( ConnectorViewModel ).IsAssignableFrom( ViewModelType ) )
				throw new ArgumentException( "ViewModelType of ConnectorAttribute must be subclass of ConnectorViewModel" );
		}
	}
}
