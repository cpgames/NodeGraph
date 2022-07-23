using NodeGraph.View;
using System;

namespace NodeGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class NodePropertyPortViewModelAttribute : Attribute
	{
		public string ViewStyleName = "DefaultNodePropertyPortViewStyle";
		public Type ViewType = typeof( NodePropertyPortView );
	}
}
