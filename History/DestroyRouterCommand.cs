using System;

namespace NodeGraph.History
{
	public class DestroyRouterCommand : NodeGraphCommand
	{
		#region Constructor

		public DestroyRouterCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{

		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			NodeGraphManager.DeserializeRouter( UndoParams as string );
		}

		public override void Redo()
		{
			Guid guid = ( Guid )RedoParams;

			NodeGraphManager.DestroyRouter( guid );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
