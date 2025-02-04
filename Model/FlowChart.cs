﻿using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace NodeGraph.Model
{
	[FlowChart()]
	public class FlowChart : ModelBase
	{
		#region Properties
        protected string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

		protected bool _IsReference;
		public bool IsReference
		{
			get { return _IsReference; }
			set
			{
				if (value != _IsReference)
				{
					_IsReference = value;
					RaisePropertyChanged("IsReference");
				}
			}
		}

		protected FlowChartViewModel _ViewModel;
		public FlowChartViewModel ViewModel
		{
			get { return _ViewModel; }
			set
			{
				if( value != _ViewModel )
				{
					_ViewModel = value;
					RaisePropertyChanged( "ViewModel" );
				}
			}
		}

		protected ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
		public ObservableCollection<Node> Nodes
		{
			get { return _Nodes; }
			set
			{
				if( value != _Nodes )
				{
					_Nodes = value;
					RaisePropertyChanged( "Nodes" );
				}
			}
		}

		protected ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
		public ObservableCollection<Connector> Connectors
		{
			get { return _Connectors; }
			set
			{
				if( value != _Connectors )
				{
					_Connectors = value;
					RaisePropertyChanged( "Connectors" );
				}
			}
		}

        protected ObservableCollection<Router> _Routers = new ObservableCollection<Router>();
        public ObservableCollection<Router> Routers
        {
            get { return _Routers; }
            set
            {
                if (value != _Routers)
                {
                    _Routers = value;
                    RaisePropertyChanged("Routers");
                }
            }
        }

        public History.NodeGraphHistory History { get; private set; }

		#endregion // Properties

		#region Constructor

		/// <summary>
		/// Never call this constructor directly. Use GraphManager.CreateFlowChart() method.
		/// </summary>
		public FlowChart( Guid guid ) : base( guid )
		{
			History = new History.NodeGraphHistory( this, 100 );
		}

		#endregion // Constructor

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnCreate()" );
			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		public virtual void OnPreExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreExecute()" );
		}

		public virtual void OnExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnExecute()" );
		}

		public virtual void OnPostExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostExecute()" );
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnPostDestroy()" );
		}

		public virtual void OnDeserialize()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "FlowChart.OnDeserialize()" );

			foreach( var node in Nodes )
			{
				node.OnDeserialize();
			}

			foreach( var connector in Connectors )
			{
				connector.OnDeserialize();
			}

			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		#endregion // Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

            writer.WriteAttributeString("Name", Name);
			writer.WriteAttributeString("IsReference", IsReference.ToString());

			writer.WriteStartElement( "Nodes" );
			foreach( var node in Nodes )
			{
				writer.WriteStartElement( "Node" );
				node.WriteXml( writer );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement( "Connectors" );
			foreach( var connector in Connectors )
			{
				writer.WriteStartElement( "Connector" );
				connector.WriteXml( writer );
				writer.WriteEndElement();
			}
            writer.WriteEndElement();

            writer.WriteStartElement("Routers");
            foreach (var router in Routers)
            {
                writer.WriteStartElement("Router");
                router.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

		public override void ReadXml(XmlReader reader)
		{
			base.ReadXml(reader);

			Name = reader.GetAttribute("Name");
			if (bool.TryParse(reader.GetAttribute("IsReference"), out var isReference))
			{
				IsReference = isReference;
			}

			bool isNodesEnd = false;
			bool isConnectorsEnd = false;
			bool isRoutersEnd = false;

			while (reader.Read())
			{
				if (XmlNodeType.Element == reader.NodeType)
				{
					if (("Node" == reader.Name) ||
						("Connector" == reader.Name) ||
						("Router" == reader.Name))
					{
						string prevReaderName = reader.Name;

						Guid guid = Guid.Parse(reader.GetAttribute("Guid"));
						Type type = Type.GetType(reader.GetAttribute("Type"));
						FlowChart flowChart = NodeGraphManager.FindFlowChart(
							Guid.Parse(reader.GetAttribute("Owner")));

						if ("Node" == prevReaderName)
						{
							Type vmType = Type.GetType(reader.GetAttribute("ViewModelType"));

							Node node = NodeGraphManager.CreateNode(true, guid, flowChart, type, 0.0, 0.0, 0, vmType);
							try
							{
								node.ReadXml(reader);
							}
							catch (Exception e)
							{
								NodeGraphManager.DestroyNode(guid);
							}
						}
						else if ("Connector" == prevReaderName)
						{
							Connector connector = NodeGraphManager.CreateConnector(false, guid, flowChart, type);
							connector.ReadXml(reader);
						}
						else if ("Router" == prevReaderName)
						{
							Router router = NodeGraphManager.CreateRouter(guid, flowChart);
							router.ReadXml(reader);
						}

					}
				}

				if (reader.IsEmptyElement || XmlNodeType.EndElement == reader.NodeType)
				{
					if ("Nodes" == reader.Name)
					{
						isNodesEnd = true;
					}
					else if ("Connectors" == reader.Name)
					{
						isConnectorsEnd = true;
					}
					else if ("Routers" == reader.Name)
					{
						isRoutersEnd = true;
					}
				}

				if (isNodesEnd && isConnectorsEnd && isRoutersEnd)
					break;
			}
		}

		#endregion // Overrides IXmlSerializable
	}
}
