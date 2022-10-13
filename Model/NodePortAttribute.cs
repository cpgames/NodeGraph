using System;

namespace NodeGraph.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class NodePortAttribute : Attribute
    {
        #region Fields
        public string Name = string.Empty;
        public string DisplayName = string.Empty;
        public bool IsInput  ;
        public bool AllowMultipleInput = false;
        public bool AllowMultipleOutput = false;
        public bool IsPortEnabled = true;
        public bool IsEnabled = true;
        public int Index = -1;
        public Type FontColorConverterType = null;
        #endregion

        #region Constructors
        public NodePortAttribute(string displayName, bool isInput)
        {
            DisplayName = displayName;
            IsInput = isInput;
        }
        #endregion
    }
}