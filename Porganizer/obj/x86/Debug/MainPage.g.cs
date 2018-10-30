﻿#pragma checksum "C:\Users\user\Desktop\PROJECT UWP\Porganizer\Porganizer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "85C94BC3CAE04E9E15F420080B522380"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Porganizer
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_Image_Source(global::Windows.UI.Xaml.Controls.Image obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.Source = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class MainPage_obj12_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IMainPage_Bindings
        {
            private global::Porganizer.VideoFile dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj12;
            private global::Windows.UI.Xaml.Controls.Image obj13;
            private global::Windows.UI.Xaml.Controls.TextBlock obj14;

            public MainPage_obj12_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 12: // MainPage.xaml line 67
                        this.obj12 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.Grid)target);
                        break;
                    case 13: // MainPage.xaml line 76
                        this.obj13 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 14: // MainPage.xaml line 77
                        this.obj14 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj12.Target as global::Windows.UI.Xaml.Controls.Grid).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::Porganizer.VideoFile) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // IMainPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Porganizer.VideoFile)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Porganizer.VideoFile obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Thumbnail(obj.Thumbnail, phase);
                        this.Update_File(obj.File, phase);
                    }
                }
            }
            private void Update_Thumbnail(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // MainPage.xaml line 76
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj13, obj, null);
                }
            }
            private void Update_File(global::Windows.Storage.StorageFile obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_File_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_File_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // MainPage.xaml line 77
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj14, obj, null);
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class MainPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IMainPage_Bindings
        {
            private global::Porganizer.MainPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.ListView obj11;

            private MainPage_obj1_BindingsTracking bindingsTracking;

            public MainPage_obj1_Bindings()
            {
                this.bindingsTracking = new MainPage_obj1_BindingsTracking(this);
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 11: // MainPage.xaml line 64
                        this.obj11 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        break;
                    default:
                        break;
                }
            }

            // IMainPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Porganizer.MainPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Porganizer.MainPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_thumbFileList(obj.thumbFileList, phase);
                    }
                }
            }
            private void Update_thumbFileList(global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile> obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_thumbFileList(obj);
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // MainPage.xaml line 64
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj11, obj, null);
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class MainPage_obj1_BindingsTracking
            {
                private global::System.WeakReference<MainPage_obj1_Bindings> weakRefToBindingObj; 

                public MainPage_obj1_BindingsTracking(MainPage_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<MainPage_obj1_Bindings>(obj);
                }

                public MainPage_obj1_Bindings TryGetBindingObject()
                {
                    MainPage_obj1_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_thumbFileList(null);
                }

                public void PropertyChanged_thumbFileList(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    MainPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile>;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                        }
                        else
                        {
                            switch (propName)
                            {
                                default:
                                    break;
                            }
                        }
                    }
                }
                public void CollectionChanged_thumbFileList(object sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    MainPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile>;
                    }
                }
                private global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile> cache_thumbFileList = null;
                public void UpdateChildListeners_thumbFileList(global::System.Collections.ObjectModel.ObservableCollection<global::Porganizer.VideoFile> obj)
                {
                    if (obj != cache_thumbFileList)
                    {
                        if (cache_thumbFileList != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_thumbFileList).PropertyChanged -= PropertyChanged_thumbFileList;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)cache_thumbFileList).CollectionChanged -= CollectionChanged_thumbFileList;
                            cache_thumbFileList = null;
                        }
                        if (obj != null)
                        {
                            cache_thumbFileList = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_thumbFileList;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)obj).CollectionChanged += CollectionChanged_thumbFileList;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // MainPage.xaml line 36
                {
                    this.TextFileName = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3: // MainPage.xaml line 37
                {
                    this.bitmap = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 4: // MainPage.xaml line 58
                {
                    this.screenshot = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 5: // MainPage.xaml line 51
                {
                    this.TextFileSeries = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 6: // MainPage.xaml line 52
                {
                    this.TextFileActress = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 7: // MainPage.xaml line 53
                {
                    this.TextFileTitle = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 8: // MainPage.xaml line 54
                {
                    this.TextFileSize = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9: // MainPage.xaml line 55
                {
                    this.TextFileLength = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10: // MainPage.xaml line 63
                {
                    this.mainGrid1 = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 11: // MainPage.xaml line 64
                {
                    this.listView1 = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    ((global::Windows.UI.Xaml.Controls.ListView)this.listView1).DoubleTapped += this.LaunchVideo;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.listView1).SelectionChanged += this.SelectionChanged;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.listView1).RightTapped += this.ListView_RightTapped;
                }
                break;
            case 15: // MainPage.xaml line 82
                {
                    this.videoMenuFlyout = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                }
                break;
            case 16: // MainPage.xaml line 84
                {
                    this.Play = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.Play).Click += this.Play_Clicked;
                }
                break;
            case 17: // MainPage.xaml line 85
                {
                    this.OpenFileLocation = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.OpenFileLocation).Click += this.OpenFileLocation_Clicked;
                }
                break;
            case 18: // MainPage.xaml line 19
                {
                    global::Windows.UI.Xaml.Controls.AppBarButton element18 = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)element18).Click += this.AddFolder;
                }
                break;
            case 19: // MainPage.xaml line 23
                {
                    this.Progress = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 20: // MainPage.xaml line 25
                {
                    this.TextFileNum = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 21: // MainPage.xaml line 27
                {
                    this.StatusText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // MainPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    MainPage_obj1_Bindings bindings = new MainPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            case 12: // MainPage.xaml line 67
                {                    
                    global::Windows.UI.Xaml.Controls.Grid element12 = (global::Windows.UI.Xaml.Controls.Grid)target;
                    MainPage_obj12_Bindings bindings = new MainPage_obj12_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element12.DataContext);
                    element12.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element12, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element12, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

