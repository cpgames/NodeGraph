using NodeGraph.View;
using System;

namespace NodeGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class NodeFlowPortViewModelAttribute : Attribute
	{
		public string ViewStyleName = "DefaultNodeFlowPortViewStyle";
		public Type ViewType = typeof( NodeFlowPortView );
	}
}
