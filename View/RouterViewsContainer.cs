using System;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.View
{
    public class RouterViewsContainer : ItemsControl
    {
        #region Methods
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var fe = element as FrameworkElement;
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/NodeGraph;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
            };
            var style = resourceDictionary["RouterViewStyle"] as Style ?? 
                Application.Current.TryFindResource("RouterViewStyle") as Style;
            fe.Style = style;

            if (fe.Style == null)
            {
                throw new Exception("RouterViewStyle does not exist");
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RouterView();
        }
        #endregion
    }
}