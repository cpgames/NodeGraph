using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Xml;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    public class NodePort : ModelBase
    {
        #region Fields
        public readonly bool IsInput;

        public readonly Node Owner;

        private string _Name;

        private bool _AllowMultipleInput;

        private bool _AllowMultipleOutput;

        private string _DisplayName;

        private ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();

        private bool _IsPortEnabled = true;

        private bool _IsEnabled = true;

        private SolidColorBrush _TextForegroundColor = Brushes.White;
        #endregion

        #region Properties
        public NodePortViewModel ViewModel { get; set; }
        public string Name
        {
            get => _Name;
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }
        public bool AllowMultipleInput
        {
            get => _AllowMultipleInput;
            set
            {
                if (value != _AllowMultipleInput)
                {
                    _AllowMultipleInput = value;
                    RaisePropertyChanged("AllowMultipleInput");
                }
            }
        }
        public bool AllowMultipleOutput
        {
            get => _AllowMultipleOutput;
            set
            {
                if (value != _AllowMultipleOutput)
                {
                    _AllowMultipleOutput = value;
                    RaisePropertyChanged("AllowMultipleOutput");
                }
            }
        }
        public string DisplayName
        {
            get => _DisplayName;
            set
            {
                if (value != _DisplayName)
                {
                    _DisplayName = value;
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
        public ObservableCollection<Connector> Connectors
        {
            get => _Connectors;
            set
            {
                if (value != _Connectors)
                {
                    _Connectors = value;
                    RaisePropertyChanged("Connectors");
                }
            }
        }
        public bool IsPortEnabled
        {
            get => _IsPortEnabled;
            set
            {
                if (value != _IsPortEnabled)
                {
                    _IsPortEnabled = value;
                    RaisePropertyChanged("IsPortEnabled");
                }
            }
        }
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }

        public SolidColorBrush TextForegroundColor
        {
            get => _TextForegroundColor;
            set
            {
                if (value != _TextForegroundColor)
                {
                    _TextForegroundColor = value;
                    RaisePropertyChanged("TextForegroundColor");
                }
            }
        }
        #endregion

        #region Constructors
        #region Constructor
        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() or GraphManager.CreateNodePropertyPort()
        /// method.
        /// </summary>
        protected NodePort(Guid guid, Node node, bool isInput) : base(guid)
        {
            Owner = node;
            IsInput = isInput;
        }
        #endregion // Constructor
        #endregion

        #region Methods
        #region Destructor
        ~NodePort() { }
        #endregion // Destructor

        public virtual bool IsConnectable(NodePort otherPort, out string error)
        {
            error = "";
            return true;
        }
        #endregion

        #region Callbacks
        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnCreate()");
            }

            IsInitialized = true;

            RaisePropertyChanged("Model");
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnPostDestroy()");
            }
        }

        public virtual void OnConnect(Connector connector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnConnect()");
            }
        }

        public virtual void OnDisconnect(Connector connector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnDisconnect()");
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                Debug.WriteLine("NodePort.OnDeserialize()");
            }

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
            writer.WriteAttributeString("Owner", Owner.Guid.ToString());
            writer.WriteAttributeString("IsInput", IsInput.ToString());
            //} End Creation Info.

            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("DisplayName", DisplayName);
            writer.WriteAttributeString("AllowMultipleInput", AllowMultipleInput.ToString());
            writer.WriteAttributeString("AllowMultipleOutput", AllowMultipleOutput.ToString());
            writer.WriteAttributeString("IsPortEnabled", IsPortEnabled.ToString());
            writer.WriteAttributeString("IsEnabled", IsEnabled.ToString());
            writer.WriteAttributeString("TextForegroundColor", TextForegroundColor.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            Name = reader.GetAttribute("Name");
            DisplayName = reader.GetAttribute("DisplayName");
            AllowMultipleInput = bool.Parse(reader.GetAttribute("AllowMultipleInput") ?? throw new InvalidOperationException());
            AllowMultipleOutput = bool.Parse(reader.GetAttribute("AllowMultipleOutput") ?? throw new InvalidOperationException());
            IsPortEnabled = bool.Parse(reader.GetAttribute("IsPortEnabled") ?? throw new InvalidOperationException());
            IsEnabled = bool.Parse(reader.GetAttribute("IsEnabled") ?? throw new InvalidOperationException());
            TextForegroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString(reader.GetAttribute("TextForegroundColor") ?? "White");
        }
        #endregion // Overrides IXmlSerializable
    }
}