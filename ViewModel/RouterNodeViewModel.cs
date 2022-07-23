using NodeGraph.Model;

namespace NodeGraph.ViewModel
{
	[NodeViewModel( ViewStyleName = "RouterNodeViewStyle" )]
	public class RouterNodeViewModel : NodeViewModel
	{
		#region Constructor

		public RouterNodeViewModel( Node node ) : base( node )
		{

		}

		#endregion // Constructor
	}
}
