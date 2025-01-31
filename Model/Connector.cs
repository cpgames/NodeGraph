using System;
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

        protected ConnectorViewModel _ViewModel;

        protected NodePort _StartPort;

        protected NodePort _EndPort;
        #endregion

        #region Properties
        public FlowChart FlowChart
        {
            get;
        }
        public ConnectorViewModel ViewModel
        {
            get => _ViewModel;
            set
            {
                if (value != _ViewModel)
                {
                    _ViewModel = value;
                    RaisePropertyChanged("ViewModel");
                }
            }
        }
        public NodePort StartPort
        {
            get => _StartPort;
            set
            {
                if (value != _StartPort)
                {
                    _StartPort = value;
                    RaisePropertyChanged("StartPort");
                }
            }
        }
        public NodePort EndPort
        {
            get => _EndPort;
            set
            {
                if (value != _EndPort)
                {
                    _EndPort = value;
                    RaisePropertyChanged("EndPort");
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
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException());
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
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            StartPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("StartPort") ?? throw new InvalidOperationException()));
            if (null == StartPort)
            {
                throw new InvalidOperationException("StartPort can not be null in Connector.ReadXml().");
            }

            EndPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("EndPort") ?? throw new InvalidOperationException()));
            if (null == EndPort)
            {
                throw new InvalidOperationException("EndPort can not be null in Connector.ReadXml().");
            }
        }
        #endregion // Overrides IXmlSerializable
    }
}