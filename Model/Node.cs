using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Xml;
using NodeGraph.Commands;
using NodeGraph.History;
using NodeGraph.ViewModel;
// ReSharper disable UnusedVariable

namespace NodeGraph.Model
{
    [Node]
    public class Node : Selectable<NodeViewModel>
    {
        #region Fields
        protected string _Header;

        protected SolidColorBrush _HeaderBackgroundColor = Brushes.Black;

        protected SolidColorBrush _HeaderFontColor = Brushes.White;

        private bool _AllowEditingHeader = true;

        private bool _AllowCircularConnection  ;

        protected ObservableCollection<NodeFlowPort> _InputFlowPorts = new ObservableCollection<NodeFlowPort>();

        protected ObservableCollection<NodeFlowPort> _OutputFlowPorts = new ObservableCollection<NodeFlowPort>();

        protected ObservableCollection<NodePropertyPort> _InputPropertyPorts = new ObservableCollection<NodePropertyPort>();

        protected ObservableCollection<NodePropertyPort> _OutputPropertyPorts = new ObservableCollection<NodePropertyPort>();

        private ExecutionState _executionState;

        private RelayCommand _helpCommand;
        #endregion

        #region Properties
        public string Header
        {
            get => _Header;
            set
            {
                if (value != _Header)
                {
                    AddNodePropertyCommand("Header", _Header, value);
                    _Header = value;
                    RaisePropertyChanged("Header");
                }
            }
        }
        public SolidColorBrush HeaderBackgroundColor
        {
            get => _HeaderBackgroundColor;
            set
            {
                if (value != _HeaderBackgroundColor)
                {
                    AddNodePropertyCommand("HeaderBackgroundColor", _HeaderBackgroundColor, value);
                    _HeaderBackgroundColor = value;
                    RaisePropertyChanged("HeaderBackgroundColor");
                }
            }
        }
        public SolidColorBrush HeaderFontColor
        {
            get => _HeaderFontColor;
            set
            {
                if (value != _HeaderFontColor)
                {
                    AddNodePropertyCommand("HeaderFontColor", _HeaderFontColor, value);
                    _HeaderFontColor = value;
                    RaisePropertyChanged("HeaderFontColor");
                }
            }
        }
        public bool AllowEditingHeader
        {
            get => _AllowEditingHeader;
            set
            {
                if (value != _AllowEditingHeader)
                {
                    AddNodePropertyCommand("AllowEditingHeader", _AllowEditingHeader, value);
                    _AllowEditingHeader = value;
                    RaisePropertyChanged("AllowEditingHeader");
                }
            }
        }
        public bool AllowCircularConnection
        {
            get => _AllowCircularConnection;
            set
            {
                if (value != _AllowCircularConnection)
                {
                    AddNodePropertyCommand("AllowCircularConnection", _AllowCircularConnection, value);
                    _AllowCircularConnection = value;
                    RaisePropertyChanged("AllowCircularConnection");
                }
            }
        }

        public ObservableCollection<NodeFlowPort> InputFlowPorts
        {
            get => _InputFlowPorts;
            set
            {
                if (value != _InputFlowPorts)
                {
                    _InputFlowPorts = value;
                    RaisePropertyChanged("InputFlowPorts");
                }
            }
        }
        public ObservableCollection<NodeFlowPort> OutputFlowPorts
        {
            get => _OutputFlowPorts;
            set
            {
                if (value != _OutputFlowPorts)
                {
                    _OutputFlowPorts = value;
                    RaisePropertyChanged("OutputFlowPorts");
                }
            }
        }
        public ObservableCollection<NodePropertyPort> InputPropertyPorts
        {
            get => _InputPropertyPorts;
            set
            {
                if (value != _InputPropertyPorts)
                {
                    _InputPropertyPorts = value;
                    RaisePropertyChanged("InputPropertyPorts");
                }
            }
        }
        public ObservableCollection<NodePropertyPort> OutputPropertyPorts
        {
            get => _OutputPropertyPorts;
            set
            {
                if (value != _OutputPropertyPorts)
                {
                    _OutputPropertyPorts = value;
                    RaisePropertyChanged("OutputPropertyPorts");
                }
            }
        }
        public ExecutionState ExecutionState
        {
            get => _executionState;
            set
            {
                if (value != _executionState)
                {
                    _executionState = value;
                    RaisePropertyChanged("ExecutionState");
                }
            }
        }

        public RelayCommand HelpCommand
        {
            get => _helpCommand;
            set => _helpCommand = value;
        }

        public override double ActualHeight => ViewModel.View.ActualHeight;
        public override double ActualWidth => ViewModel.View.ActualWidth;
        #endregion

        #region Constructors
        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNode() method.
        /// </summary>
        public Node(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        ~Node() { }

        protected virtual void AddNodePropertyCommand(string propertyName, object prevValue, object newValue)
        {
            if (!IsInitialized)
            {
                return;
            }

            Owner.History.BeginTransaction("Setting Property");

            Owner.History.AddCommand(new NodePropertyCommand(
                propertyName, Guid, propertyName, prevValue, newValue));

            Owner.History.EndTransaction(false);
        }

        public override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);

            var port = NodeGraphManager.FindNodePropertyPort(this, propertyName);
            if (null != port)
            {
                var nodeType = GetType();

                var propertyInfo = nodeType.GetProperty(propertyName);
                if (null != propertyInfo)
                {
                    port.Value = propertyInfo.GetValue(this);
                }
                else
                {
                    var fieldInfo = nodeType.GetField(propertyName);
                    if (null != fieldInfo)
                    {
                        port.Value = fieldInfo.GetValue(this);
                    }
                }
            }
        }

        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnPreExecute()");
            }

            IsInitialized = true;

            RaisePropertyChanged("Model");
        }

        public ExecutionState Execute(Connector connector)
        {
            ExecutionState = ExecutionState.Executing;
            OnPreExecute(connector);
            OnExecute(connector);
            ExecutionState = ExecutionState == ExecutionState.Executing ? ExecutionState.Executed : ExecutionState;
            if (ExecutionState == ExecutionState.Executed)
            {
                OnPostExecute(connector);
            }
            return ExecutionState;
        }

        protected virtual void OnPreExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnPreExecute()");
            }
        }

        protected virtual void OnExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnExecute()");
            }
        }

        protected virtual void OnPostExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnPostExecute()");
            }
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnPostDestroy()");
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("Node.OnDeserialize()");
            }

            foreach (var port in InputFlowPorts)
            {
                port.OnDeserialize();
            }

            foreach (var port in OutputFlowPorts)
            {
                port.OnDeserialize();
            }

            foreach (var port in InputPropertyPorts)
            {
                port.OnDeserialize();
            }

            foreach (var port in OutputPropertyPorts)
            {
                port.OnDeserialize();
            }

            IsInitialized = true;

            RaisePropertyChanged("Model");
        }

        public override void OnCanvasRenderTransformChanged()
        {
            ViewModel.View.OnCanvasRenderTransformChanged();
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            //{ Begin Creation info : You need not deserialize this block in ReadXml().
            // These are automatically serialized in FlowChart.ReadXml().
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().FullName ?? throw new InvalidOperationException());
            //} End creation info.

            writer.WriteAttributeString("Header", Header);
            writer.WriteAttributeString("HeaderBackgroundColor", HeaderBackgroundColor.ToString());
            writer.WriteAttributeString("HeaderFontColor", HeaderFontColor.ToString());
            writer.WriteAttributeString("AllowEditingHeader", AllowEditingHeader.ToString());

            writer.WriteAttributeString("AllowCircularConnection", AllowCircularConnection.ToString());

            writer.WriteStartElement("InputFlowPorts");
            foreach (var port in InputFlowPorts)
            {
                writer.WriteStartElement("FlowPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("OutputFlowPorts");
            foreach (var port in OutputFlowPorts)
            {
                writer.WriteStartElement("FlowPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("InputPropertyPorts");
            foreach (var port in InputPropertyPorts)
            {
                writer.WriteStartElement("PropertyPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("OutputPropertyPorts");
            foreach (var port in OutputPropertyPorts)
            {
                writer.WriteStartElement("PropertyPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            IsDeserializedFromXml = true;
            Header = reader.GetAttribute("Header");
            HeaderBackgroundColor = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(reader.GetAttribute("HeaderBackgroundColor")));
            HeaderFontColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                reader.GetAttribute("HeaderFontColor")));
            AllowEditingHeader = bool.Parse(reader.GetAttribute("AllowEditingHeader") ?? throw new InvalidOperationException());

            AllowCircularConnection = bool.Parse(reader.GetAttribute("AllowCircularConnection") ?? throw new InvalidOperationException());

            var isInputFlowPortsEnd = false;
            var isOutputFlowPortsEnd = false;
            var isInputPropertyPortsEnd = false;
            var isOutputPropertyPortsEnd = false;
            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    if ("PropertyPort" == reader.Name ||
                        "FlowPort" == reader.Name)
                    {
                        var prevReaderName = reader.Name;

                        var guid = Guid.Parse(reader.GetAttribute("Guid"));
                        var type = Type.GetType(reader.GetAttribute("Type"));
                        var vmType = Type.GetType(reader.GetAttribute("ViewModelType"));
                        var isInput = bool.Parse(reader.GetAttribute("IsInput"));

                        var ownerGuidString = reader.GetAttribute("Owner");
                        var node = NodeGraphManager.FindNode(Guid.Parse(ownerGuidString));

                        if ("PropertyPort" == prevReaderName)
                        {
                            var name = reader.GetAttribute("Name");
                            var valueType = Type.GetType(reader.GetAttribute("ValueType"));
                            var hasEditor = bool.Parse(reader.GetAttribute("HasEditor"));
                            var serializeValue = bool.Parse(reader.GetAttribute("SerializeValue") ?? "false");

                            var port = NodeGraphManager.CreateNodePropertyPort(
                                true, guid, node, isInput, valueType, null, name, hasEditor, vmType, serializeValue: serializeValue);
                            port.ReadXml(reader);
                        }
                        else
                        {
                            var port = NodeGraphManager.CreateNodeFlowPort(
                                true, guid, node, isInput, vmType);
                            port.ReadXml(reader);
                        }
                    }
                }

                if (reader.IsEmptyElement || XmlNodeType.EndElement == reader.NodeType)
                {
                    if ("InputFlowPorts" == reader.Name)
                    {
                        isInputFlowPortsEnd = true;
                    }
                    else if ("OutputFlowPorts" == reader.Name)
                    {
                        isOutputFlowPortsEnd = true;
                    }
                    else if ("InputPropertyPorts" == reader.Name)
                    {
                        isInputPropertyPortsEnd = true;
                    }
                    else if ("OutputPropertyPorts" == reader.Name)
                    {
                        isOutputPropertyPortsEnd = true;
                    }
                }

                if (isInputFlowPortsEnd && isOutputFlowPortsEnd &&
                    isInputPropertyPortsEnd && isOutputPropertyPortsEnd)
                {
                    break;
                }
            }
            IsDeserializedFromXml = false;
        }
        #endregion
    }
}