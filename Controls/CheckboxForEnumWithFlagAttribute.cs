using System;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.Controls
{
    // <summary>
    // Usage: Bind EnumFlag and Two way binding on EnumValue instead of IsChecked
    // Example: <myControl:CheckBoxForEnumWithFlagAttribute
    //                 EnumValue = "{Binding SimulationNatureTypeToCreateStatsCacheAtEndOfSimulation, Mode=TwoWay}"
    //                 EnumFlag = "{x:Static Core:SimulationNatureType.LoadFlow }">Load Flow results</myControl:CheckBoxForEnumWithFlagAttribute>
    // </summary>
    public class CheckBoxForEnumWithFlagAttribute : CheckBox
    {
        #region Fields
        public static DependencyProperty EnumValueProperty =
            DependencyProperty.Register("EnumValue", typeof(object), typeof(CheckBoxForEnumWithFlagAttribute), new PropertyMetadata(EnumValueChangedCallback));

        public static DependencyProperty EnumFlagProperty =
            DependencyProperty.Register("EnumFlag", typeof(object), typeof(CheckBoxForEnumWithFlagAttribute), new PropertyMetadata(EnumFlagChangedCallback));
        #endregion

        #region Properties
        public object EnumValue
        {
            get => GetValue(EnumValueProperty);
            set => SetValue(EnumValueProperty, value);
        }

        public object EnumFlag
        {
            get => GetValue(EnumFlagProperty);
            set => SetValue(EnumFlagProperty, value);
        }
        #endregion

        #region Constructors
        public CheckBoxForEnumWithFlagAttribute()
        {
            Checked += CheckBoxForEnumWithFlag_Checked;
            Unchecked += CheckBoxForEnumWithFlag_Unchecked;
        }
        #endregion

        #region Methods
        private static void EnumValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var checkBoxForEnumWithFlag = dependencyObject as CheckBoxForEnumWithFlagAttribute;
            if (checkBoxForEnumWithFlag != null)
            {
                checkBoxForEnumWithFlag.RefreshCheckBoxState();
            }
        }

        private static void EnumFlagChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var checkBoxForEnumWithFlag = dependencyObject as CheckBoxForEnumWithFlagAttribute;
            if (checkBoxForEnumWithFlag != null)
            {
                checkBoxForEnumWithFlag.RefreshCheckBoxState();
            }
        }

        private void RefreshCheckBoxState()
        {
            if (EnumValue != null)
            {
                if (EnumValue is Enum)
                {
                    var underlyingType = Enum.GetUnderlyingType(EnumValue.GetType());
                    dynamic valueAsInt = Convert.ChangeType(EnumValue, underlyingType);
                    dynamic flagAsInt = Convert.ChangeType(EnumFlag, underlyingType);

                    IsChecked = (valueAsInt & flagAsInt) > 0;
                }
            }
        }

        private void CheckBoxForEnumWithFlag_Checked(object sender, RoutedEventArgs e)
        {
            RefreshEnumValue();
        }

        private void CheckBoxForEnumWithFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshEnumValue();
        }

        private void RefreshEnumValue()
        {
            if (EnumValue != null)
            {
                if (EnumValue is Enum)
                {
                    var underlyingType = Enum.GetUnderlyingType(EnumValue.GetType());
                    dynamic valueAsInt = Convert.ChangeType(EnumValue, underlyingType);
                    dynamic flagAsInt = Convert.ChangeType(EnumFlag, underlyingType);

                    var newValueAsInt = valueAsInt;
                    if (IsChecked == true)
                    {
                        newValueAsInt = valueAsInt | flagAsInt;
                    }
                    else
                    {
                        newValueAsInt = valueAsInt & ~flagAsInt;
                    }

                    if (newValueAsInt != valueAsInt)
                    {
                        object o = Enum.ToObject(EnumValue.GetType(), newValueAsInt);

                        EnumValue = o;
                    }
                }
            }
        }
        #endregion
    }
}