// needs nuget webAPI client for .net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SmartDevice
{
    public class Device
    {
        public string Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string Type { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string Pressure { get; set; }
    }

    public class WebApiClient
    {
        private string UrlDevice_PathAndQuery = "";
        private string DeviceId = "";
        private HttpClient client = new HttpClient();

        public Device device;

        // Called initially when program starts
        public async Task PostDevice()
        {
            // Update port # in the following line.
            client.BaseAddress = new Uri("http://192.168.0.2/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                device = new Device
                {
                    Location = "Office",
                    Name = "IoT01",
                    X = "-1.0",
                    Y = "0.0",
                    Z = "2.0",
                    Type = "BME820",
                    Temperature = "0.0",
                    Humidity = "0.0",
                    Pressure = "0.0"
                };

                // Create a new device POST
                var urlDevice = await CreateDeviceAsync(device);
                Console.WriteLine($"Created at {urlDevice}");

                // Get the device
                device = await GetDeviceAsync(urlDevice.PathAndQuery);
                DeviceId = device.Id;
                UrlDevice_PathAndQuery = urlDevice.PathAndQuery;
                //ShowDevice(device);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Called each TimerTick
        public async Task PutDevice(string X, string Y, string Z, double temperature, double humidity, double pressure)
        {
            if(UrlDevice_PathAndQuery == "")
            {
                return;
            }
            device = await GetDeviceAsync(UrlDevice_PathAndQuery);

            //device.X = X;
            //device.Y = Y;
            //device.Z = Z;
            device.Temperature = temperature.ToString("n1");
            device.Humidity = humidity.ToString("n1");
            device.Pressure = pressure.ToString("n1");
            await UpdateDeviceAsync(device);
        }

        //public async Task DeleteDevice()
        //{
        //    if (DeviceId == "")
        //    {
        //        return;
        //    }
        //    await DeleteDeviceAsync(DeviceId);
        //}
        public void DeleteDevice()
        {
            if (UrlDevice_PathAndQuery == "")
            {
                return;
            }
             DeleteDevice(DeviceId);
        }

        private void ShowDevice(Device device)
        {
            Console.WriteLine($"Location: {device.Location}\t" + 
                "Name: " +  $"{device.Name}\t" + 
                "Type: {device.Type}");
        }

        private async Task<Uri> CreateDeviceAsync(Device device)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("api/devices/", device);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        private async Task<Device> GetDeviceAsync(string path)
        {
            Device device = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                device = await response.Content.ReadAsAsync<Device>();
            }
            return device;
        }

        private async Task<Device> UpdateDeviceAsync(Device device)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync($"api/devices/{device.Id}", device);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            device = await response.Content.ReadAsAsync<Device>();
            return device;
        }

        //private async Task<HttpStatusCode> DeleteDeviceAsync(string id)
        //{
        //    HttpResponseMessage response = await client.DeleteAsync($"api/devices/{id}");
        //    return response.StatusCode;
        //}
        private void DeleteDevice(string id)
        {
            client.DeleteAsync($"api/devices/{id}");
        }
    }
}
