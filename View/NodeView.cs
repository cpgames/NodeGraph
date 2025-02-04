﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NodeGraph.Model;
using NodeGraph.ViewModel;
using PropertyTools.Wpf;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_Header", Type = typeof(EditableTextBlock))]
    public class NodeView : SelectableView
    {
        #region Fields
        public static readonly DependencyProperty HasConnectionProperty =
            DependencyProperty.Register("HasConnection", typeof(bool), typeof(NodeView), new PropertyMetadata(false));
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(NodeView), new PropertyMetadata(8.0));
        public static readonly DependencyProperty ExecutionStateImageProperty =
            DependencyProperty.Register("ExecutionStateImage", typeof(BitmapImage), typeof(NodeView),
                new PropertyMetadata(null));

        private EditableTextBlock _Part_Header;
        private readonly DispatcherTimer _ClickTimer = new DispatcherTimer();
        private int _ClickCount  ;

        private readonly Dictionary<ExecutionState, BitmapImage> _executionResultImages = new Dictionary<ExecutionState, BitmapImage>();
        #endregion

        #region Properties
        public NodeViewModel ViewModel { get; private set; }
        public override FlowChart Owner => ViewModel.Model.Owner;
        public override ISelectable Selectable => ViewModel.Model;

        public bool HasConnection
        {
            get => (bool)GetValue(HasConnectionProperty);
            set => SetValue(HasConnectionProperty, value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public BitmapImage ExecutionStateImage
        {
            get => (BitmapImage)GetValue(ExecutionStateImageProperty);
            set => SetValue(ExecutionStateImageProperty, value);
        }
        #endregion

        #region Constructors
        public NodeView()
        {
            DataContextChanged += NodeView_DataContextChanged;
            Loaded += NodeView_Loaded;
            Unloaded += NodeView_Unloaded;

            _executionResultImages[ExecutionState.None] = new BitmapImage();

            _executionResultImages[ExecutionState.Executing] = LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Executing.png"));
            _executionResultImages[ExecutionState.Executed] = LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Executed.png"));
            _executionResultImages[ExecutionState.Failed] = LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Failed.png"));
            _executionResultImages[ExecutionState.Skipped] = LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Skipped.png"));
        }
        #endregion

        #region Methods
        private BitmapImage LoadBitmapImage(Uri uri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        #region Template Events
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _Part_Header = Template.FindName("PART_Header", this) as EditableTextBlock;
            if (null != _Part_Header)
            {
                _Part_Header.MouseDown += _Part_Header_MouseDown;
            }
            var node = Template.FindName("Node", this) as Grid;
            if (null != node)
            {
                node.MouseEnter += Node_MouseEnter;
                node.MouseLeave += Node_MouseLeave;
            }
        }
        #endregion // Template Events

        #region RenderTrasnform
        public override void OnCanvasRenderTransformChanged()
        {
            base.OnCanvasRenderTransformChanged();
            if (VisualParent == null)
            {
                return;
            }
            var matrix = (VisualParent as Canvas).RenderTransform.Value;
            var scale = matrix.M11;
            
            CornerRadius = 8.0 / scale;
        }
        #endregion // RenderTransform
        #endregion

        #region Header Events
        private void _Part_Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(_Part_Header);

            if (0 == _ClickCount)
            {
                _ClickTimer.Start();
                _ClickCount++;
            }
            else if (1 == _ClickCount)
            {
                _Part_Header.IsEditing = true;
                Keyboard.Focus(_Part_Header);
                _ClickCount = 0;
                _ClickTimer.Stop();

                e.Handled = true;
            }
        }

        private void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            var helpButton = Template.FindName("HelpButton", this) as Button;
            helpButton.Visibility = Visibility.Visible;
        }

        private void Node_MouseLeave(object sender, MouseEventArgs e)
        {
            var helpButton = Template.FindName("HelpButton", this) as Button;
            helpButton.Visibility = Visibility.Hidden;
        }
        #endregion // Header Events

        #region Events
        private void NodeView_Loaded(object sender, RoutedEventArgs e)
        {
            SynchronizeProperties();
            OnCanvasRenderTransformChanged();

            _ClickTimer.Interval = TimeSpan.FromMilliseconds(300);
            _ClickTimer.Tick += _ClickTimer_Tick;
        }

        private void _ClickTimer_Tick(object sender, EventArgs e)
        {
            _ClickCount = 0;
            _ClickTimer.Stop();
        }

        private void NodeView_Unloaded(object sender, RoutedEventArgs e) { }

        private void NodeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as NodeViewModel;
            if (null == ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in NodeView.");
            }
            ViewModel.View = this;
            ViewModel.PropertyChanged += ViewModelPropertyChanged;

            SynchronizeProperties();
        }

        protected virtual void SynchronizeProperties()
        {
            if (null == ViewModel)
            {
                return;
            }

            IsSelected = ViewModel.IsSelected;
            HasConnection = 0 < ViewModel.InputFlowPortViewModels.Count ||
                0 < ViewModel.OutputFlowPortViewModels.Count ||
                0 < ViewModel.InputPropertyPortViewModels.Count ||
                0 < ViewModel.OutputPropertyPortViewModels.Count;

            ExecutionStateImage = _executionResultImages[ViewModel.Model.ExecutionState];
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SynchronizeProperties();
        }
        #endregion // Events

        #region Mouse Events
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (NodeGraphManager.IsConnecting)
            {
                bool bConnected;
                Owner.History.BeginTransaction("Creating Connection");
                {
                    bConnected = NodeGraphManager.EndConnection();
                }
                Owner.History.EndTransaction(!bConnected);
            }
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/cpgames/RefactorGraph/wiki");
        }
        #endregion // Mouse Events
    }
}