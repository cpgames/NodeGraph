using System;
using System.Diagnostics;

namespace NodeGraph.History
{
    public class SelectablePropertyCommand : NodeGraphCommand
    {
        #region Properties
        public Guid Guid { get; }
        public string PropertyName { get; }
        #endregion

        #region Constructors
        public SelectablePropertyCommand(string name, Guid guid, string propertyName, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {
            Guid = guid;
            PropertyName = propertyName;
        }
        #endregion

        #region Methods
        public override void Undo()
        {
            var selectable = NodeGraphManager.FindSelectable(Guid);
            if (selectable == null)
            {
                throw new InvalidOperationException("Selectable does not exist.");
            }

            if (PropertyName == "IsSelected")
            {
                UpdateSelection((bool)UndoParams);
            }
            else
            {
                var type = selectable.GetType();
                var propInfo = type.GetProperty(PropertyName);
                propInfo.SetValue(selectable, UndoParams);
            }
        }

        public override void Redo()
        {
            var selectable = NodeGraphManager.FindSelectable(Guid);
            if (null == selectable)
            {
                throw new InvalidOperationException("Selectable does not exist.");
            }

            if (PropertyName == "IsSelected")
            {
                UpdateSelection((bool)RedoParams);
            }
            else
            {
                var type = selectable.GetType();
                var propInfo = type.GetProperty(PropertyName);
                propInfo.SetValue(selectable, RedoParams);
            }
        }

        private void UpdateSelection(bool isSelected)
        {
            var selectable = NodeGraphManager.FindSelectable(Guid);
            var selectionList = NodeGraphManager.GetSelectionList(selectable.Owner);
            selectable.IsSelected = isSelected;
            if (selectable.IsSelected)
            {
                Debug.WriteLine("True");
                if (!selectionList.Contains(Guid))
                {
                    selectionList.Add(Guid);
                }
            }
            else
            {
                Debug.WriteLine("False");
                if (selectionList.Contains(Guid))
                {
                    selectionList.Remove(Guid);
                }
            }
        }
        #endregion
    }
}