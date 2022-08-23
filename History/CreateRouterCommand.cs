using System;

namespace NodeGraph.History
{
	public class CreateRouterCommand : NodeGraphCommand
	{
		#region Constructor

		public CreateRouterCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{

		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Guid guid = ( Guid )UndoParams;

			NodeGraphManager.DestroyRouter( guid );
		}

		public override void Redo()
		{
			NodeGraphManager.DeserializeRouter( RedoParams as string );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
