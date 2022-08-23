using System.ComponentModel;
using NodeGraph.Model;
using NodeGraph.View;

namespace NodeGraph.ViewModel
{
    [NodeViewModel(ViewStyleName = "RouterViewStyle")]
    public class RouterViewModel : SelectableViewModel
    {
        #region Fields
        private Router _model;
        private RouterView _view;
        #endregion

        #region Properties
        public RouterView View
        {
            get => _view;
            set => _view = value;
        }

        public Router Model
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
        #endregion

        #region Constructors
        public RouterViewModel(Router router) : base(router)
        {
            Model = router;
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