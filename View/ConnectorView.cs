using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NodeGraph.History;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class ConnectorView : ContentControl
    {
        #region Fields
        private const int MAX_ROUTERS = 3;
        public static readonly DependencyProperty CurveDataProperty =
            DependencyProperty.Register("CurveData", typeof(string), typeof(ConnectorView), new PropertyMetadata(""));

        private Curve _curve = new Curve();
        #endregion

        #region Properties
        public ConnectorViewModel ViewModel { get; private set; }

        public string CurveData
        {
            get => (string)GetValue(CurveDataProperty);
            set => SetValue(CurveDataProperty, value);
        }
        #endregion

        #region Constructors
        public ConnectorView()
        {
            LayoutUpdated += ConnectorView_LayoutUpdated;
            DataContextChanged += ConnectorView_DataContextChanged;
            Loaded += ConnectorView_Loaded;
        }
        #endregion

        #region Methods
        public void BuildCurveData(Point mousePos)
        {
            var connector = ViewModel.Model;
            var flowChart = connector.FlowChart;
            var flowChartView = flowChart.ViewModel.View;

            var startPort = connector.StartPort;
            var endPort = connector.EndPort;

            var start = null != startPort ? ViewUtil.GetRelativeCenterLocation(startPort.ViewModel.View.PartPort, flowChartView) : mousePos;
            var end = null != endPort ? ViewUtil.GetRelativeCenterLocation(endPort.ViewModel.View.PartPort, flowChartView) : mousePos;

            var points = new List<Point>();
            foreach (var router in connector.Routers)
            {
                var point = ViewUtil.GetRelativeCenterLocation(router.ViewModel.View, flowChartView);
                points.Add(point);
            }
            _curve = CurveBuilder.BuildCurve(start, end, points);
            CurveData = _curve.ToString();
        }

        private void ConnectorView_Loaded(object sender, RoutedEventArgs e)
        {
            SynchronizeProperties();
        }

        private void ConnectorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as ConnectorViewModel;
            if (null == ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in ConnectorView.");
            }
            ViewModel.view = this;
            ViewModel.PropertyChanged += ViewModelPropertyChanged;

            SynchronizeProperties();
        }

        private void ConnectorView_LayoutUpdated(object sender, EventArgs e)
        {
            var flowChart = ViewModel.Model.FlowChart;
            var flowChartView = flowChart.ViewModel.View;
            BuildCurveData(Mouse.GetPosition(flowChartView));
        }

        protected virtual void SynchronizeProperties()
        {
            if (null == ViewModel) { }
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SynchronizeProperties();
        }
        #endregion

        #region Mouse Events
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            var connector = ViewModel.Model;

            if (null != connector.StartPort)
            {
                var portView = connector.StartPort.ViewModel.View;
                portView.IsConnectorMouseOver = true;
            }

            if (null != connector.EndPort)
            {
                var portView = connector.EndPort.ViewModel.View;
                portView.IsConnectorMouseOver = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            var connector = ViewModel.Model;

            if (null != connector.StartPort)
            {
                var portView = connector.StartPort.ViewModel.View;
                portView.IsConnectorMouseOver = false;
            }

            if (null != connector.EndPort)
            {
                var portView = connector.EndPort.ViewModel.View;
                portView.IsConnectorMouseOver = false;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (MouseButton.Left == e.ChangedButton)
            {
                var connector = ViewModel.Model;
                var flowChart = connector.FlowChart;
                var flowChartView = flowChart.ViewModel.View;
                var vsMousePos = e.GetPosition(flowChartView);
                var nodePos = flowChartView.ZoomAndPan.MatrixInv.Transform(vsMousePos);

                if (connector.Routers.Count < MAX_ROUTERS)
                {
                    flowChart.History.BeginTransaction("Creating Router");
                    {
                        var index = _curve.GetSegmentIndex(nodePos);
                        var router = NodeGraphManager.CreateRouter(Guid.NewGuid(), connector, index);
                        router.X = nodePos.X;
                        router.Y = nodePos.Y;
                        flowChart.History.AddCommand(new CreateRouterCommand(
                            "Creating router", router.Guid, NodeGraphManager.SerializeRouter(router)));
                    }
                    flowChart.History.EndTransaction(false);
                }
            }

            e.Handled = true;
        }
        #endregion
    }
}