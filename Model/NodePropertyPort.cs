using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
    public class NodePropertyPort : NodePort
    {
        #region Nested type: DynamicPropertyPortValueChangedDelegate
        public delegate void DynamicPropertyPortValueChangedDelegate(NodePropertyPort port, object prevValue, object newValue);
        #endregion

        #region Fields
        public readonly bool SerializeValue;
        public readonly bool IsDynamic;
        public readonly bool HasEditor;
        protected FieldInfo _FieldInfo;
        protected PropertyInfo _PropertyInfo;

        public object _Value;
        #endregion

        #region Properties
        public object Value
        {
            get
            {
                if (IsDynamic)
                {
                    return _Value;
                }
                return null != _FieldInfo ? _FieldInfo.GetValue(Owner) : _PropertyInfo.GetValue(Owner);
            }
            set
            {
                object prevValue;
                if (IsDynamic)
                {
                    prevValue = _Value;
                }
                else
                {
                    prevValue = null != _FieldInfo ? _FieldInfo.GetValue(Owner) : _PropertyInfo.GetValue(Owner);
                }

                if (value != prevValue)
                {
                    if (IsDynamic)
                    {
                        _Value = value;
                        OnDynamicPropertyPortValueChanged(prevValue, value);
                    }
                    else
                    {
                        if (null != _FieldInfo)
                        {
                            _FieldInfo.SetValue(Owner, value);
                        }
                        else if (null != _PropertyInfo)
                        {
                            _PropertyInfo.SetValue(Owner, value);
                        }
                    }

                    RaisePropertyChanged("Value");
                }
            }
        }

        public Type ValueType
        {
            get;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNodePropertyPort() method.
        /// </summary>
        public NodePropertyPort(Guid guid, Node node, bool isInput, Type valueType, object value, string name, bool hasEditor, bool serializeValue) :
            base(guid, node, isInput)
        {
            Name = name;
            HasEditor = hasEditor;
            SerializeValue = serializeValue;

            var nodeType = node.GetType();
            _FieldInfo = nodeType.GetField(Name);
            _PropertyInfo = nodeType.GetProperty(Name);
            IsDynamic = null == _FieldInfo && null == _PropertyInfo;

            ValueType = valueType;
            Value = value;
        }
        #endregion

        #region Methods
        public void CheckValidity()
        {
            if (null != Value)
            {
                if (!ValueType.IsAssignableFrom(Value.GetType()))
                {
                    throw new ArgumentException("Type of value is not same as typeOfvalue.");
                }
            }

            if (ValueType == null || !ValueType.IsClass && !ValueType.IsInterface && null == Value &&
                Nullable.GetUnderlyingType(ValueType) == null)
            {
                throw new ArgumentNullException("If typeOfValue is not a class, you cannot specify value as null");
            }

            if (!IsDynamic)
            {
                var nodeType = Owner.GetType();
                _FieldInfo = nodeType.GetField(Name);
                _PropertyInfo = nodeType.GetProperty(Name);
                Type propType;
                if (_FieldInfo != null)
                {
                    var value = _FieldInfo.GetValue(Owner);
                    if (value != null)
                    {
                        propType = value.GetType();
                    }
                    else
                    {
                        propType = _FieldInfo.FieldType;
                    }
                }
                else
                {
                    var value = _PropertyInfo.GetValue(Owner);
                    if (value != null)
                    {
                        propType = value.GetType();
                    }
                    else
                    {
                        propType = _PropertyInfo.PropertyType;
                    }
                }
                if (!ValueType.IsAssignableFrom(propType))
                {
                    throw new ArgumentException(string.Format("ValueType( {0} ) is invalid, because a type of property or field is {1}.",
                        ValueType.Name, propType.Name));
                }
            }
        }

        public event DynamicPropertyPortValueChangedDelegate DynamicPropertyPortValueChanged;

        protected virtual void OnDynamicPropertyPortValueChanged(object prevValue, object newValue)
        {
            DynamicPropertyPortValueChanged?.Invoke(this, prevValue, newValue);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            if (ValueType != null)
            {
                writer.WriteAttributeString("ValueType", ValueType.AssemblyQualifiedName);
            }
            writer.WriteAttributeString("HasEditor", HasEditor.ToString());
            writer.WriteAttributeString("SerializeValue", SerializeValue.ToString());

            var realValueType = Value != null ? Value.GetType() : ValueType;
            writer.WriteAttributeString("RealValueType", realValueType.AssemblyQualifiedName);

            if (SerializeValue)
            {
                var serializer = new XmlSerializer(realValueType);
                serializer.Serialize(writer, Value);
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            if (SerializeValue)
            {
                var realValueType = Type.GetType(reader.GetAttribute("RealValueType"));

                while (reader.Read())
                {
                    if (XmlNodeType.Element == reader.NodeType)
                    {
                        var serializer = new XmlSerializer(realValueType);
                        Value = serializer.Deserialize(reader);
                        break;
                    }
                }
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CheckValidity();
        }

        public override void OnDeserialize()
        {
            base.OnDeserialize();

            CheckValidity();
        }
        #endregion
    }
}