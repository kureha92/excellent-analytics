using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Analytics.Data.Enums;
using System.Windows.Media;
using System.Windows;

namespace UI.Controls
{
    public class SizeViewModel : INotifyPropertyChanged
    {
        #region Fields

        bool? _isChecked = false;
        bool? _isEnabled = true;
        SizeViewModel _parent;
        int maxSelections;

        #endregion 

        public static List<SizeViewModel> CreateDimensions()
        {
            return LoadXML(SizeKeyType.Dimension);
        }

        public static List<SizeViewModel> CreateMetrics()
        {
            return LoadXML(SizeKeyType.Metric);
        }

        private static List<SizeViewModel> LoadXML(SizeKeyType feedObjectType)
        {
            SizeViewModel sizes = new SizeViewModel(feedObjectType == SizeKeyType.Dimension ? "Dimensions" : "Metrics");
            XDocument xDocument = Analytics.Data.Query.GetSizeCollectionAsXML(feedObjectType);
            foreach (XElement element in xDocument.Root.Elements("Category"))
            {
                SizeViewModel category = new SizeViewModel(element.Attribute("name").Value);

                foreach (XElement subElement in element.Elements(feedObjectType == SizeKeyType.Dimension ? "Dimension" : "Metric"))
                {
                    string paramValue = subElement.Attribute("value").Value;
                    SizeViewModel size = new SizeViewModel(subElement.Attribute("name").Value , paramValue);
                 /*   if (size.Name.Contains("hour"))
                    {
                        size.IsChecked = true;
                    }*/
                    category.Children.Add(size);
                }
                category.IsInitiallyExpanded = false;
                
                sizes.Children.Add(category);
            }
            sizes.IsInitiallyExpanded = true;
            sizes.Initialize();
            return new List<SizeViewModel> { sizes };
        }

        SizeViewModel(string name)
        {
            this.Name = name;
            this.Children = new List<SizeViewModel>();
        }

        SizeViewModel(string name, string value)
        {
            this.Value = value;
            this.Name = name;
            this.Children = new List<SizeViewModel>();
        }

        void Initialize()
        {
            foreach (SizeViewModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        

        #region Properties

        public List<SizeViewModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public bool IsInitiallyExpanded { get; private set; }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public string Category { get; private set; }

        public int MaxSelections
        {
            get { return maxSelections; }
            set { maxSelections = value; }
        }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child SizeViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }



        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;

                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #region IsEnabled

        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { this.SetIsEnabled(value, true, true); }
        }

        void SetIsEnabled(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isEnabled)
                return;

            _isEnabled = value;

            if (updateChildren && _isEnabled.HasValue)
                this.Children.ForEach(c => c.SetIsEnabled(_isEnabled, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyEnableState();

            this.OnPropertyChanged("IsEnabled");
        }

        void VerifyEnableState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsEnabled;

                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsEnabled(state, false, true);
        }

        #endregion //IsEnabled

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}