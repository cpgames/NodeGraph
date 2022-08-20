using System;
using System.Windows;
using System.Xml;
using NodeGraph.View;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    public class Router : ModelBase
    {
        #region Fields
        protected double _x;
        protected double _y;
        protected int _index;
        public readonly Connector Owner;
        protected RouterViewModel _viewModel;
        #endregion

        #region Properties
        public RouterViewModel ViewModel
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
        public double X
        {
            get => _x;
            set
            {
                if (value != _x)
                {
                    _x = value;
                    RaisePropertyChanged("X");
                    RaisePropertyChanged("RelativeX");
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
                    RaisePropertyChanged("RelativeY");
                }
            }
        }

        public double RelativeX
        {
            get
            {
                var p = new Point(X, Y);
                var flowChartView = Owner.FlowChart.ViewModel.View;
                p = flowChartView.ZoomAndPan.Matrix.Transform(p);
                return p.X;
            }
        }

        public double RelativeY
        {
            get
            {
                var p = new Point(X, Y);
                var flowChartView = Owner.FlowChart.ViewModel.View;
                p = flowChartView.ZoomAndPan.Matrix.Transform(p);
                return p.Y;
            }
        }

        protected int Index
        {
            get => _index;
            set
            {
                if (value != _index)
                {
                    _index = value;
                    RaisePropertyChanged("Index");
                }
            }
        }
        #endregion

        #region Constructors
        public Router(Guid guid, Connector connector, int index) : base(guid)
        {
            Owner = connector;
            Index = index;
        }
        #endregion

        #region Methods
        public void ConnectView(RouterView view)
        {
            var flowChartView = Owner.FlowChart.ViewModel.View;
            flowChartView.ZoomAndPan.UpdateTransform += ZoomAndPan_UpdateTransform;
        }

        private void ZoomAndPan_UpdateTransform()
        {
            RaisePropertyChanged("RelativeX");
            RaisePropertyChanged("RelativeY");
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Owner", Owner.Guid.ToString());
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("Index", Index.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            X = double.Parse(reader.GetAttribute("X"));
            Y = double.Parse(reader.GetAttribute("Y"));
            Index = int.Parse(reader.GetAttribute("Index") ?? "0");
        }
        #endregion
    }
}