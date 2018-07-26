// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;

using BuildAzure.IoT.Adafruit.BME280;

using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.ApplicationModel;

namespace SmartDevice
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer measureTimer;

        // temperature sensor
        private BME280Sensor bme280Sensor = new BME280Sensor();

        private WebApiClient webApiClient = new WebApiClient();

        //const float seaLevelPressure = 1013.25f;  //hectopascal
        const float seaLevelPressure =1018.50f;

        public MainPage()
        {
            InitializeComponent();
            Application.Current.Suspending += Current_Suspending;
            // set up timer to sample proximity and temperature values every 1000 milliseconds
            measureTimer = new DispatcherTimer();
            measureTimer.Interval = TimeSpan.FromMilliseconds(1000);
            // attach event handler on each 1000ms tick
            measureTimer.Tick += MeasureTimerTick;

            // initialize GPIO devices
            InitializeDevices();
        }

        private  void Current_Suspending(object sender, SuspendingEventArgs e)
        {
             webApiClient.DeleteDevice();
        }


        #region Methods
        private async void InitializeDevices()
        {
            #region Old
            //var gpio = GpioController.GetDefault();
            //// show an error if there is no GPIO controller
            //if (gpio is null)
            //{
            //    //interruptPin = null;
            //    GpioStatus.Text = "There is no GPIO controller on this device.";
            //    return;
            //}
            // set up interrupt pin for proximity sensor
            //interruptPin = gpio.OpenPin(intPinNumber);

            //// pull up interrupt pin as sensor will pull down to notify
            //interruptPin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            // initialize BME280 Sensor + Intitialize VNCL4010
            //await Task.WhenAll(bme280Sensor.Initialize(), vncl4010Sensor.Initialize()); //hhe
            #endregion Old

            await Task.WhenAll(bme280Sensor.Initialize());

            GpioStatus.Text = "Connecting to WebAPI...";

            //await IoTHub.ConnectToIoTHub();  //hhe
            //RunAsync().GetAwaiter().GetResult();
            await webApiClient.PostDevice();

            GpioStatus.Text = "";

            // start measuring temperature and proximity 
            measureTimer.Start();
        }
        #endregion Methods

        #region Handlers
        private async void MeasureTimerTick(object sender, object e)
        {
            // read Temperature
            double temperature = await bme280Sensor.ReadTemperature();
            double humidity = await bme280Sensor.ReadHumidity();
            double pressure = await bme280Sensor.ReadPressure();
            float altitude = await bme280Sensor.ReadAltitude(seaLevelPressure);
            // convert to Fahrenheit
            //double fahrenheitTemperature = temperature * 1.8 + 32.0; // hhe

            // read Proximity
            //int proximity = vncl4010Sensor.ReadProximity(); //hhe

            //TemperatureStatus.Text = "The temperature is currently " + fahrenheitTemperature.ToString("n1") + "°F"; //hhe
            Location.Text = webApiClient.device.Location;
            DeviceName.Text = webApiClient.device.Name;
            X.Text = webApiClient.device.X;
            Y.Text = webApiClient.device.Y;
            Z.Text = webApiClient.device.Z;

            TemperatureStatus.Text = "The temperature is currently " + temperature.ToString("n1") + "°C";
            HumidityStatus.Text = "The humidity is currently " + humidity.ToString("n1") + "%";
            PressureStatus.Text = "The pressure is currently " + pressure.ToString("n1") + "";
            Altitude.Text = "The altitude is currently " + altitude.ToString("n1") + " meters";

            await webApiClient.PutDevice(X.Text, Y.Text, Z.Text, temperature,humidity,pressure);
        }
        #endregion
    }
}

