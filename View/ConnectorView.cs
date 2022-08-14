using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeGraph.View
{
	public class ConnectorView : ContentControl
	{
        private Curve _curve = new Curve();

        #region Properties

        public ConnectorViewModel ViewModel { get; private set; }

		public string CurveData
		{
			get { return ( string )GetValue( CurveDataProperty ); }
			set { SetValue( CurveDataProperty, value ); }
		}
		public static readonly DependencyProperty CurveDataProperty =
			DependencyProperty.Register( "CurveData", typeof( string ), typeof( ConnectorView ), new PropertyMetadata( "" ) );

		#endregion // Properties
		
		#region Constructor

		public ConnectorView()
		{
			LayoutUpdated += ConnectorView_LayoutUpdated;
			DataContextChanged += ConnectorView_DataContextChanged;
			Loaded += ConnectorView_Loaded;
		}

		#endregion // Constructor

		#region Events

		private void ConnectorView_Loaded( object sender, RoutedEventArgs e )
		{
			SynchronizeProperties();
		}

		private void ConnectorView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			ViewModel = DataContext as ConnectorViewModel;
			if( null == ViewModel )
				throw new Exception( "ViewModel must be bound as DataContext in ConnectorView." );
			ViewModel.View = this;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			SynchronizeProperties();
		}

		private void ConnectorView_LayoutUpdated( object sender, EventArgs e )
		{
			FlowChart flowChart = ViewModel.Model.FlowChart;
			FlowChartView flowChartView = flowChart.ViewModel.View;
			BuildCurveData( Mouse.GetPosition( flowChartView ) );
		}

		protected virtual void SynchronizeProperties()
		{
			if( null == ViewModel )
			{
				return;
			}
		}
		
		protected virtual void ViewModelPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			SynchronizeProperties();
		}

		#endregion // Events

		#region Curve

		public void BuildCurveData( Point mousePos )
		{
			Connector connector = ViewModel.Model;
			FlowChart flowChart = connector.FlowChart;
			FlowChartView flowChartView = flowChart.ViewModel.View;

			NodePort startPort = connector.StartPort;
			NodePort endPort = connector.EndPort;

			Point start = ( null != startPort ) ? ViewUtil.GetRelativeCenterLocation( startPort.ViewModel.View.PartPort, flowChartView ) : mousePos;
			Point end = ( null != endPort ) ? ViewUtil.GetRelativeCenterLocation( endPort.ViewModel.View.PartPort, flowChartView ) : mousePos;
			
            var points = new List<Point>();
            foreach (var point in connector.Points)
            {
                var pRelative = flowChartView.ZoomAndPan.Matrix.Transform(point);
                points.Add(pRelative);
            }
            _curve = CurveBuilder.BuildCurve(start, end, points);
            CurveData = _curve.ToString();
        }

		#endregion // Curve

		#region Mouse Events

		protected override void OnMouseEnter( MouseEventArgs e )
		{
			base.OnMouseEnter( e );

			Connector connector = ViewModel.Model;

			if( null != connector.StartPort )
			{
				NodePortView portView = connector.StartPort.ViewModel.View;
				portView.IsConnectorMouseOver = true;
			}

			if( null != connector.EndPort )
			{
				NodePortView portView = connector.EndPort.ViewModel.View;
				portView.IsConnectorMouseOver = true;
			}
		}

		protected override void OnMouseLeave( MouseEventArgs e )
		{
			base.OnMouseLeave( e );

			Connector connector = ViewModel.Model;

			if( null != connector.StartPort )
			{
				NodePortView portView = connector.StartPort.ViewModel.View;
				portView.IsConnectorMouseOver = false;
			}

			if( null != connector.EndPort )
			{
				NodePortView portView = connector.EndPort.ViewModel.View;
				portView.IsConnectorMouseOver = false;
			}
		}

		protected override void OnMouseDoubleClick( MouseButtonEventArgs e )
		{
			base.OnMouseDoubleClick( e );

			if( MouseButton.Left == e.ChangedButton )
			{
				Connector connector = ViewModel.Model;
				FlowChart flowChart = connector.FlowChart;
				FlowChartView flowChartView = flowChart.ViewModel.View;
				Point vsMousePos = e.GetPosition( flowChartView );
                connector.Points.Add(vsMousePos);
				BuildCurveData(vsMousePos);
				//flowChart.History.BeginTransaction( "Creating RouterNode" );
				//{
				//	NodeGraphManager.CreateRouterNodeForConnector( Guid.NewGuid(), flowChart, connector,
				//		nodePos.X, nodePos.Y, 0 );
				//}
				//flowChart.History.EndTransaction( false );
			}

			e.Handled = true;
		}

		#endregion // Mouse Events
	}
}
