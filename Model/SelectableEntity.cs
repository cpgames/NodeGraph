using System.Xml;

namespace NodeGraph.Model
{
    public abstract class SelectableEntity : ModelBase
    {
        #region Fields
        protected double _x;
        protected double _y;
        protected int _zIndex;
        public readonly FlowChart Owner;
        #endregion

        #region Properties
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
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (value != _zIndex)
                {
                    _zIndex = value;
                    RaisePropertyChanged("ZIndex");
                }
            }
        }
        #endregion

        #region Methods
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("Owner", Owner.Guid.ToString());
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("ZIndex", ZIndex.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            X = double.Parse(reader.GetAttribute("X") ?? string.Empty);
            Y = double.Parse(reader.GetAttribute("Y") ?? string.Empty);
            ZIndex = int.Parse(reader.GetAttribute("ZIndex") ?? string.Empty);
        }
        #endregion
    }
}