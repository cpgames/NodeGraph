using System.Collections.ObjectModel;
using System.ComponentModel;
using ConnectorGraph.ViewModel;
using NodeGraph.Model;
using NodeGraph.View;

namespace NodeGraph.ViewModel
{
    [ConnectorViewModel]
    public class ConnectorViewModel : ViewModelBase
    {
        #region Fields
        private Connector _model;
        private ObservableCollection<RouterViewModel> _routerViewModels = new ObservableCollection<RouterViewModel>();
        public ConnectorView view;
        #endregion

        #region Properties
        public Connector Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged("Model");
                }
            }
        }

        public ObservableCollection<RouterViewModel> RouterViewModels
        {
            get => _routerViewModels;
            set
            {
                if (value != _routerViewModels)
                {
                    _routerViewModels = value;
                    RaisePropertyChanged("RouterViewModels");
                }
            }
        }
        #endregion

        #region Constructors
        public ConnectorViewModel(Connector connection) : base(connection)
        {
            Model = connection;
        }
        #endregion

        #region Methods
        protected override void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ModelPropertyChanged(sender, e);

            RaisePropertyChanged(e.PropertyName);
        }
        #endregion
    }
}