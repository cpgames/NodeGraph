using System;

namespace NodeGraph.History
{
    public class NodePropertyCommand : NodeGraphCommand
    {
        #region Properties
        public Guid Guid { get; }
        public string PropertyName { get; }
        #endregion

        #region Constructors
        public NodePropertyCommand(string name, Guid guid, string propertyName, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {
            Guid = guid;
            PropertyName = propertyName;
        }
        #endregion

        #region Methods
        public override void Undo()
        {
            var node = NodeGraphManager.FindNode(Guid);
            if (node == null)
            {
                throw new InvalidOperationException("Node does not exist.");
            }
            var type = node.GetType();
            var propInfo = type.GetProperty(PropertyName);
            propInfo.SetValue(node, UndoParams);
        }

        public override void Redo()
        {
            var node = NodeGraphManager.FindNode(Guid);
            if (node == null)
            {
                throw new InvalidOperationException("Node does not exist.");
            }
            var type = node.GetType();
            var propInfo = type.GetProperty(PropertyName);
            propInfo.SetValue(node, RedoParams);
        }
        #endregion
    }
}