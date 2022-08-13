using NodeGraph.Model;
using NodeGraph.View;

namespace NodeGraph.ViewModel
{
    [NodeViewModel(ViewStyleName = "RouterViewStyle")]
    public class RouterViewModel : ViewModelBase
    {
        #region Fields
        private Router _model;
        private bool _isSelected;

        public RouterView view;
        #endregion

        #region Properties
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

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
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
    }
}