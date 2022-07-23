using NodeGraph.Model;

namespace NodeGraph.ViewModel
{
	[NodeFlowPortViewModel()]
	public class NodeFlowPortViewModel : NodePortViewModel
	{
		#region Constructor

		public NodeFlowPortViewModel( NodeFlowPort nodeFlowPort ) : base( nodeFlowPort )
		{
		}

		#endregion // Constructor

	}
}
