using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NodeGraph.History;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class RouterView : ContentControl
    {
        #region Fields
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(RouterView), new PropertyMetadata(false));
        public static readonly DependencyProperty RelativeXProperty =
            DependencyProperty.Register("RelativeX", typeof(double), typeof(RouterView), new PropertyMetadata(0.0));
        public static readonly DependencyProperty RelativeYProperty =
            DependencyProperty.Register("RelativeY", typeof(double), typeof(RouterView), new PropertyMetadata(0.0));

        private Point _draggingStartPos;
        private Matrix _zoomAndPanStartMatrix;
        #endregion

        #region Properties
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public double RelativeX
        {
            get => (double)GetValue(RelativeXProperty);
            set => SetValue(RelativeXProperty, value);
        }
        public double RelativeY
        {
            get => (double)GetValue(RelativeYProperty);
            set => SetValue(RelativeYProperty, value);
        }
        public RouterViewModel ViewModel { get; private set; }
        #endregion

        #region Constructors
        public RouterView()
        {
            DataContextChanged += RouterView_DataContextChanged;
            LayoutUpdated += RouterView_LayoutUpdated;
        }
        #endregion

        #region Methods
        private void RouterView_LayoutUpdated(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var router = ViewModel.Model;
            var flowChartView = router.owner.FlowChart.ViewModel.View;
            var connectorView = router.owner.ViewModel.view;
            var p = new Point(router.X, router.Y);
            var rp = flowChartView.ZoomAndPan.Matrix.Transform(p);
            RelativeX = rp.X;
            RelativeY = rp.Y;
        }

        private void RouterView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as RouterViewModel;
            if (null == ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in NodeView.");
            }
            ViewModel.view = this;

        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            NodeGraphManager.EndDragNode();
            NodeGraphManager.MouseLeftDownNode = null;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var flowChart = ViewModel.Model.owner.FlowChart;
            var flowChartView = flowChart.ViewModel.View;
            Keyboard.Focus(flowChartView);

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragNode();
            NodeGraphManager.EndDragSelection(false);
            NodeGraphManager.MouseLeftDownRouter = ViewModel.Model;
            NodeGraphManager.BeginDragNode(flowChart);

            var router = ViewModel.Model;
            _draggingStartPos = new Point(router.X, router.Y);
            flowChart.History.BeginTransaction("Moving router");
            _zoomAndPanStartMatrix = flowChartView.ZoomAndPan.Matrix;

            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (NodeGraphManager.IsNodeDragging)
            {
                var flowChart = ViewModel.Model.owner.FlowChart;
                var router = ViewModel.Model;
                var delta = new Point(router.X - _draggingStartPos.X, router.Y - _draggingStartPos.Y);

                if ((int)delta.X != 0 &&
                    (int)delta.Y != 0)
                {
                    var selectionList = NodeGraphManager.GetSelectionList(router.owner.FlowChart);
                    foreach (var guid in selectionList)
                    {
                        var currentRouter = NodeGraphManager.FindRouter(guid);

                        flowChart.History.AddCommand(new NodePropertyCommand(
                            "Router.X", currentRouter.Guid, "X", currentRouter.X - delta.X, currentRouter.X));
                        flowChart.History.AddCommand(new NodePropertyCommand(
                            "Router.Y", currentRouter.Guid, "Y", currentRouter.Y - delta.Y, currentRouter.Y));
                    }

                    flowChart.History.AddCommand(new ZoomAndPanCommand(
                        "ZoomAndPan", flowChart, _zoomAndPanStartMatrix, flowChart.ViewModel.View.ZoomAndPan.Matrix));

                    flowChart.History.EndTransaction(false);
                }
                else
                {
                    flowChart.History.EndTransaction(true);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (NodeGraphManager.IsNodeDragging &&
                NodeGraphManager.MouseLeftDownRouter == ViewModel.Model &&
                !IsSelected)
            {
                var router = ViewModel.Model;
                var flowChart = router.owner.FlowChart;
                NodeGraphManager.TrySelection(flowChart, router, false, false, false);
            }
        }
        #endregion
    }
}