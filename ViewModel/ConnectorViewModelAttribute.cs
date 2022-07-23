using System;

namespace ConnectorGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class ConnectorViewModelAttribute : Attribute
	{
		public string ViewStyleName;
		public ConnectorViewModelAttribute( string viewStyleName = "DefaultConnectorViewStyle" )
		{
			ViewStyleName = viewStyleName;
		}
	}
}
