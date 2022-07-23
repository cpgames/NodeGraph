using NodeGraph.View;
using System;

namespace NodeGraph.ViewModel
{
	[AttributeUsage( AttributeTargets.Class )]
	public class NodeViewModelAttribute : Attribute
	{
		public string ViewStyleName = "DefaultNodeViewStyle";
		public Type ViewType = typeof( NodeView );
	}
}
