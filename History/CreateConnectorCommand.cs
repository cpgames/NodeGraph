﻿using System;

namespace NodeGraph.History
{
	public class CreateConnectorCommand : NodeGraphCommand
	{
		#region Constructor

		public CreateConnectorCommand( string name, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{

		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Guid guid = ( Guid )UndoParams;

			NodeGraphManager.DestroyConnector( guid );
		}

		public override void Redo()
		{
			NodeGraphManager.DeserializeConnector( RedoParams as string );
		}

		#endregion // Overrides NodeGraphCommand
	}
}
