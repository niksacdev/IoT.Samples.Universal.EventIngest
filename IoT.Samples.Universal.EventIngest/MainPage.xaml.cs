using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Media3D;
using IoT.Samples.EventIngest.Entity;
using IoT.Samples.Universal.EventIngest.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoT.Samples.Universal.EventIngest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly ConnectionManager _connectionManager;
        public MainPage()
        {
            //TODO: remove hardcoding of configuration data
            this.InitializeComponent();

            // Populate the FlipView control
            InitializeFlipView();

            // Configure EVent Hub and other settings
            // var connectionString = @"Endpoint=iotboothncm.servicebus.windows.netsb:///;SharedAccessKeyName=manage;SharedAccessKey=UtR+9AnafOGvqC/bxvMH2ndpHIAOYb9rvPWPBzpQdbI=";
            const string sbNamespace = "iotboothncm";
            const string ehName = "iotboothncmeh";
            const string ehkeyName = "sender";
            const string ehkey = "6lnCam/z9REMjLI1OBFnmqkFL+T5YH0/MV0IgsvCzQA=";

            // Create connection
            this._connectionManager = new ConnectionManager(sbNamespace, ehName, ehkeyName, ehkey);
        }

        private async void flipView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // try to cast source as content presenter
            var content = e.OriginalSource as ContentPresenter;

            if (content == null) return;
            
            // Send data to Event Hub
            var eventData = new Event
            {
                Id = "iotboothdevice",
                Timecreated = DateTime.UtcNow.ToString("mm:dd:yyyy hh:mm"),
                Value = content.Content.ToString()
            };

            var result = await _connectionManager.SendEvent(eventData); // send message over event hub
            if (!result) return;
            var dialog = new MessageDialog("Thanks for visiting the IoT booth!");
            await dialog.ShowAsync();
            InitializeFlipView();
        }

        private void InitializeFlipView()
        {
            //TODO: build list from custom json
            var items = new List<FlipViewItem>
            {
                new FlipViewItem { Name="1", Content ="MCS", IsTextScaleFactorEnabled = true, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center},
                new FlipViewItem { Name="2", Content ="EPG"},
                new FlipViewItem { Name="3", Content ="Premier"},
                new FlipViewItem { Name="3", Content ="Others"},
            };

            this.flipView.ItemsSource = items;
            flipView.UseTouchAnimationsForAllNavigation = true;
            flipView.IsTextScaleFactorEnabled = true;
            flipView.Transform3D = new PerspectiveTransform3D();
        }
    }
}
