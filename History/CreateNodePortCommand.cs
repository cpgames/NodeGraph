using System;

namespace NodeGraph.History
{
	public class CreateNodePortCommand : NodeGraphCommand
	{
		#region Constructor

		public CreateNodePortCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{
		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Guid guid = ( Guid )UndoParams;

			NodeGraphManager.DestroyNodePort( guid );
		}

		public override void Redo()
		{
			NodeGraphManager.DeserializeNodePort( RedoParams as string );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
