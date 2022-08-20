using NodeGraph.Model;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NodeGraph.History
{
	public class RouterPropertyCommand : NodeGraphCommand
	{
		#region Additional information

		public Guid Guid { get; private set; }
		public string PropertyName { get; private set; }

		#endregion // additional information

		#region Constructor

		public RouterPropertyCommand( string name, Guid nodeGuid, string propertyName, object undoParams, object redoParams ) : base( name, undoParams, redoParams )
		{
			Guid = nodeGuid;
			PropertyName = propertyName;
		}

		#endregion // Constructor

		#region Overrides NodeGraphCommand

		public override void Undo()
		{
			Router router = NodeGraphManager.FindRouter( Guid );
			if( null == router )
			{
				throw new InvalidOperationException( "Router does not exist." );
			}

			if( "IsSelected" == PropertyName )
			{
				UpdateSelection( ( bool )UndoParams );
			}
			else
			{
				Type type = router.GetType();
				PropertyInfo propInfo = type.GetProperty( PropertyName );
				propInfo.SetValue( router, UndoParams );
			}
		}

		public override void Redo()
		{
            Router router = NodeGraphManager.FindRouter(Guid);
			if ( null == router)
			{
				throw new InvalidOperationException("Router does not exist.");
			}

			if( "IsSelected" == PropertyName )
			{
				UpdateSelection( ( bool )RedoParams );
			}
			else
			{
				Type type = router.GetType();
				PropertyInfo propInfo = type.GetProperty( PropertyName );
				propInfo.SetValue(router, RedoParams );
			}
		}

		#endregion // Overrides NodeGraphCommand

		#region Private Methods

		private void UpdateSelection( bool isSelected )
		{
            Router router = NodeGraphManager.FindRouter(Guid);

            ObservableCollection<Guid> selectionList = NodeGraphManager.GetSelectionList(router.Owner.FlowChart);

            router.ViewModel.IsSelected = isSelected;

			if(router.ViewModel.IsSelected )
			{
				System.Diagnostics.Debug.WriteLine( "True" );
				if( !selectionList.Contains( Guid ) )
				{
					selectionList.Add( Guid );
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine( "False" );
				if( selectionList.Contains( Guid ) )
				{
					selectionList.Remove( Guid );
				}
			}
		}

		#endregion // Private Methods
	}
}
