using System;
using System.Xml;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    public class Router : Selectable<RouterViewModel>
    {
        #region Fields
        private int _index;
        private Connector _connector;
        #endregion

        #region Properties
        public int Index
        {
            get => _index;
            set
            {
                if (value != _index)
                {
                    _index = value;
                    RaisePropertyChanged("Index");
                }
            }
        }

        public Connector Connector
        {
            get => _connector;
            set
            {
                if (value != _connector)
                {
                    _connector = value;
                    RaisePropertyChanged("Connector");
                }
            }
        }
        public override double ActualHeight => ViewModel.View.ActualHeight;
        public override double ActualWidth => ViewModel.View.ActualWidth;
        #endregion

        #region Constructors
        public Router(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCanvasRenderTransformChanged()
        {
            ViewModel.View.OnCanvasRenderTransformChanged();
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Connector", Connector.Guid.ToString());
            writer.WriteAttributeString("Index", Index.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Connector = NodeGraphManager.FindConnector(Guid.Parse(reader.GetAttribute("Connector")));
            Index = int.Parse(reader.GetAttribute("Index") ?? string.Empty);
        }
        #endregion
    }
}