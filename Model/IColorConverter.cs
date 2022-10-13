using System.Windows.Media;

namespace NodeGraph.Model
{
    public interface IColorConverter
    {
        #region Methods
        SolidColorBrush GetColor(object model);
        #endregion
    }
}