using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class ConnectorView : ContentControl
    {
        #region Fields
        public static readonly DependencyProperty CurveDataProperty =
            DependencyProperty.Register("CurveData", typeof(string), typeof(ConnectorView), new PropertyMetadata(""));
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
            var center = new Point((start.X + end.X) * 0.5, (start.Y + end.Y) * 0.5);

            if (start.X > end.X)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            var ratio = Math.Min(1.0, (center.X - start.X) / 100.0);
            var c0 = start;
            var c1 = end;
            c0.X += 100 * ratio;
            c1.X -= 100 * ratio;

            CurveData = string.Format("M{0},{1} C{0},{1} {2},{3} {4},{5} " +
                "M{4},{5} C{4},{5} {6},{7} {8},{9}",
                (int)start.X, (int)start.Y, // 0, 1
                (int)c0.X, (int)c0.Y, // 2, 3
                (int)center.X, (int)center.Y, // 4, 5
                (int)c1.X, (int)c1.Y, // 6, 7
                (int)end.X, (int)end.Y); // 8.9
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

                flowChart.History.BeginTransaction("Creating Router");
                {
                    NodeGraphManager.CreateRouter(Guid.NewGuid(), connector, nodePos.X, nodePos.Y);
                }
                flowChart.History.EndTransaction(false);
            }

            e.Handled = true;
        }
        #endregion
    }
}