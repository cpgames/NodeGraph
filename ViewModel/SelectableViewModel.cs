using System.ComponentModel;
using NodeGraph.Model;

namespace NodeGraph.ViewModel
{
    public abstract class SelectableViewModel : ViewModelBase
    {
        #region Fields
        private bool _isSelected;
        #endregion

        #region Properties
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
        protected SelectableViewModel(ModelBase model) : base(model) { }
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