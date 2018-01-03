/***************************************************************************
This is a library for the BME280 humidity, temperature & pressure sensor

Designed specifically to work with the Adafruit BME280 Breakout
----> http://www.adafruit.com/products/2650

These sensors use I2C or SPI to communicate, 2 or 4 pins are required
to interface. The device's I2C address is either 0x76 or 0x77.

Adafruit invests time and resources providing this open source code,
please support Adafruit andopen-source hardware by purchasing products
from Adafruit!

Written by Limor Fried & Kevin Townsend for Adafruit Industries.
BSD license, all text above must be included in any redistribution
***************************************************************************/

#include <Wire.h>
#include <SPI.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#include <ESP8266WiFi.h>
#include <aREST.h>


#define SEALEVELPRESSURE_HPA (1013.25)
#define LISTEN_PORT           80

Adafruit_BME280 bme; // I2C
aREST rest = aREST();
WiFiServer server(LISTEN_PORT);

unsigned long delayTime;
const char* ssid = "yourwifihere";
const char* password = "yourpasswordhere";

//VARTEMPERATURE
//VARHUMIDITY
//VARPRESSURE
//VARALTITUDE
float temperature;

void setup() {

	Serial.begin(115200);
	Serial.println("Launchify IOT Cube!");

	bool status;

	// default settings
	// (you can also pass in a Wire library object like &Wire2)
	status = bme.begin();
	if (!status) {
		Serial.println("Could not find a valid sensor, check wiring!");
		while (1);
	}

	Serial.println("Checking WiFi");
	delayTime = 1000;

	Serial.println();

	delay(100); // let sensor boot up
				//INITTEMPERATURE
				//INITHUMIDITY
				//INITPRESSURE
				//INITALTITUDE
	temperature = 24;
	rest.variable("temperature", &temperature);

	// Give name & ID to the device (ID should be 6 characters long)
	rest.set_id("1");
	rest.set_name("esp8266");

	// Connect to WiFi
	WiFi.begin(ssid, password);
	while (WiFi.status() != WL_CONNECTED) {
		delay(500);
		Serial.print(".");
	}
	Serial.println("");
	Serial.println("WiFi connected");

	// Start the server
	server.begin();
	Serial.println("Server started");

	// Print the IP address
	Serial.println(WiFi.localIP());
}


void loop() {
	// Handle REST calls
	//INSERTTEMP
	//INSERTHUMIDITY
	//INSERTPRESSURE
	//INSERTALTITUDE
	temperature = bme.readTemperature();
	delay(1);
	WiFiClient client = server.available();
	if (!client) {
		return;
	}
	while (!client.available()) {
		delay(1);
	}
	rest.handle(client);
}


