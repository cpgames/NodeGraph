using System;
using System.Windows;
using System.Windows.Controls;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class NodeFlowPortViewsContainer : ItemsControl
    {
        #region Fields
        public static readonly DependencyProperty IsInputProperty =
            DependencyProperty.Register("IsInput", typeof(bool), typeof(NodeFlowPortViewsContainer), new PropertyMetadata(false));

        private Type _ViewType  ;
        #endregion

        #region Properties
        public bool IsInput
        {
            get => (bool)GetValue(IsInputProperty);
            set => SetValue(IsInputProperty, value);
        }
        #endregion

        #region Overrides ItemsControl
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            var attrs = item.GetType().GetCustomAttributes(typeof(NodeFlowPortViewModelAttribute), false) as NodeFlowPortViewModelAttribute[];

            if (0 == attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist for NodeFlowPortViewModel class.");
            }
            if (1 < attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist only one.");
            }

            _ViewType = attrs[0].ViewType;

            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var attrs = item.GetType().GetCustomAttributes(typeof(NodeFlowPortViewModelAttribute), false) as NodeFlowPortViewModelAttribute[];
            if (1 != attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist for NodeFlowPortViewModel class");
            }

            var fe = element as FrameworkElement;

            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/NodeGraph;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
            };

            var style = resourceDictionary[attrs[0].ViewStyleName] as Style;
            if (null == style)
            {
                style = Application.Current.TryFindResource(attrs[0].ViewStyleName) as Style;
            }
            fe.Style = style;

            if (null == fe.Style)
            {
                throw new Exception($"{attrs[0].ViewStyleName} does not exist");
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return Activator.CreateInstance(_ViewType, new object[] { IsInput }) as DependencyObject ?? throw new InvalidOperationException();
        }
        #endregion // Overrides ItemsControl
    }
}