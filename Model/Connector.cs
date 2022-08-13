using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    [Connector]
    public class Connector : ModelBase
    {
        #region Fields
        protected ConnectorViewModel _viewModel;
        protected NodePort _startPort;
        protected NodePort _endPort;
        protected ObservableCollection<Router> _routers = new ObservableCollection<Router>();
        #endregion

        #region Properties
        public FlowChart FlowChart
        {
            get;
        }

        public ConnectorViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (value != _viewModel)
                {
                    _viewModel = value;
                    RaisePropertyChanged("ViewModel");
                }
            }
        }
        
        public NodePort StartPort
        {
            get => _startPort;
            set
            {
                if (value != _startPort)
                {
                    _startPort = value;
                    RaisePropertyChanged("StartPort");
                }
            }
        }
        
        public NodePort EndPort
        {
            get => _endPort;
            set
            {
                if (value != _endPort)
                {
                    _endPort = value;
                    RaisePropertyChanged("EndPort");
                }
            }
        }
        
        public ObservableCollection<Router> Routers
        {
            get => _routers;
            set
            {
                if (value != _routers)
                {
                    RaisePropertyChanged("Routers");
                }
            }
        }
        #endregion

        #region Methods
        public bool IsConnectedPort(NodePort port)
        {
            return StartPort == port || EndPort == port;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateConnector() method.
        /// </summary>
        protected internal Connector() { }

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateConnector() method.
        /// </summary>
        public Connector(Guid guid, FlowChart flowChart) : base(guid)
        {
            FlowChart = flowChart;
        }
        #endregion // Constructor

        #region Callbacks
        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnCreate()");
            }
            IsInitialized = true;

            RaisePropertyChanged("Model");
        }

        public ExecutionState Execute()
        {
            return null != EndPort ? EndPort.Owner.Execute(this) : ExecutionState.Failed;
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnPostDestroy()");
            }
        }

        public virtual void OnConnect(NodePort port)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnConnect()");
            }
        }

        public virtual void OnDisconnect(NodePort port)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnDisconnect()");
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Connector.OnDeserialize()");
            }

            NodeGraphManager.ConnectTo(StartPort, this);
            NodeGraphManager.ConnectTo(EndPort, this);
            IsInitialized = true;

            RaisePropertyChanged("Model");
        }
        #endregion // Callbacks

        #region Overrides IXmlSerializable
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            //{ Begin Creation info : You need not deserialize this block in ReadXml().
            // These are automatically serialized in Node.ReadXml().
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Owner", FlowChart.Guid.ToString());
            //} End Creation Info.

            if (null != StartPort)
            {
                writer.WriteAttributeString("StartPort", StartPort.Guid.ToString());
            }
            if (null != EndPort)
            {
                writer.WriteAttributeString("EndPort", EndPort.Guid.ToString());
            }

            writer.WriteStartElement("Routers");
            foreach (var router in Routers)
            {
                writer.WriteStartElement("Router");
                router.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            StartPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("StartPort")));
            if (null == StartPort)
            {
                throw new InvalidOperationException("StartPort can not be null in Connector.ReadXml().");
            }

            EndPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("EndPort")));
            if (null == EndPort)
            {
                throw new InvalidOperationException("EndPort can not be null in Connector.ReadXml().");
            }

            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    if ("Router" == reader.Name)
                    {
                        var guid = Guid.Parse(reader.GetAttribute("Guid"));
                        var ownerGuidString = reader.GetAttribute("Owner");
                        var connector = NodeGraphManager.FindConnector(Guid.Parse(ownerGuidString));
                        var router = NodeGraphManager.CreateRouter(guid, connector, 0, 0);
                        router.ReadXml(reader);
                    }
                }
                if (reader.IsEmptyElement || reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }
        #endregion // Overrides IXmlSerializable
    }
}