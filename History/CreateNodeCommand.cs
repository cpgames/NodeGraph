﻿using System;

namespace NodeGraph.History
{
	public class CreateNodeCommand : NodeGraphCommand
	{
		#region Constructor

		public CreateNodeCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{

		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Guid guid = ( Guid )UndoParams;

			NodeGraphManager.DestroyNode( guid );
		}

		public override void Redo()
		{
			NodeGraphManager.DeserializeNode( RedoParams as string );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
