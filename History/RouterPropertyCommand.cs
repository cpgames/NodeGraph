using System;

namespace NodeGraph.History
{
    public class RouterPropertyCommand : NodeGraphCommand
    {
        #region Properties
        public Guid Guid { get; }
        public string PropertyName { get; }
        #endregion

        #region Constructors
        public RouterPropertyCommand(string name, Guid guid, string propertyName, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {
            Guid = guid;
            PropertyName = propertyName;
        }
        #endregion

        #region Methods
        public override void Undo()
        {
            var router = NodeGraphManager.FindRouter(Guid);
            if (router == null)
            {
                throw new InvalidOperationException("Router does not exist.");
            }
            var type = router.GetType();
            var propInfo = type.GetProperty(PropertyName);
            propInfo.SetValue(router, UndoParams);
        }

        public override void Redo()
        {
            var router = NodeGraphManager.FindRouter(Guid);
            if (null == router)
            {
                throw new InvalidOperationException("Router does not exist.");
            }
            var type = router.GetType();
            var propInfo = type.GetProperty(PropertyName);
            propInfo.SetValue(router, RedoParams);
        }
        #endregion
    }
}