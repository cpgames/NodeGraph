using System;
using System.Xml;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    public interface ISelectable
    {
        #region Properties
        Guid Guid { get; }
        FlowChart Owner { get; }
        bool IsSelected { get; set; }
        double X { get; set; }
        double Y { get; set; }
        int ZIndex { get; set; }
        double ActualWidth { get; }
        double ActualHeight { get; }
        #endregion

        #region Methods
        void OnCanvasRenderTransformChanged();
        #endregion
    }
    public abstract class Selectable<TViewModel> : ModelBase, ISelectable
        where TViewModel : SelectableViewModel
    {
        #region Fields
        protected TViewModel _viewModel;
        protected double _x;
        protected double _y;
        protected int _zIndex = 1;
        #endregion

        #region Properties
        public TViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (value != _viewModel)
                {
                    _viewModel = value;
                    RaisePropertyChanged("ViewModel");
                }
            }
        }
        #endregion

        #region Constructors
        protected Selectable(Guid guid, FlowChart flowChart)
            : base(guid)
        {
            Owner = flowChart;
        }
        #endregion

        #region ISelectable Members
        public FlowChart Owner { get; }
        public bool IsSelected
        {
            get => ViewModel.IsSelected;
            set => ViewModel.IsSelected = value;
        }
        public double X
        {
            get => _x;
            set
            {
                if (value != _x)
                {
                    _x = value;
                    RaisePropertyChanged("X");
                }
            }
        }
        public double Y
        {
            get => _y;
            set
            {
                if (value != _y)
                {
                    _y = value;
                    RaisePropertyChanged("Y");
                }
            }
        }
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (value != _zIndex)
                {
                    _zIndex = value;
                    RaisePropertyChanged("ZIndex");
                }
            }
        }
        public abstract double ActualHeight { get; }
        public abstract double ActualWidth { get; }
        public abstract void OnCanvasRenderTransformChanged();
        #endregion

        #region Methods
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("Owner", Owner.Guid.ToString());
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("ZIndex", ZIndex.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            X = double.Parse(reader.GetAttribute("X") ?? string.Empty);
            Y = double.Parse(reader.GetAttribute("Y") ?? string.Empty);
            ZIndex = int.Parse(reader.GetAttribute("ZIndex") ?? string.Empty);
        }
        #endregion
    }
}