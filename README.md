# Using Win10 Core and Azure IoT Services with Raspberry Pi2
I recently hosted an IoT booth at a Microsoft Internal conference and showcased a demo of using Win 10 Core RTM build on a Raspberry Pi2 (also referred as Pi from here on) and connecting it to Microsoft Azure IoT Services.

I wanted to show a very simple scenario to highlight how easy it is to develop and deploy Windows 10 Universal apps for IoT Devices and connect them to Azure IoT Services. The Scenario in my case was generating metrics of visitors coming to the booth. The application collects details about the organization the visitors belong to and uses Azure IoT Services to process the data and generate reports showing metrics and distribution of visitors. The below snapshot shows the output of how the report looks like:

There were many queries around the code sample and how to setup the device for connecting to Azure IoT Services. This repo contains the sample code used for the event.
Also here is the link to the blog series for further details:
http://connectedstuff.net/2015/08/18/using-win10-core-and-azure-iot-services-with-raspberry-pi2/
