using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NodeGraph.History;
using NodeGraph.Model;

namespace NodeGraph.View
{
    public abstract class SelectableView : ContentControl
    {
        #region Fields
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(SelectableView), new PropertyMetadata(false));
        public static readonly DependencyProperty SelectionThicknessProperty =
            DependencyProperty.Register("SelectionThickness", typeof(Thickness), typeof(SelectableView), new PropertyMetadata(new Thickness(2.0)));

        private Point _draggingStartPos;
        private Matrix _zoomAndPanStartMatrix;
        #endregion

        #region Properties
        public abstract FlowChart Owner { get; }
        public abstract ISelectable Selectable { get; }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public Thickness SelectionThickness
        {
            get => (Thickness)GetValue(SelectionThicknessProperty);
            set => SetValue(SelectionThicknessProperty, value);
        }

        #endregion

        #region Constructors
        #endregion

        #region Methods
        public virtual void OnCanvasRenderTransformChanged()
        {
            if (VisualParent == null)
            {
                return;
            }
            var matrix = (VisualParent as Canvas).RenderTransform.Value;
            var scale = matrix.M11;

            SelectionThickness = new Thickness(2.0 / scale);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            
            if (NodeGraphManager.IsSelecting)
            {
                var bChanged = false;
                Owner.History.BeginTransaction("Selecting");
                {
                    bChanged = NodeGraphManager.EndDragSelection(false);
                }
                Owner.History.EndTransaction(!bChanged);
            }
            if (!NodeGraphManager.AreSelectablesReallyDragged &&
                NodeGraphManager.MouseLeftDownSelectable == Selectable)
            {
                Owner.History.BeginTransaction("Selection");
                {
                    NodeGraphManager.TrySelection(Owner, Selectable,
                        Keyboard.IsKeyDown(Key.LeftCtrl),
                        Keyboard.IsKeyDown(Key.LeftShift),
                        Keyboard.IsKeyDown(Key.LeftAlt));
                }
                Owner.History.EndTransaction(false);
            }
            
            NodeGraphManager.EndDragSelectable();
            NodeGraphManager.MouseLeftDownSelectable = null;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var flowChartView = Owner.ViewModel.View;
            Keyboard.Focus(flowChartView);

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragSelectable();
            NodeGraphManager.EndDragSelection(false);
            NodeGraphManager.MouseLeftDownSelectable = Selectable;
            NodeGraphManager.BeginDragSelectable(Owner);

            _draggingStartPos = new Point(Selectable.X, Selectable.Y);
            Owner.History.BeginTransaction("Moving selectable");
            _zoomAndPanStartMatrix = flowChartView.ZoomAndPan.Matrix;

            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (NodeGraphManager.IsSelectableDragging)
            {
                var delta = new Point(Selectable.X - _draggingStartPos.X, Selectable.Y - _draggingStartPos.Y);

                if ((int)delta.X != 0 &&
                    (int)delta.Y != 0)
                {
                    var selectionList = NodeGraphManager.GetSelectionList(Owner);
                    foreach (var guid in selectionList)
                    {
                        var currentSelectable = NodeGraphManager.FindSelectable(guid);
                        Owner.History.AddCommand(new SelectablePropertyCommand(
                            "Selectable.X", currentSelectable.Guid, "X", currentSelectable.X - delta.X, currentSelectable.X));
                        Owner.History.AddCommand(new SelectablePropertyCommand(
                            "Selectable.Y", currentSelectable.Guid, "Y", currentSelectable.Y - delta.Y, currentSelectable.Y));
                    }

                    Owner.History.AddCommand(new ZoomAndPanCommand(
                        "ZoomAndPan", Owner, _zoomAndPanStartMatrix, Owner.ViewModel.View.ZoomAndPan.Matrix));

                    Owner.History.EndTransaction(false);
                }
                else
                {
                    Owner.History.EndTransaction(true);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (NodeGraphManager.IsSelectableDragging &&
                NodeGraphManager.MouseLeftDownSelectable == Selectable &&
                !IsSelected)
            {
                NodeGraphManager.TrySelection(Owner, Selectable, false, false, false);
            }
        }
        #endregion
    }
}