using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NodeGraph.Controls;
using NodeGraph.Converters;
using NodeGraph.Model;
using PropertyTools.Wpf;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_Header", Type = typeof(EditableTextBlock))]
    public class NodePropertyPortView : NodePortView
    {
        #region Fields
        private EditableTextBlock _Part_Header;
        private readonly DispatcherTimer _ClickTimer = new DispatcherTimer();
        private int _ClickCount  ;
        #endregion

        #region Constructors
        #region Constructor
        public NodePropertyPortView(bool isInput) : base(isInput)
        {
            Loaded += NodePropertyPortView_Loaded;
        }
        #endregion // Constructor
        #endregion

        #region Methods
        #region Template Events
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _Part_Header = Template.FindName("PART_Header", this) as EditableTextBlock;
            if (null != _Part_Header)
            {
                _Part_Header.MouseDown += _Part_Header_MouseDown;
                ;
            }
        }
        #endregion // Template Events

        #region Header Events
        private void _Part_Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(_Part_Header);

            if (0 == _ClickCount)
            {
                _ClickTimer.Start();
                _ClickCount++;
            }
            else if (1 == _ClickCount)
            {
                _Part_Header.IsEditing = true;
                Keyboard.Focus(_Part_Header);
                _ClickCount = 0;
                _ClickTimer.Stop();

                e.Handled = true;
            }
        }
        #endregion // Header Events
        #endregion

        #region Properites
        public Visibility PropertyEditorVisibility
        {
            get => (Visibility)GetValue(PropertyEditorVisibilityProperty);
            set => SetValue(PropertyEditorVisibilityProperty, value);
        }
        public static readonly DependencyProperty PropertyEditorVisibilityProperty =
            DependencyProperty.Register("PropertyEditorVisibility", typeof(Visibility), typeof(NodePropertyPortView), new PropertyMetadata(Visibility.Hidden));

        public FrameworkElement PropertyEditor
        {
            get => (FrameworkElement)GetValue(PropertyEditorProperty);
            set => SetValue(PropertyEditorProperty, value);
        }
        public static readonly DependencyProperty PropertyEditorProperty =
            DependencyProperty.Register("PropertyEditor", typeof(FrameworkElement), typeof(NodePropertyPortView), new PropertyMetadata(null));
        #endregion // Properties

        #region Events
        private void NodePropertyPortView_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePropertyEditor();
            SynchronizeProperties();

            _ClickTimer.Interval = TimeSpan.FromMilliseconds(300);
            _ClickTimer.Tick += _ClickTimer_Tick;
            ;
        }

        private void _ClickTimer_Tick(object sender, EventArgs e)
        {
            _ClickCount = 0;
            _ClickTimer.Stop();
        }

        protected override void SynchronizeProperties()
        {
            base.SynchronizeProperties();

            if (IsInput)
            {
                PropertyEditorVisibility = IsFilledPort ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        #endregion // Events

        #region Property Editors
        protected virtual void CreatePropertyEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;
            if (port.HasEditor)
            {
                var type = port.ValueType;

                if (typeof(bool) == type)
                {
                    PropertyEditor = CreateBoolEditor();
                }
                else if (typeof(bool?) == type)
                {
                    PropertyEditor = CreateNullableBoolEditor();
                }
                else if (typeof(string) == type)
                {
                    PropertyEditor = CreateStringEditor();
                }
                else if (typeof(byte) == type)
                {
                    PropertyEditor = CreateByteEditor();
                }
                else if (typeof(short) == type)
                {
                    PropertyEditor = CreateShortEditor();
                }
                else if (typeof(int) == type)
                {
                    PropertyEditor = CreateIntEditor();
                }
                else if (typeof(long) == type)
                {
                    PropertyEditor = CreateLongEditor();
                }
                else if (typeof(float) == type)
                {
                    PropertyEditor = CreateFloatEditor();
                }
                else if (typeof(double) == type)
                {
                    PropertyEditor = CreateDoubleEditor();
                }
                else if (type.IsEnum)
                {
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), true).Length == 0)
                    {
                        PropertyEditor = CreateEnumEditor();
                    }
                    else
                    {
                        PropertyEditor = CreateEnumFlagsEditor();
                    }
                }
                else if (typeof(Color) == type)
                {
                    PropertyEditor = CreateColorEditor();
                }

                if (null != PropertyEditor)
                {
                    PropertyEditorVisibility = Visibility.Visible;
                }
            }
        }

        public FrameworkElement CreateEnumEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var array = Enum.GetValues(port.ValueType);
            var selectedIndex = -1;
            for (var i = 0; i < array.Length; ++i)
            {
                if (port.Value.ToString() == array.GetValue(i).ToString())
                {
                    selectedIndex = i;
                    break;
                }
            }

            var comboBox = new ComboBox();
            comboBox.SelectionChanged += EnumComboBox_SelectionChanged;
            comboBox.ItemsSource = array;
            comboBox.SelectedIndex = selectedIndex;
            return comboBox;
        }

        private class EnumFlagsConverter : IValueConverter
        {
            #region IValueConverter Members
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return $@"[{value}]";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            #endregion
        }

        public FrameworkElement CreateEnumFlagsEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;
            var array = Enum.GetValues(port.ValueType);
            var expander = new Expander();
            var panel = new StackPanel();
            expander.Content = panel;
            expander.Foreground = new SolidColorBrush(Colors.White);
            expander.SetBinding(HeaderedContentControl.HeaderProperty, CreateBinding(port, "Value", new EnumFlagsConverter()));
            for (var i = 0; i < array.Length; i++)
            {
                var checkbox = new CheckBoxForEnumWithFlagAttribute();
                checkbox.Content = array.GetValue(i).ToString();
                checkbox.EnumFlag = array.GetValue(i);
                checkbox.Foreground = new SolidColorBrush(Colors.White);
                checkbox.SetBinding(CheckBoxForEnumWithFlagAttribute.EnumValueProperty, CreateBinding(port, "Value", null));
                panel.Children.Add(checkbox);
            }
            return expander;
        }

        private void EnumComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var port = ViewModel.Model as NodePropertyPort;

            var comboBox = PropertyEditor as ComboBox;
            if (null != comboBox)
            {
                port.Value = comboBox.SelectedItem;
            }
        }

        public FrameworkElement CreateBoolEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var checkBox = new CheckBox();
            checkBox.IsChecked = (bool)port.Value;
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, CreateBinding(port, "Value", null));
            return checkBox;
        }

        public FrameworkElement CreateNullableBoolEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var checkBox = new CheckBox();
            checkBox.IsThreeState = true;
            checkBox.IsChecked = (bool?)port.Value;
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, CreateBinding(port, "Value", null));
            return checkBox;
        }

        public FrameworkElement CreateStringEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new TextBoxEx();
            textBox.Text = port.Value?.ToString() ?? string.Empty;
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", null));
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.MinWidth = 50;
            return textBox;
        }

        public FrameworkElement CreateByteEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = true;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new ByteToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateShortEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = true;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new ShortToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateIntEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = true;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new IntToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateLongEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = true;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new LongToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateFloatEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = false;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new FloatToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateDoubleEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var textBox = new NumericTextBox();
            textBox.IsInteger = false;
            textBox.Text = port.Value.ToString();
            textBox.SetBinding(TextBox.TextProperty, CreateBinding(port, "Value", new DoubleToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateColorEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var picker = new ColorPicker();
            picker.SelectedColor = (Color)port.Value;
            picker.SetBinding(ColorPicker.SelectedColorProperty, CreateBinding(port, "Value", null));
            return picker;
        }

        public Binding CreateBinding(NodePropertyPort port, string propertyName, IValueConverter converter)
        {
            var binding = new Binding(propertyName)
            {
                Source = port,
                Mode = BindingMode.TwoWay,
                Converter = converter,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true
            };

            return binding;
        }
        #endregion // Property Editors.
    }
}