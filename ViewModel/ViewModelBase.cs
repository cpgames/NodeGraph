using NodeGraph.Model;
using System.ComponentModel;

namespace NodeGraph.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		#region Overrides InotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // Overrides INotifyPropertyChanged

		#region Constructor

		public ViewModelBase( ModelBase model )
		{
			model.PropertyChanged += ModelPropertyChanged;
		}

		#endregion // Constructor

		#region Model PropertyChanged

		protected virtual void ModelPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			
		}

		#endregion // Model PropertyChanged
	}
}
