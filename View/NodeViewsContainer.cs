using System;
using System.Windows;
using System.Windows.Controls;
using NodeGraph.ViewModel;

namespace NodeGraph.View
{
    public class NodeViewsContainer : ItemsControl
    {
        #region Fields
        private Type _ViewType  ;
        #endregion

        #region Overrides ItemsControl
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            var attrs = item.GetType().GetCustomAttributes(typeof(NodeViewModelAttribute), false) as NodeViewModelAttribute[];

            if (0 == attrs.Length)
            {
                throw new Exception("A NodeViewModelAttribute must exist for NodeViewModel class.");
            }
            if (1 < attrs.Length)
            {
                throw new Exception("A NodeViewModelAttribute must exist only one.");
            }

            _ViewType = attrs[0].ViewType;

            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var attrs = item.GetType().GetCustomAttributes(typeof(NodeViewModelAttribute), false) as NodeViewModelAttribute[];
            if (0 == attrs.Length)
            {
                throw new Exception("A NodeViewModelAttribute must exist for NodeViewModel class.");
            }
            if (1 < attrs.Length)
            {
                throw new Exception("A NodeViewModelAttribute must exist only one.");
            }

            var fe = element as FrameworkElement;

            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/NodeGraph;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
            };

            var styleName = attrs[0].ViewStyleName;
            var res = resourceDictionary[styleName];
            var style = res as Style;
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
            return Activator.CreateInstance(_ViewType) as DependencyObject ?? throw new InvalidOperationException();
        }
        #endregion // Overrides ItemsControl
    }
}