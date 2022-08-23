using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using NodeGraph.Model;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class RouterView : SelectableView
    {
        #region Properties
        public RouterViewModel ViewModel { get; private set; }

        public override FlowChart Owner => ViewModel.Model.Owner;
        public override ISelectable Selectable => ViewModel.Model;
        #endregion

        #region Constructors
        public RouterView()
        {
            DataContextChanged += RouterView_DataContextChanged;
            Loaded += RouterView_Loaded;
        }
        #endregion

        #region Methods
        private void RouterView_Loaded(object sender, RoutedEventArgs e)
        {
            SynchronizeProperties();
        }

        protected virtual void SynchronizeProperties()
        {
            if (null == ViewModel)
            {
                return;
            }
            IsSelected = ViewModel.IsSelected;
        }

        private void RouterView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as RouterViewModel;
            if (null == ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in NodeView.");
            }
            ViewModel.View = this;
            ViewModel.PropertyChanged += ViewModelPropertyChanged;

            SynchronizeProperties();
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SynchronizeProperties();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            NodeGraphManager.EndDragSelectable();
            NodeGraphManager.MouseLeftDownSelectable = null;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                NodeGraphManager.DestroyRouter(ViewModel.Model.Guid);
            }
        }
        #endregion
    }
}