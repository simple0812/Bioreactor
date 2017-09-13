using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;
using Shunxi.App.CellMachine.Common.DevExpress.Mvvm.Native;

namespace Shunxi.App.CellMachine.Common.Behaviors
{
    public class EnumItemsSourceBehavior : Behavior<ItemsControl>
    {
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(EnumItemsSourceBehavior),
                new PropertyMetadata(null));

        public string EnumType
        {
            get { return (string)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }

        public string EnumAssembly
        {
            get { return (string)GetValue(EnumAssemblyAProperty); }
            set { SetValue(EnumAssemblyAProperty, value); }
        }

        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(string), typeof(EnumItemsSourceBehavior),
                new PropertyMetadata(null, (s, e) => ((EnumItemsSourceBehavior)s).Update()));

        public static readonly DependencyProperty EnumAssemblyAProperty =
            DependencyProperty.Register("EnumAssembly", typeof(string), typeof(EnumItemsSourceBehavior),
                new PropertyMetadata(null, (s, e) => ((EnumItemsSourceBehavior)s).Update()));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        protected Selector Selector
        {
            get { return AssociatedObject as Selector; }
        }

        void Update()
        {
            if (AssociatedObject == null)
                return;
            Type enumType = GetEnumType(EnumType);
            if (enumType == null)
                return;
            object seletedItem = Selector.With(x => x.SelectedItem);
            var itemSource = GetEnumSource(enumType);
            AssociatedObject.ItemsSource = itemSource;
            AssociatedObject.DisplayMemberPath = "Name";
            if (Selector == null)
                return;
            var converter = new EnumMemberInfoConverter(enumType, itemSource);
            Selector.SelectedValuePath = "Name";
            Selector.SelectedItem = seletedItem ?? converter.Convert(SelectedItem, null, null, null);
            BindingOperations.SetBinding(Selector, Selector.SelectedItemProperty,
                new Binding()
                {
                    Path = new PropertyPath("SelectedItem"),
                    Source = this,
                    Converter = converter,
                    Mode = BindingMode.TwoWay
                });
        }

        Type GetEnumType(string enumType)
        {
            if (string.IsNullOrEmpty(EnumAssembly))
            {
                return Type.GetType(enumType);
            }

            AssemblyName name = new AssemblyName(EnumAssembly);
            var result = Assembly.Load(name);
            return result.GetType(enumType);
        }


        IEnumerable<EnumMemberInfo> GetEnumSource(Type enumType)
        {
            var result = new List<EnumMemberInfo>();
            foreach (var item in Enum.GetValues(enumType))
            {
                result.Add(new EnumMemberInfo(GetEnumName(item), item));
            }
            return result;
        }

        string GetEnumName(object item)
        {
            string name = item.ToString();
            return name;
        }

        public class EnumMemberInfoConverter : IValueConverter
        {
            Type enumType;
            IEnumerable<EnumMemberInfo> itemSource;

            public EnumMemberInfoConverter(Type enumType, IEnumerable<EnumMemberInfo> itemSource)
            {
                this.enumType = enumType;
                this.itemSource = itemSource;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo language)
            {
                if (value == null || value.GetType() != enumType)
                    return null;
                return new EnumMemberInfo(value.ToString(), value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
            {
                var a = (value as EnumMemberInfo).With(x => x.Id);
                return a;
            }
        }
    }

    public class EnumMemberInfo
    {
        public EnumMemberInfo(string value, object id)
        {
            Id = id;
            Name = value;
        }

        public object Id { get; private set; }
        public string Name { get; private set; }

        public override bool Equals(object obj)
        {
            return (obj as EnumMemberInfo).If(x => x.Id.Equals(Id)).ReturnSuccess();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
