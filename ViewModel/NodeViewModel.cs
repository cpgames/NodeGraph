using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NodeGraph.Model;
using NodeGraph.View;

namespace NodeGraph.ViewModel
{
    [NodeViewModel]
    public class NodeViewModel : ViewModelBase
    {
        #region Fields
        public Action<NodeView> NodeViewChanged;
        private NodeView _view;

        public readonly Type NodeViewType;

        private Node _Model;

        private ObservableCollection<NodeFlowPortViewModel> _InputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();

        private ObservableCollection<NodeFlowPortViewModel> _OutputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();

        private bool _IsSelected;
        #endregion

        #region Properties
        public NodeView View
        {
            get => _view;
            set
            {
                _view = value;
                NodeViewChanged?.Invoke(value);
            }
        }
        public Node Model
        {
            get => _Model;
            set
            {
                if (value != _Model)
                {
                    _Model = value;
                    RaisePropertyChanged("Model");
                }
            }
        }

        public Visibility InputFlowPortsVisibility => 0 < _InputFlowPortViewModels.Count ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OutputFlowPortsVisibility => 0 < _OutputFlowPortViewModels.Count ? Visibility.Visible : Visibility.Collapsed;
        public ObservableCollection<NodeFlowPortViewModel> InputFlowPortViewModels
        {
            get => _InputFlowPortViewModels;
            set
            {
                if (value != _InputFlowPortViewModels)
                {
                    _InputFlowPortViewModels = value;
                    RaisePropertyChanged("InputFlowPortViewModels");
                }
            }
        }
        public ObservableCollection<NodeFlowPortViewModel> OutputFlowPortViewModels
        {
            get => _OutputFlowPortViewModels;
            set
            {
                if (value != _OutputFlowPortViewModels)
                {
                    _OutputFlowPortViewModels = value;
                    RaisePropertyChanged("OutputFlowPortViewModels");
                }
            }
        }
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        #region Constructors
        public NodeViewModel(Node node) : base(node)
        {
            Model = node ?? throw new ArgumentException("Node can not be null in NodeViewModel constructor");
        }
        #endregion

        #region Methods
        #region Events
        protected override void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ModelPropertyChanged(sender, e);

            RaisePropertyChanged(e.PropertyName);
        }
        #endregion // Events
        #endregion

        #region NodePropertyPorts
        public Visibility InputPropertyPortsVisibility => 0 < _InputPropertyPortViewModels.Count ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OutputPropertyPortsVisibility => 0 < _OutputPropertyPortViewModels.Count ? Visibility.Visible : Visibility.Collapsed;

        private ObservableCollection<NodePropertyPortViewModel> _InputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
        public ObservableCollection<NodePropertyPortViewModel> InputPropertyPortViewModels
        {
            get => _InputPropertyPortViewModels;
            set
            {
                if (value != _InputPropertyPortViewModels)
                {
                    _InputPropertyPortViewModels = value;
                    RaisePropertyChanged("InputPropertyPortViewModels");
                }
            }
        }

        private ObservableCollection<NodePropertyPortViewModel> _OutputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
        public ObservableCollection<NodePropertyPortViewModel> OutputPropertyPortViewModels
        {
            get => _OutputPropertyPortViewModels;
            set
            {
                if (value != _OutputPropertyPortViewModels)
                {
                    _OutputPropertyPortViewModels = value;
                    RaisePropertyChanged("OutputPropertyPortViewModels");
                }
            }
        }
        #endregion // NodePropertyPorts
    }
}