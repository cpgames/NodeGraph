using System;
using System.Xml;
using NodeGraph.ViewModel;

namespace NodeGraph.Model
{
    public class Router : ModelBase
    {
        #region Fields
        protected double _x;
        protected double _y;
        public readonly Connector owner;
        #endregion

        #region Properties
        protected RouterViewModel _viewModel;
        public RouterViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (value != _viewModel)
                {
                    _viewModel = value;
                    RaisePropertyChanged("ViewModel");
                }
            }
        }
        public double X
        {
            get => _x;
            set
            {
                if (value != _x)
                {
                    _x = value;
                    RaisePropertyChanged("X");
                }
            }
        }
        public double Y
        {
            get => _y;
            set
            {
                if (value != _y)
                {
                    _y = value;
                    RaisePropertyChanged("Y");
                }
            }
        }
        #endregion

        #region Constructors
        public Router(Guid guid, Connector connector) : base(guid)
        {
            owner = connector;
        }
        #endregion

        #region Methods
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("ViewModelType", ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Owner", owner.Guid.ToString());
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            X = double.Parse(reader.GetAttribute("X"));
            Y = double.Parse(reader.GetAttribute("Y"));
        }
        #endregion
    }
}