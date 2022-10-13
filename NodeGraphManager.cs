﻿using NodeGraph.Model;
using NodeGraph.View;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml;

namespace NodeGraph
{
	public enum SelectionMode
	{
		Overlap,
		Include,
	}

	public class NodeGraphManager
	{
		#region Fields

		public static readonly Dictionary<Guid, FlowChart> FlowCharts = new Dictionary<Guid, FlowChart>();
		public static readonly Dictionary<Guid, Node> Nodes = new Dictionary<Guid, Node>();
        public static readonly Dictionary<Guid, Router> Routers = new Dictionary<Guid, Router>();
        public static readonly Dictionary<Guid, ISelectable> Selectables = new Dictionary<Guid, ISelectable>();
		public static readonly Dictionary<Guid, Connector> Connectors = new Dictionary<Guid, Connector>();
		public static readonly Dictionary<Guid, NodeFlowPort> NodeFlowPorts = new Dictionary<Guid, NodeFlowPort>();
		public static readonly Dictionary<Guid, NodePropertyPort> NodePropertyPorts = new Dictionary<Guid, NodePropertyPort>();
		public static readonly Dictionary<Guid, ObservableCollection<Guid>> SelectedGuids = new Dictionary<Guid, ObservableCollection<Guid>>();
		public static bool OutputDebugInfo = false;
		public static SelectionMode SelectionMode = SelectionMode.Overlap;

		#endregion // Fields

		#region FlowChart

		/// <summary>
		/// Create FlowChart with FlowChartViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid of this FlowChart.</param>
		/// <param name="flowChartModelType">Type of FlowChart to be created.</param>
		/// <returns>Created FlowChart instance</returns>
		public static FlowChart CreateFlowChart( bool isDeserializing, Guid guid, Type flowChartModelType )
		{
			//------ create FlowChart.

			var flowChartAttrs = flowChartModelType.GetCustomAttributes( typeof( FlowChartAttribute ), false ) as FlowChartAttribute[];
			if( 1 != flowChartAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE FlowChartAttribute", flowChartModelType.Name ) );
			var flowChartAttr = flowChartAttrs[ 0 ];

			FlowChart flowChart = Activator.CreateInstance( flowChartModelType, new object[] { guid } ) as FlowChart;
			FlowCharts.Add( flowChart.Guid, flowChart );

			//----- create viewmodel

			flowChart.ViewModel = Activator.CreateInstance( flowChartAttr.ViewModelType, new object[] { flowChart } ) as FlowChartViewModel;

			//----- create selection list.

			ObservableCollection<Guid> selectionList = new ObservableCollection<Guid>();
			selectionList.CollectionChanged += Node_SelectionList_CollectionChanged;
			SelectedGuids.Add( flowChart.Guid, selectionList );

			//----- invocke create callback.

			if( !isDeserializing )
			{
				flowChart.OnCreate();
			}

			//----- return.

			return flowChart;
		}

		public static void DestroyFlowChart( Guid guid )
		{
			FlowChart flowChart;
			if( !FlowCharts.TryGetValue( guid, out flowChart ) )
			{
				return;
			}

			flowChart.OnPreDestroy();

			ObservableCollection<Guid> guids = new ObservableCollection<Guid>();
			foreach( var node in flowChart.Nodes )
			{
				guids.Add( node.Guid );
			}

			foreach( var nodeGuid in guids )
			{
				DestroyNode( nodeGuid );
			}

			if( 0 < flowChart.Connectors.Count )
			{
				throw new InvalidOperationException( "Connectors are not removed." );
			}

			flowChart.OnPostDestroy();

			SelectedGuids.Remove( guid );
			FlowCharts.Remove( guid );
		}

		public static FlowChart FindFlowChart( Guid guid )
		{
			FlowChart flowChart;
			FlowCharts.TryGetValue( guid, out flowChart );
			return flowChart;
		}

		public static FlowChart FindFlowChart(string name)
		{
			return FlowCharts.Values.FirstOrDefault(x => x.Name == name);
        }
		#endregion // FlowChart

		#region Node

		/// <summary>
		/// Create Node with NodeViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.
		/// If it is true, Node's attribute will not be evaluated. That means flows and properties will not be created automatically by attributes.
		/// All flows and properties will be created during deserialization process.</param>
		/// <param name="guid">Guid for this Node.</param>
		/// <param name="flowChart">Owner FlowChart.</param>
		/// <param name="nodeType">Type of this node.</param>
		/// <param name="x">Location along X axis( Canvas.Left ).</param>
		/// <param name="y">Location along Y axis( Canvas.Top )</param>
		/// <param name="ZIndex">Z index( Canvas.ZIndex ).</param>
		/// <param name="headerOverride">User defined header.</param>
		/// <param name="nodeViewModelTypeOverride">NodeViewModel to override.</param>
		/// <param name="flowPortViewModelTypeOverride">FlowPortViewModel to override.</param>
		/// <param name="propertyPortViewModelTypeOverride">PropertyPortViewmodel to override.</param>
		/// <returns>Created node instance.</returns>
		public static Node CreateNode( bool isDeserializing, Guid guid, FlowChart flowChart, Type nodeType, double x, double y, int ZIndex,
			Type nodeViewModelTypeOverride = null, Type flowPortViewModelTypeOverride = null, Type propertyPortViewModelTypeOverride = null )
		{
			//----- exceptions.

			if( null == flowChart )
				throw new ArgumentNullException( "flowChart of CreateNode() can not be null" );

			if( null == nodeType )
				throw new ArgumentNullException( "nodeType of CreateNode() can not be null" );

			//----- create node from NodeAttribute.

			var nodeAttrs = nodeType.GetCustomAttributes( typeof( NodeAttribute ), false ) as NodeAttribute[];
			if( 1 != nodeAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE NodeAttribute", nodeType.Name ) );
			var nodeAttr = nodeAttrs[ 0 ];

			// create node model.
			Node node = Activator.CreateInstance( nodeType, new object[] { guid, flowChart } ) as Node;
			node.X = x;
			node.Y = y;
			node.ZIndex = ZIndex;
			Nodes.Add( guid, node );
            Selectables.Add(guid, node);
            // create node viewmodel.
            node.ViewModel = Activator.CreateInstance(
				( null != nodeViewModelTypeOverride ) ? nodeViewModelTypeOverride : nodeAttr.ViewModelType,
				new object[] { node } ) as NodeViewModel;
			flowChart.ViewModel.NodeViewModels.Add( node.ViewModel );
			flowChart.Nodes.Add( node );


			//----- create ports.

			if( !isDeserializing )
			{
				//----- create flowPorts from NodeFlowPortAttribute.

				var flowPortAttrs = nodeType.GetCustomAttributes( typeof( NodeFlowPortAttribute ), false ) as NodeFlowPortAttribute[];
				foreach( var attr in flowPortAttrs )
				{
					CreateNodeFlowPort( false,
						Guid.NewGuid(), node, attr.IsInput,
						( null != flowPortViewModelTypeOverride ) ? flowPortViewModelTypeOverride : attr.ViewModelType,
						attr.Name, attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled,
                        -1, attr.FontColorConverterType);
				}

				//----- create nodePropertyPorts( property ) from NodePropertyAttribute.

				var propertyInfos = nodeType.GetProperties( BindingFlags.Public | BindingFlags.Instance );
				foreach( var propertyInfo in propertyInfos )
				{
					var nodePropertyAttrs = propertyInfo.GetCustomAttributes( typeof( NodePropertyPortAttribute ), false ) as NodePropertyPortAttribute[];
					if( null != nodePropertyAttrs )
					{
						foreach( var attr in nodePropertyAttrs )
						{
							CreateNodePropertyPort( false, Guid.NewGuid(), node, attr.IsInput,
								attr.ValueType, attr.DefaultValue, propertyInfo.Name, attr.HasEditor,
								( null != propertyPortViewModelTypeOverride ) ? propertyPortViewModelTypeOverride : attr.ViewModelType,
								attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled,
                                attr.Index, attr.Serialized, attr.FontColorConverterType);
						}
					}
				}

				//----- create nodePropertyPorts( field ) from NodePropertyAttribute.

				var fieldInfos = nodeType.GetFields( BindingFlags.Public | BindingFlags.Instance );
				foreach( var fieldInfo in fieldInfos )
				{
					var nodePropertyAttrs = fieldInfo.GetCustomAttributes( typeof( NodePropertyPortAttribute ), false ) as NodePropertyPortAttribute[];
					if( null != nodePropertyAttrs )
					{
						foreach( var attr in nodePropertyAttrs )
						{
							CreateNodePropertyPort( false, Guid.NewGuid(), node, attr.IsInput,
								attr.ValueType, attr.DefaultValue, fieldInfo.Name, attr.HasEditor,
								( null != propertyPortViewModelTypeOverride ) ? propertyPortViewModelTypeOverride : attr.ViewModelType,
								attr.DisplayName, attr.AllowMultipleInput, attr.AllowMultipleOutput, attr.IsPortEnabled, attr.IsEnabled,
                                attr.Index, attr.Serialized, attr.FontColorConverterType);
						}
					}
				}

				//----- invoke Create callback.

				node.OnCreate();


                //---- history.

                flowChart.History.AddCommand(new NodeGraph.History.CreateNodeCommand(
                    "Creating node", node.Guid, NodeGraphManager.SerializeNode(node)));
			}

			//----- return.

			return node;
		}

		public static void DestroyNode(Guid guid)
		{
            if (Nodes.TryGetValue(guid, out var node))
			{
				//----- destroy.

				node.OnPreDestroy();

				List<Guid> connectorGuids = new List<Guid>();
				List<Guid> portGuids = new List<Guid>();

				foreach (var port in node.InputFlowPorts)
				{
					foreach (var connector in port.Connectors)
					{
						if (!connectorGuids.Contains(connector.Guid))
							connectorGuids.Add(connector.Guid);
					}
					portGuids.Add(port.Guid);
				}

				foreach (var port in node.OutputFlowPorts)
				{
					foreach (var connector in port.Connectors)
					{
						if (!connectorGuids.Contains(connector.Guid))
							connectorGuids.Add(connector.Guid);
					}
					portGuids.Add(port.Guid);
				}

				foreach (var port in node.InputPropertyPorts)
				{
					foreach (var connector in port.Connectors)
					{
						if (!connectorGuids.Contains(connector.Guid))
							connectorGuids.Add(connector.Guid);
					}
					portGuids.Add(port.Guid);
				}

				foreach (var port in node.OutputPropertyPorts)
				{
					foreach (var connector in port.Connectors)
					{
						if (!connectorGuids.Contains(connector.Guid))
							connectorGuids.Add(connector.Guid);
					}
					portGuids.Add(port.Guid);
				}

				foreach (var connectorGuid in connectorGuids)
				{
					DestroyConnector(connectorGuid);
				}

				foreach (var portGuid in portGuids)
				{
					DestroyNodePort(portGuid);
				}

				FlowChart flowChart = node.Owner;
				flowChart.ViewModel.NodeViewModels.Remove(node.ViewModel);
				flowChart.Nodes.Remove(node);

				ObservableCollection<Guid> selectionList = GetSelectionList(node.Owner);
				selectionList.Remove(guid);

				node.OnPostDestroy();

				node.Owner.History.AddCommand(new NodeGraph.History.DestroyNodeCommand(
					"Destroying node", SerializeNode(node), node.Guid));

				Nodes.Remove(guid);
				Selectables.Remove(guid);
			}
		}

		public static Node FindNode( Guid guid )
		{
			Node node;
			Nodes.TryGetValue( guid, out node );
			return node;
		}
        
		public static List<Node> FindNode( FlowChart flowChart, string header )
		{
			List<Node> nodes = new List<Node>();

			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( ( flowChart == node.Owner ) && ( header == node.Header ) )
				{
					nodes.Add( node );
				}
			}

			return nodes;
		}

		#endregion // Node

        #region Selectables
		public static ISelectable FindSelectable(Guid guid)
        {
            Selectables.TryGetValue(guid, out var selectable);
            return selectable;
        }
        #endregion // Selectables
        
		#region Connector

		public static Connector CreateConnector( bool isDeserializing, Guid guid, FlowChart flowChart, Type connectorType = null )
		{
			//----- exceptions.

			if( null == flowChart )
				throw new ArgumentNullException( "flowChart of CreateNode() can not be null" );

			//------ create connector.

			if( null == connectorType )
				connectorType = typeof( Connector );

			var connectorAttrs = connectorType.GetCustomAttributes( typeof( ConnectorAttribute ), false ) as ConnectorAttribute[];
			if( 1 != connectorAttrs.Length )
				throw new ArgumentException( string.Format( "{0} must have ONE ConnectorAttribute", connectorType.Name ) );
			var connectorAttr = connectorAttrs[ 0 ];

			Connector connector = Activator.CreateInstance( connectorType, new object[] { guid, flowChart } ) as Connector;
			Connectors.Add( connector.Guid, connector );

			//----- create viewmodel

			connector.ViewModel = Activator.CreateInstance( connectorAttr.ViewModelType, new object[] { connector } ) as ConnectorViewModel;
			flowChart.ViewModel.ConnectorViewModels.Add( connector.ViewModel );
			flowChart.Connectors.Add( connector );

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				connector.OnCreate();
			}

			//----- return.

			return connector;
		}

		public static void DestroyConnector( Guid guid )
		{
			Connector connector;
			if( Connectors.TryGetValue( guid, out connector ) )
			{
				//----- history.

				connector.FlowChart.History.AddCommand( new NodeGraph.History.DestroyConnectorCommand(
					"Destroying connector", SerializeConnector( connector ), connector.Guid ) );

				//----- destroy.

				connector.OnPreDestroy();

				if( null != connector.StartPort )
				{
					DisconnectFrom( connector.StartPort, connector );
				}

				if( null != connector.EndPort )
				{
					DisconnectFrom( connector.EndPort, connector );
				}

				FlowChart flowChart = connector.FlowChart;
                var routers = flowChart.Routers.Where(r => r.Connector == connector).ToList();
                foreach (var router in routers)
                {
                    DestroyRouter(router.Guid);
                }
                flowChart.ViewModel.ConnectorViewModels.Remove( connector.ViewModel );
				flowChart.Connectors.Remove( connector );

				connector.OnPostDestroy();
				Connectors.Remove( guid );
			}
		}

		public static Connector FindConnector( Guid guid )
		{
			Connector connector;
			Connectors.TryGetValue( guid, out connector );
			return connector;
		}

        public static Router CreateRouter(Guid guid, FlowChart flowChart)
        {
			if (flowChart == null)
			{
				throw new ArgumentNullException("flowChart of CreateRouter() can not be null");
			}
            var router = new Router(guid, flowChart);
            router.ViewModel = new RouterViewModel(router);
            flowChart.Routers.Add(router);
            flowChart.ViewModel.RouterViewModels.Add(router.ViewModel);
            Routers.Add(guid, router);
			Selectables.Add(guid, router);
			return router;
        }
		
		public static Router FindRouter(Guid guid)
        {
            Routers.TryGetValue(guid, out var router);
            return router;
        }
        
		public static void DestroyRouter(Guid guid)
		{
			if (Routers.TryGetValue(guid, out var router))
			{
                var flowChart = router.Owner;
                flowChart.ViewModel.RouterViewModels.Remove(router.ViewModel);
                flowChart.Routers.Remove(router);

                var routersNext = flowChart.Routers.Where(r => r.Connector == router.Connector && r.Index > router.Index).ToList();
				foreach (var routerNext in routersNext)
                {
                    routerNext.Index--;
                }

				var selectionList = GetSelectionList(flowChart);
                selectionList.Remove(guid);
				
				flowChart.History.AddCommand(new History.DestroyRouterCommand(
					"Destroying router", SerializeRouter(router), router.Guid));

                Routers.Remove(guid);
                Selectables.Remove(guid);
            }
		}
		#endregion // Connector

		#region Port

		public static NodePort FindNodePort( Guid guid )
		{
			NodePort port = FindNodeFlowPort( guid );
			if( null == port )
				port = FindNodePropertyPort( guid );
			return port;
		}

		public static void DestroyNodePort( Guid guid )
		{
			// ---- exception.

			NodePort port = FindNodePort( guid );
			if( null == port )
			{
				return;
			}

			//----- history.

			Node node = port.Owner;

			node.Owner.History.AddCommand( new History.DestroyNodePortCommand(
				"Destroying port", SerializeNodePort( port ), port.Guid ) );

			//----- destroy.

			bool isFlowPort = ( port is NodeFlowPort );

			port.OnPreDestroy();

			List<Guid> guids = new List<Guid>();
			foreach( var connector in port.Connectors )
			{
				guids.Add( connector.Guid );
			}

			foreach( var connectorGuid in guids )
			{
				DestroyConnector( connectorGuid );
			}

			if( port.IsInput )
			{
				if (isFlowPort)
				{
					node.ViewModel.InputFlowPortViewModels.Remove(port.ViewModel as NodeFlowPortViewModel);
					node.InputFlowPorts.Remove(port as NodeFlowPort);
				}
				else
				{
                    node.ViewModel.InputPropertyPortViewModels.Remove(port.ViewModel as NodePropertyPortViewModel);
					node.InputPropertyPorts.Remove(port as NodePropertyPort);
				}
			}
			else
			{
				if (isFlowPort)
				{
					node.ViewModel.OutputFlowPortViewModels.Remove(port.ViewModel as NodeFlowPortViewModel);
					node.OutputFlowPorts.Remove(port as NodeFlowPort);
				}
				else
				{
                    node.ViewModel.OutputPropertyPortViewModels.Remove(port.ViewModel as NodePropertyPortViewModel);
					node.OutputPropertyPorts.Remove(port as NodePropertyPort);
				}
			}

			port.OnPostDestroy();

			if( isFlowPort )
				NodeFlowPorts.Remove( guid );
			else
				NodePropertyPorts.Remove( guid );
		}

		private static void FindConnectedPortsInternal( NodePort port, List<NodePort> outConnectedPorts )
		{
			if( null == port )
			{
				return;
			}

			if( 0 < port.Connectors.Count )
			{
				if( port.IsInput )
				{
					foreach( var connector in port.Connectors )
					{
						NodePort nextPort = connector.StartPort;
                        outConnectedPorts.Add(nextPort);
					}
				}
				else
				{
					foreach( var connector in port.Connectors )
					{
						NodePort nextPort = connector.EndPort;
                        outConnectedPorts.Add(nextPort);
					}
				}
			}
		}

		public static void FindConnectedPorts( NodePort port, out List<NodePort> outConnectedPorts )
		{
			outConnectedPorts = new List<NodePort>();
			FindConnectedPortsInternal( port, outConnectedPorts );
		}

		#endregion Port

		#region FlowPort

		/// <summary>
		/// Create NodeFlowPort with NodeFlwoPortViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid for this port.</param>
		/// <param name="node">Owner of this port.</param>
		/// <param name="name">Name of port.</param>
		/// <param name="displayName">Display name of port.</param>
		/// <param name="isInput">Is input port?</param>
		/// <param name="allowMultipleInput">Multiple inputs are allowed for this port?</param>
		/// <param name="allowMultipleOutput">Multiple outputs are allowed for this port?</param>
		/// <param name="portViewModelTypeOverride">ViewModelType to override.</param>
		/// <returns>Created NodeFlwoPort instance.</returns>
		public static NodeFlowPort CreateNodeFlowPort( bool isDeserializing, Guid guid, Node node, bool isInput, Type portViewModelTypeOverride = null,
			string name = "None", string displayName = "None", bool allowMultipleInput = true, bool allowMultipleOutput = false, bool isPortEnabled = true, bool isEnabled = true,
            int index = -1, Type fontColorConverterType = null)
		{
			//----- exceptions.

			if( null == node )
				throw new ArgumentNullException( "node of CreateNodeFlowPort() can not be null" );

			//----- create port.

			// create flowPort model.
			NodeFlowPort port = Activator.CreateInstance( typeof( NodeFlowPort ),
				new object[] { guid, node, isInput } ) as NodeFlowPort;
			port.Name = name;
			port.DisplayName = displayName;
			port.AllowMultipleInput = allowMultipleInput;
			port.AllowMultipleOutput = allowMultipleOutput;
			port.IsPortEnabled = isPortEnabled;
			port.IsEnabled = isEnabled;
			if (fontColorConverterType != null)
            {
                var fontColorConverter = (IColorConverter)Activator.CreateInstance(fontColorConverterType);
                port.TextForegroundColor = fontColorConverter.GetColor(port);
			}
            NodeFlowPorts.Add( port.Guid, port );

			// create flowPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != portViewModelTypeOverride ) ? portViewModelTypeOverride : typeof( NodeFlowPortViewModel ),
				new object[] { port } ) as NodeFlowPortViewModel;

			// add port to node.
			port.ViewModel = portVM;
			if ( isInput )
			{
				if (index < 0)
				{
					node.InputFlowPorts.Add(port);
					node.ViewModel.InputFlowPortViewModels.Add(portVM);
				}
                else
				{
					index = Math.Min(index, node.InputFlowPorts.Count - 1);
					node.InputFlowPorts.Insert(index, port);
                    node.ViewModel.InputFlowPortViewModels.Insert(index, portVM);
				}
			}
			else
			{
				if (index == -1)
				{
					node.OutputFlowPorts.Add(port);
					node.ViewModel.OutputFlowPortViewModels.Add(portVM);
				}
                else
				{
					index = Math.Min(index, node.OutputFlowPorts.Count - 1);
					node.OutputFlowPorts.Insert(index, port);
                    node.ViewModel.OutputFlowPortViewModels.Insert(index, portVM);
				}
			}

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				port.OnCreate();
			}
			
			//----- return.

			return port;
		}

		public static NodeFlowPort FindNodeFlowPort( Guid guid )
		{
			NodeFlowPort port;
			NodeFlowPorts.TryGetValue( guid, out port );
			return port;
		}

		public static NodeFlowPort FindNodeFlowPort( Node node, string propertyName )
		{
			foreach( var pair in NodeFlowPorts )
			{
				NodeFlowPort port = pair.Value;
				if( ( node == port.Owner ) && ( propertyName == port.Name ) )
				{
					return port;
				}
			}

			return null;
		}

		#endregion // FlowPort

		#region PropertyPort

		/// <summary>
		/// Create PropertyPort with PropertyPortViewModel.
		/// </summary>
		/// <param name="isDeserializing">Is in deserializing routine? 
		/// If it is true, OnCreate() callback will not be called, otherwise OnDeserialize will be called.</param>
		/// <param name="guid">Guid for this port.</param>
		/// <param name="node">Owner of this port.</param>
		/// <param name="name">Name of port.</param>
		/// <param name="displayName">Display name of port.</param>
		/// <param name="isInput">Is input port?</param>
		/// <param name="allowMultipleInput">Multiple inputs are allowed for this port?</param>
		/// <param name="allowMultipleOutput">Multiple outputs are allowed for this port?</param>
		/// <param name="valueType">Type of property value.</param>
		/// <param name="defaultValue">Default property value.</param>
		// <param name="portViewModelTypeOverride">ViewModelType to override.</param>
		/// <returns>Created NodePropertyPort instance.</returns>
		public static NodePropertyPort CreateNodePropertyPort( bool isDeserializing, Guid guid, Node node, bool isInput, Type valueType, object defaultValue, string name, bool hasEditor,
			Type portViewModelTypeOverride = null, string displayName = "", bool allowMultipleInput = false, bool allowMultipleOutput = true, bool isPortEnabled = true, bool isEnabled = true,
            int index = -1, bool serializeValue = true, Type fontColorConverterType = null)
		{
			//----- exceptions.

			if( null == node )
				throw new ArgumentNullException( "node of CreateNodePropertyPort() can not be null" );

			//----- create port.

			// create propertyPort model.
			NodePropertyPort port = Activator.CreateInstance( typeof( NodePropertyPort ),
				new object[] { guid, node, isInput, valueType, defaultValue, name, hasEditor, serializeValue } ) as NodePropertyPort;
			port.DisplayName = displayName;
			port.AllowMultipleInput = allowMultipleInput;
			port.AllowMultipleOutput = allowMultipleOutput;
			port.IsPortEnabled = isPortEnabled;
			port.IsEnabled = isEnabled;
			if (fontColorConverterType != null)
            {
                var fontColorConverter = (IColorConverter)Activator.CreateInstance(fontColorConverterType);
				port.TextForegroundColor = fontColorConverter.GetColor(port);
			}
			NodePropertyPorts.Add( port.Guid, port );

			// create propertyPort viewmodel.
			var portVM = Activator.CreateInstance( ( null != portViewModelTypeOverride ) ? portViewModelTypeOverride : typeof( NodePropertyPortViewModel ),
				new object[] { port } ) as NodePropertyPortViewModel;
			port.ViewModel = portVM;

			// add to node.
			if ( port.IsInput )
			{
				if (index < 0 || node.InputFlowPorts.Count == 0)
				{
					node.InputPropertyPorts.Add(port);
					node.ViewModel.InputPropertyPortViewModels.Add(portVM);
				}
                else
				{
					index = Math.Min(index, node.InputFlowPorts.Count - 1);
					node.InputPropertyPorts.Insert(index, port);
                    node.ViewModel.InputPropertyPortViewModels.Insert(index, portVM);
				}
			}
			else
			{
				if (index < 0 || node.OutputPropertyPorts.Count == 0)
				{
					node.OutputPropertyPorts.Add(port);
					node.ViewModel.OutputPropertyPortViewModels.Add(portVM);
				}
                else
				{
					index = Math.Min(index, node.OutputPropertyPorts.Count - 1);
					node.OutputPropertyPorts.Insert(index, port);
                    node.ViewModel.OutputPropertyPortViewModels.Insert(index, portVM);
				}
			}

			//----- invoke Create callback.

			if( !isDeserializing )
			{
				port.OnCreate();
			}
			
			//----- return.

			return port;
		}

		public static NodePropertyPort FindNodePropertyPort( Guid guid )
		{
			NodePropertyPort port;
			NodePropertyPorts.TryGetValue( guid, out port );
			return port;
		}

		public static NodePropertyPort FindNodePropertyPort( Node node, string propertyName )
		{
			foreach( var pair in NodePropertyPorts )
			{
				NodePropertyPort port = pair.Value;
				if( ( node == port.Owner ) && ( propertyName == port.Name ) )
				{
					return port;
				}
			}

			return null;
		}

		#endregion // PropertyPort

		#region Connection

		public static bool IsConnecting { get; private set; }
		public static NodePort FirstConnectionPort { get; private set; }
		public static Connector CurrentConnector { get; private set; }

		public static void ConnectTo( NodePort port, Connector connector )
		{
			if( port.IsInput )
			{
				connector.EndPort = port;
			}
			else
			{
				connector.StartPort = port;
			}
			port.Connectors.Add( connector );

			port.OnConnect( connector );
			connector.OnConnect( port );
		}

		public static void DisconnectFrom( NodePort port, Connector connector )
		{
			if( null == port )
				return;

			connector.OnDisconnect( port );
			port.OnDisconnect( connector );

			if( port.IsInput )
			{
				connector.EndPort = null;
			}
			else
			{
				connector.StartPort = null;
			}
			port.Connectors.Remove( connector );
		}

		public static void DisconnectAll( NodePort port )
		{
			List<Guid> connectorGuids = new List<Guid>();
			foreach( var connection in port.Connectors )
			{
				connectorGuids.Add( connection.Guid );
			}

			foreach( var guid in connectorGuids )
			{
				DestroyConnector( guid );
			}
		}

		public static void BeginConnection( NodePort port )
		{
			if( IsConnecting )
				throw new InvalidOperationException( "You can not connect node during other connection occurs." );

			IsConnecting = true;

			Node node = port.Owner;
			FlowChart flowChart = node.Owner;
			FlowChartView flowChartView = flowChart.ViewModel.View;

			BeginDragging( flowChartView );

			CurrentConnector = CreateConnector( false, Guid.NewGuid(), flowChart, typeof( Connector ) );
			ConnectTo( port, CurrentConnector );

			FirstConnectionPort = port;
		}

		public static void SetOtherConnectionPort( NodePort port )
		{
			if( null == port )
			{
				if( ( null != CurrentConnector.StartPort ) && ( CurrentConnector.StartPort == FirstConnectionPort ) )
				{
					if( null != CurrentConnector.EndPort )
					{
						DisconnectFrom( CurrentConnector.EndPort, CurrentConnector );
					}
				}
				else if( ( null != CurrentConnector.EndPort ) && ( CurrentConnector.EndPort == FirstConnectionPort ) )
				{
					if( null != CurrentConnector.StartPort )
					{
						DisconnectFrom( CurrentConnector.StartPort, CurrentConnector );
					}
				}
			}
			else
			{
				ConnectTo( port, CurrentConnector );
			}
		}

		private static List<Node> _AlreadyCheckedNodes;

		public static bool CheckIfConnectable( NodePort otherPort, out string error )
		{
			Type firstType = FirstConnectionPort.GetType();
			Type otherType = otherPort.GetType();

			Node firstNode = FirstConnectionPort.Owner;
			Node otherNode = otherPort.Owner;

			error = "";

			// same port.
			if( FirstConnectionPort == otherPort )
			{
				//error = "It's a same port.";
				return false;
			}

			// same node.
			if( firstNode == otherNode )
			{
				error = "It's a port of same node.";
				return false;
			}

			bool areAllPropertyPorts = ( typeof( NodePropertyPort ).IsAssignableFrom( firstType ) && typeof( NodePropertyPort ).IsAssignableFrom( otherType ) );
			bool areAllFlowPorts = ( typeof( NodeFlowPort ).IsAssignableFrom( firstType ) && typeof( NodeFlowPort ).IsAssignableFrom( otherType ) );

			// different type of ports
			if( !areAllPropertyPorts && !areAllFlowPorts )
			{
				error = "Port type is not same with other's.";
				return false;
			}

			// same orientation.
			if( FirstConnectionPort.IsInput == otherPort.IsInput )
			{
				error = "Ports are all input or output.";
				return false;
			}

			// already connectecd.
			foreach( var connector in FirstConnectionPort.Connectors )
			{
				if( connector.StartPort == otherPort )
				{
					error = "Already connected";
					return false;
				}
			}

			// different type of value.
			if( areAllPropertyPorts )
			{
				NodePropertyPort firstPropPort = FirstConnectionPort as NodePropertyPort;
				NodePropertyPort otherPropPort = otherPort as NodePropertyPort;
				if( !firstPropPort.IsInput )
				{
					if( !otherPropPort.ValueType.IsAssignableFrom( firstPropPort.ValueType ) )
					{
						error = "Value type is not assignable";
						return false;
					}
				}
				else
				{
					if( !firstPropPort.ValueType.IsAssignableFrom( otherPropPort.ValueType ) )
					{
						error = "Value type is not assignable";
						return false;
					}
				}
			}

			// circular test
			if( !otherPort.Owner.AllowCircularConnection )
			{
				_AlreadyCheckedNodes = new List<Node>();
				if( IsReachable(
					FirstConnectionPort.IsInput ? firstNode : otherNode,
					FirstConnectionPort.IsInput ? otherNode : firstNode ) )
				{
					error = "Circular connection";
					_AlreadyCheckedNodes = null;
					return false;
				}
				_AlreadyCheckedNodes = null;
			}

			return FirstConnectionPort.IsConnectable( otherPort, out error );
		}

		private static bool IsReachable( Node nodeFrom, Node nodeTo )
		{
			if( _AlreadyCheckedNodes.Contains( nodeFrom ) )
				return false;

			_AlreadyCheckedNodes.Add( nodeFrom );

			foreach( var port in nodeFrom.OutputFlowPorts )
			{
				foreach( var connector in port.Connectors )
				{
					NodePort endPort = connector.EndPort;
					Node nextNode = endPort.Owner;
					if( nextNode == nodeTo )
						return true;

					if( IsReachable( nextNode, nodeTo ) )
						return true;
				}
			}

			foreach( var port in nodeFrom.OutputPropertyPorts )
			{
				foreach( var connector in port.Connectors )
				{
					NodePort endPort = connector.EndPort;
					Node nextNode = endPort.Owner;
					if( nextNode == nodeTo )
						return true;

					if( IsReachable( nextNode, nodeTo ) )
						return true;
				}
			}

			return false;
		}

		public static bool EndConnection( NodePort endPort = null )
		{
			EndDragging();

			bool bResult = false;

			if( !IsConnecting )
			{
				return false;
			}

			if( null != endPort )
			{
				SetOtherConnectionPort( endPort );
			}

			if( ( null == CurrentConnector.StartPort ) || ( null == CurrentConnector.EndPort ) )
			{
				DestroyConnector( CurrentConnector.Guid );
			}
			else
			{
				NodePort startPort = FindNodePort( CurrentConnector.StartPort.Guid );
				if( null == endPort )
					endPort = FindNodePort( CurrentConnector.EndPort.Guid );
				if( !startPort.AllowMultipleOutput )
				{
					List<Guid> connectorGuids = new List<Guid>();
					foreach( var connector in startPort.Connectors )
					{
						if( CurrentConnector.Guid != connector.Guid )
						{
							connectorGuids.Add( connector.Guid );
						}
					}

					foreach( var guid in connectorGuids )
					{
						DestroyConnector( guid );
					}
				}

				if( !endPort.AllowMultipleInput )
				{
					List<Guid> connectorGuids = new List<Guid>();
					foreach( var connector in endPort.Connectors )
					{
						if( CurrentConnector.Guid != connector.Guid )
						{
							connectorGuids.Add( connector.Guid );
						}
					}

					foreach( var guid in connectorGuids )
					{
						DestroyConnector( guid );
					}
				}

				//----- history.

				CurrentConnector.FlowChart.History.AddCommand( new History.CreateConnectorCommand(
					"Creating connector", CurrentConnector.Guid, SerializeConnector( CurrentConnector ) ) );

				bResult = true;
			}

			IsConnecting = false;
			CurrentConnector = null;
			FirstConnectionPort = null;

			return bResult;
		}

		public static void UpdateConnection( Point mousePos )
		{
			if( null != CurrentConnector )
				CurrentConnector.ViewModel.view.BuildCurveData( mousePos );
		}

		#endregion // Connection

		#region Node Dragging

		public static bool IsSelectableDragging { get; private set; }
		public static bool AreSelectablesReallyDragged { get; private set; }
		private static Guid _selectableDraggingFlowChartGuid;

		public static void BeginDragSelectable( FlowChart flowChart )
		{
			BeginDragging( flowChart.ViewModel.View );

			if( IsSelectableDragging )
				throw new InvalidOperationException("Selectable is already dragging.");

			IsSelectableDragging = true;
			_selectableDraggingFlowChartGuid = flowChart.Guid;
		}
        
        public static void EndDragSelectable()
		{
			EndDragging();

			IsSelectableDragging = false;
			AreSelectablesReallyDragged = false;
		}

		public static void DragSelectable(Point delta)
		{
			if (!IsSelectableDragging)
				return;

			AreSelectablesReallyDragged = true;

			if (SelectedGuids.TryGetValue(_selectableDraggingFlowChartGuid, out var selected))
			{
				foreach (var guid in selected)
				{
					var selectable = FindSelectable(guid);
					if (selectable != null)
					{
						selectable.X += delta.X;
						selectable.Y += delta.Y;
					}
				}
			}
		}
		#endregion // Node Dragging

		#region Mouse Trapping

		[DllImport( "user32.dll" )]
		public static extern void ClipCursor( ref System.Drawing.Rectangle rect );

		[DllImport( "user32.dll" )]
		public static extern void ClipCursor( IntPtr rect );

		private static FlowChartView _TrappingFlowChartView;
		public static bool IsDragging = false;

		public static void BeginDragging( FlowChartView flowChartView )
		{
			_TrappingFlowChartView = flowChartView;
			IsDragging = true;

			Point startLocation = flowChartView.PointToScreen( new Point( 0, 0 ) );

			System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
				( int )startLocation.X + 2, ( int )startLocation.Y + 2,
				( int )( startLocation.X + flowChartView.ActualWidth ) - 2,
				( int )( startLocation.Y + flowChartView.ActualHeight ) - 2 );
			ClipCursor( ref rect );
		}

		public static void EndDragging()
		{
			if( null != _TrappingFlowChartView )
			{
				IsDragging = false;
				_TrappingFlowChartView = null;
			}

			ClipCursor( IntPtr.Zero );
		}

		#endregion // Mouse Trapping

		#region Node Selection

		public static ISelectable MouseLeftDownSelectable { get; set; }

		public static ObservableCollection<Guid> GetSelectionList( FlowChart flowChart )
		{
            return !SelectedGuids.TryGetValue( flowChart.Guid, out var selectionList ) ? null : selectionList;
        }

		public static void TrySelection(FlowChart flowChart, ISelectable selectable, bool bCtrl, bool bShift, bool bAlt)
		{
			bool bAdd;
			if (bCtrl)
			{
				bAdd = !selectable.IsSelected;
			}
			else if (bShift)
			{
				bAdd = true;
			}
			else if (bAlt)
			{
				bAdd = false;
			}
			else
			{
				DeselectAll(flowChart);
				bAdd = true;
			}

			if (bAdd)
			{
				if (!selectable.IsSelected)
				{
					AddSelection(selectable);
					flowChart.History.AddCommand(new History.SelectablePropertyCommand(
						"Selection", selectable.Guid, "IsSelected", false, true));
				}
			}
			else
			{
				if (selectable.IsSelected)
				{
					RemoveSelection(selectable);
					flowChart.History.AddCommand(new History.SelectablePropertyCommand(
						"Selection", selectable.Guid, "IsSelected", true, false));
				}
			}
		}

		public static void AddSelection(ISelectable selectable)
		{
			if (selectable.IsSelected)
			{
				return;
			}
			var selectionList = GetSelectionList(selectable.Owner);
			if (!selectionList.Contains(selectable.Guid))
			{
				selectable.IsSelected = true;
				selectionList.Add(selectable.Guid);
			}
            MoveSelectableToFront(selectable);
		}

		public static void RemoveSelection(ISelectable selectable)
		{
			var selectionList = GetSelectionList(selectable.Owner);
			selectable.IsSelected = false;
			selectionList.Remove(selectable.Guid);
		}

		public static void DeselectAll(FlowChart flowChart)
		{
			var selectionList = GetSelectionList(flowChart);
			foreach (var guid in selectionList)
			{
				var selectable = FindSelectable(guid);
				if (selectable != null)
				{
					selectable.IsSelected = false;
					flowChart.History.AddCommand(new History.SelectablePropertyCommand(
						"Deselection", selectable.Guid, "IsSelected", true, false));
				}
			}
			selectionList.Clear();
		}

		public static void SelectAll(FlowChart flowChart)
		{
			DeselectAll(flowChart);
            var selectionList = GetSelectionList(flowChart);
			foreach (var selectable in Selectables
                         .Select(pair => pair.Value)
                         .Where(selectable => selectable.Owner == flowChart))
            {
                selectable.IsSelected = true;
                selectionList.Add(selectable.Guid);
            }
		}

		public static bool IsSelecting => _FlowChartSelecting != null;
        private static FlowChart _FlowChartSelecting;
		public static Point SelectingStartPoint { get; private set; }
		private static Guid[] _OriginalSelections;

		public static void BeginDragSelection(FlowChart flowChart, Point start)
		{
			FlowChartView flowChartView = flowChart.ViewModel.View;
			BeginDragging(flowChartView);

			SelectingStartPoint = start;

			_FlowChartSelecting = flowChart;
			_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Visible;

			SelectedGuids.TryGetValue(flowChart.Guid, out var temp);
			_OriginalSelections = new Guid[temp.Count];
			temp.CopyTo(_OriginalSelections, 0);
		}

        public static void UpdateDragSelection(FlowChart flowChart, Point end, bool bCtrl, bool bShift, bool bAlt)
        {
            var startX = SelectingStartPoint.X;
            var startY = SelectingStartPoint.Y;

            var selectionStart = new Point(Math.Min(startX, end.X), Math.Min(startY, end.Y));
            var selectionEnd = new Point(Math.Max(startX, end.X), Math.Max(startY, end.Y));

            var bAdd = false;
            if (bCtrl)
            {
                bAdd = true;
            }
            else if (bShift)
            {
                bAdd = true;
            }
            else if (bAlt)
            {
                bAdd = false;
            }
            else
            {
                bAdd = true;
            }

            foreach (var pair in Selectables)
            {
                var selectable = pair.Value;
                if (selectable.Owner == _FlowChartSelecting)
                {
                    var selectableStart = new Point(selectable.X, selectable.Y);
                    var selectableEnd = new Point(selectable.X + selectable.ActualWidth,
                        selectable.Y + selectable.ActualHeight);

                    var isInOriginalSelection = false;
                    foreach (var nodeGuid in _OriginalSelections)
                    {
                        if (selectable.Guid == nodeGuid)
                        {
                            isInOriginalSelection = true;
                            break;
                        }
                    }

                    var isOutside = selectableEnd.X < selectionStart.X ||
                        selectableEnd.Y < selectionStart.Y ||
                        selectableStart.X > selectionEnd.X ||
                        selectableStart.Y > selectionEnd.Y;

                    var isIncluded = !isOutside &&
                        selectableStart.X >= selectionStart.X &&
                        selectableStart.Y >= selectionStart.Y &&
                        selectableEnd.X <= selectionEnd.X &&
                        selectableEnd.Y <= selectionEnd.Y;

                    var isSelected = (SelectionMode.Include == SelectionMode && isIncluded) ||
                        (SelectionMode.Overlap == SelectionMode && !isOutside);

                    if (!isSelected)
                    {
                        if (isInOriginalSelection)
                        {
                            if (bCtrl || !bAdd)
                            {
                                AddSelection(selectable);
                            }
                        }
                        else
                        {
                            if (bCtrl || bAdd)
                            {
                                RemoveSelection(selectable);
                            }
                        }

                        continue;
                    }

                    var bThisAdd = bAdd;
                    if (isInOriginalSelection && bCtrl)
                    {
                        bThisAdd = false;
                    }

                    if (bThisAdd)
                    {
                        AddSelection(selectable);
                    }
                    else
                    {
                        RemoveSelection(selectable);
                    }
                }
            }
        }

		public static bool EndDragSelection( bool bCancel )
		{
			EndDragging();

			bool bChanged = false;

			if( IsSelecting )
			{
				if (bCancel)
				{
					if ((null != _FlowChartSelecting) && (null != _OriginalSelections))
					{
						DeselectAll(_FlowChartSelecting);

						foreach (var guid in _OriginalSelections)
						{
							AddSelection(FindSelectable(guid));
						}
					}
				}
				else
				{
					if (null != _FlowChartSelecting)
					{
						ObservableCollection<Guid> selectionList = GetSelectionList(_FlowChartSelecting);
						foreach (var guid in _OriginalSelections)
						{
							if (!selectionList.Contains(guid))
							{
								_FlowChartSelecting.History.AddCommand(new History.NodePropertyCommand(
									"Selection", guid, "IsSelected", true, false));
								bChanged = true;
							}
						}

						foreach (var guid in selectionList)
						{
							if (-1 == Array.FindIndex(_OriginalSelections, (currentGuid) => guid == currentGuid))
							{
								_FlowChartSelecting.History.AddCommand(new History.NodePropertyCommand(
									"Selection", guid, "IsSelected", false, true));
								bChanged = true;
							}
						}
					}
				}

				if( null != _FlowChartSelecting )
				{
					_FlowChartSelecting.ViewModel.SelectionVisibility = Visibility.Collapsed;
				}
				_FlowChartSelecting = null;
				_OriginalSelections = null;
			}

			return bChanged;
		}

		#endregion // Node Selection

		#region Z-Indexing

		public static void MoveSelectableToFront(ISelectable selectable)
		{
			var selectables = new List<ISelectable>();
			var maxZIndex = int.MinValue;
			foreach (var currentSelectable in Selectables.Select(pair => pair.Value))
            {
                maxZIndex = Math.Max(maxZIndex, currentSelectable.ZIndex);
                selectables.Add(currentSelectable);
            }
            selectable.ZIndex = maxZIndex + 1;
			selectables.Sort((left, right) => left.ZIndex.CompareTo(right.ZIndex));
			var zIndex = 0;
			foreach (var currentNode in selectables)
			{
				currentNode.ZIndex = zIndex++;
			}
		}

		#endregion // Z-Indexing.

		#region Delete

		public static void DestroySelectedNodes( FlowChart flowChart )
		{
			List<Guid> guids = new List<Guid>();

			ObservableCollection<Guid> selectedNodeGuids;
			SelectedGuids.TryGetValue( flowChart.Guid, out selectedNodeGuids );

			foreach( var guid in selectedNodeGuids )
			{
				guids.Add( guid );
			}

			foreach( var guid in guids )
			{
				DestroyNode( guid );
			}
		}

		#endregion // Delete

		#region ContentSize

		public static void CalculateContentSize(FlowChart flowChart, bool bOnlySelected,
			out double minX, out double maxX, out double minY, out double maxY)
		{
			minX = double.MaxValue;
			maxX = double.MinValue;
			minY = double.MaxValue;
			maxY = double.MinValue;

			bool hasNodes = false;
			foreach (var pair in Selectables)
			{
				var selectable = pair.Value;
				if (selectable.Owner == flowChart)
				{
					if (bOnlySelected && !selectable.IsSelected)
						continue;

					minX = Math.Min(selectable.X, minX);
					maxX = Math.Max(selectable.X + selectable.ActualWidth, maxX);
					minY = Math.Min(selectable.Y, minY);
					maxY = Math.Max(selectable.Y + selectable.ActualHeight, maxY);
					hasNodes = true;
				}
			}

			if (!hasNodes)
			{
				minX = maxX = minY = maxY = 0.0;
			}
		}

		#endregion // ContentSize

		#region Serialization

		private static XmlWriter CreateXmlWriter( StringWriter sw )
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.NewLineOnAttributes = false;
			XmlWriter writer = XmlWriter.Create( sw, settings );
			return writer;
		}

		public static void Serialize( string filePath )
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.NewLineOnAttributes = false;
			using( XmlWriter writer = XmlWriter.Create( filePath, settings ) )
			{
				writer.WriteStartDocument();
				{
					writer.WriteStartElement( "NodeGraphManager" );
					foreach( var pair in FlowCharts )
					{
						writer.WriteStartElement( "FlowChart" );
						pair.Value.WriteXml( writer );
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}
				writer.WriteEndDocument();

				writer.Flush();
				writer.Close();
			}
		}

		public static bool Deserialize( string filePath )
		{
			if( !File.Exists( filePath ) )
			{
				return false;
			}

			List<FlowChart> loadedFlowCharts = new List<FlowChart>();

			using( XmlReader reader = XmlReader.Create( filePath ) )
			{
				while( reader.Read() )
				{
					if( XmlNodeType.Element == reader.NodeType )
					{
						if( "FlowChart" == reader.Name )
						{
							Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) ?? throw new InvalidOperationException() );
							Type type = Type.GetType( reader.GetAttribute( "Type" ) ?? throw new InvalidOperationException() );

							FlowChart flowChart = CreateFlowChart( true, guid, type );
							flowChart.ReadXml( reader );
							loadedFlowCharts.Add( flowChart );
						}
					}
				}
			}

			foreach( var flowChart in loadedFlowCharts )
			{
				flowChart.OnDeserialize();
			}

			return true;
		}

		public static string SerializeNode( Node node )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "Node" );
			node.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeNode( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( "Node" == reader.Name )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) ?? throw new InvalidOperationException() );
						Type type = Type.GetType( reader.GetAttribute( "Type" ) ?? throw new InvalidOperationException());
						FlowChart flowChart = FindFlowChart( Guid.Parse( reader.GetAttribute( "Owner" ) ?? throw new InvalidOperationException()) );
						Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) ?? throw new InvalidOperationException());

						Node node = CreateNode( true, guid, flowChart, type, 0.0, 0.0, 0, vmType );
						node.ReadXml( reader );

						node.OnDeserialize();

						break;
					}
				}
			}
		}

		public static string SerializeConnector( Connector connector )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "Connector" );
			connector.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeConnector( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( "Connector" == reader.Name )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) ?? throw new InvalidOperationException());
						Type type = Type.GetType( reader.GetAttribute( "Type" ) ?? throw new InvalidOperationException());
						FlowChart flowChart = FindFlowChart( Guid.Parse( reader.GetAttribute( "Owner" ) ?? throw new InvalidOperationException()) );

						Connector connector = CreateConnector( true, guid, flowChart, type );
						connector.ReadXml( reader );

						connector.OnDeserialize();

						break;
					}
				}
			}
		}

        public static string SerializeRouter(Router router)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            StringWriter sw = new StringWriter(builder);
            XmlWriter writer = CreateXmlWriter(sw);

            writer.WriteStartElement("Router");
            router.WriteXml(writer);
            writer.WriteEndElement();

            sw.Flush();
            writer.Close();

            return builder.ToString();
        }

        public static void DeserializeRouter(string xml)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml));
            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    if ("Router" == reader.Name)
                    {
                        Guid guid = Guid.Parse(reader.GetAttribute("Guid") ?? throw new InvalidOperationException());
                        var flowChart = FindFlowChart(Guid.Parse(reader.GetAttribute("Owner") ?? throw new InvalidOperationException()));
                        Router router = CreateRouter(guid, flowChart);
                        router.ReadXml(reader);
                        break;
                    }
                }
            }
        }

		public static string SerializeNodePort( NodePort port )
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			StringWriter sw = new StringWriter( builder );
			XmlWriter writer = CreateXmlWriter( sw );

			writer.WriteStartElement( "NodePort" );
			port.WriteXml( writer );
			writer.WriteEndElement();

			sw.Flush();
			writer.Close();

			return builder.ToString();
		}

		public static void DeserializeNodePort( string xml )
		{
			XmlReader reader = XmlReader.Create( new StringReader( xml ) );
			while( reader.Read() )
			{
				if( XmlNodeType.Element == reader.NodeType )
				{
					if( ( "NodePort" == reader.Name ) )
					{
						Guid guid = Guid.Parse( reader.GetAttribute( "Guid" ) ?? throw new InvalidOperationException());
						Type type = Type.GetType( reader.GetAttribute( "Type" ) ?? throw new InvalidOperationException());
						Type vmType = Type.GetType( reader.GetAttribute( "ViewModelType" ) ?? throw new InvalidOperationException());
						Node node = FindNode( Guid.Parse( reader.GetAttribute( "Owner" ) ?? throw new InvalidOperationException()) );
						bool isInput = bool.Parse( reader.GetAttribute( "IsInput" ) ?? throw new InvalidOperationException());

						bool isFlowPort = typeof( NodeFlowPort ).IsAssignableFrom( type );

						if( isFlowPort )
						{
							NodeFlowPort port = CreateNodeFlowPort(
								true, guid, node, isInput, vmType );
							port.ReadXml( reader );
							port.OnDeserialize();
						}
						else
						{
							string name = reader.GetAttribute( "Name" );
							Type valueType = Type.GetType( reader.GetAttribute( "ValueType" ) ?? throw new InvalidOperationException());
							bool hasEditor = bool.Parse( reader.GetAttribute( "HasEditor" ) ?? throw new InvalidOperationException());

							NodePropertyPort port = CreateNodePropertyPort(
								true, guid, node, isInput, valueType, null, name, hasEditor, vmType );
							port.ReadXml( reader );
							port.OnDeserialize();
						}

						break;
					}
				}
			}
		}

		#endregion // Serialization

		#region ContextMenu

		public delegate bool BuildContextMenuDelegate( object sender, BuildContextMenuArgs args );
		public static event BuildContextMenuDelegate BuildFlowChartContextMenu;
		public static event BuildContextMenuDelegate BuildNodeContextMenu;
		public static event BuildContextMenuDelegate BuildFlowPortContextMenu;
		public static event BuildContextMenuDelegate BuildPropertyPortContextMenu;

		public static bool InvokeBuildContextMenu( object sender, BuildContextMenuArgs args )
		{
			BuildContextMenuDelegate targetEvent = null;

			switch( args.ModelType )
			{
				case ModelType.FlowChart:
					targetEvent = BuildFlowChartContextMenu;
					break;
				case ModelType.Selecatable:
					targetEvent = BuildNodeContextMenu;
					break;
				case ModelType.FlowPort:
					targetEvent = BuildFlowPortContextMenu;
					break;
				case ModelType.PropertyPort:
					targetEvent = BuildPropertyPortContextMenu;
					break;
			}

			if( null == targetEvent )
				return false;

			return targetEvent.Invoke( sender, args );
		}

		#endregion // ContextMenu

		#region Selection Events

		public delegate void NodeSelectionChangedDelegate( FlowChart flowChart, ObservableCollection<Guid> nodes, NotifyCollectionChangedEventArgs args );
		public static event NodeSelectionChangedDelegate NodeSelectionChanged;

		private static void Node_SelectionList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
		{
			FlowChart flowChart = null;
			foreach( var pair in SelectedGuids )
			{
				if( pair.Value == sender )
				{
					flowChart = FindFlowChart( pair.Key );
				}
			}

			NodeSelectionChanged?.Invoke( flowChart, sender as ObservableCollection<Guid>, args );
		}

		#endregion // Selection Events

		#region Drag & Drop Events

		public delegate void NodeGraphDragEventDelegate( object sender, NodeGraphDragEventArgs args );

		public static event NodeGraphDragEventDelegate DragEnter;
		public static event NodeGraphDragEventDelegate DragLeave;
		public static event NodeGraphDragEventDelegate DragOver;
		public static event NodeGraphDragEventDelegate Drop;

		public static void InvokeDragEnter( object sender, NodeGraphDragEventArgs args )
		{
			DragEnter?.Invoke( sender, args );
		}

		public static void InvokeDragLeave( object sender, NodeGraphDragEventArgs args )
		{
			DragLeave?.Invoke( sender, args );
		}

		public static void InvokeDragOver( object sender, NodeGraphDragEventArgs args )
		{
			DragOver?.Invoke( sender, args );
		}

		public static void InvokeDrop( object sender, NodeGraphDragEventArgs args )
		{
			Drop?.Invoke( sender, args );
		}

		#endregion // Drag & Drop Events

		#region Logs

		public static void AddScreenLog( FlowChart flowChart, string log )
		{
			FlowChartView view = flowChart.ViewModel.View;
			if (view != null)
			{
				view.AddLog(log);
			}
		}

		public static void RemoveScreenLog( FlowChart flowChart, string log )
		{
			FlowChartView view = flowChart.ViewModel.View;
			if (view != null)
			{
				view.RemoveLog(log);
			}
		}

		public static void ClearScreenLogs( FlowChart flowChart )
		{
			FlowChartView view = flowChart.ViewModel.View;
			if (view != null)
			{
				view.ClearLogs();
			}
		}

		#endregion // Logs

		#region Execution

		public static void ClearNodeExecutionStates( FlowChart flowChart )
		{
			foreach( var pair in Nodes )
			{
				Node node = pair.Value;
				if( flowChart == node.Owner )
				{
					node.ExecutionState = ExecutionState.None;
				}
			}
		}

		#endregion // Execution
	}

	public enum ModelType
	{
		FlowChart,
		Selecatable,
		FlowPort,
		PropertyPort,
	}

	public class BuildContextMenuArgs
	{
		public Point ViewSpaceMouseLocation { get; set; }
		public Point ModelSpaceMouseLocation { get; set; }
		public ModelType ModelType { get; set; }
		public System.Windows.Controls.ContextMenu ContextMenu { get; internal set; }
	}

	public class NodeGraphDragEventArgs
	{
		public Point ViewSpaceMouseLocation { get; set; }
		public Point ModelSpaceMouseLocation { get; set; }
		public ModelType ModelType { get; set; }
		public DragEventArgs DragEventArgs { get; set; }
	}
}
