#include <Tle493d_w2b6.h>		// load the Magnetic 3D Sensor library


#define PEDAL_LEFT 0			// left pedal joystick ------> pin 0 (P2.9)
#define PEDAL_RIGHT 1			// right pedal joystick -----> pin 1 (P2.7)

Tle493d_w2b6 sensor = Tle493d_w2b6();

void setup() {

	// start Serial communication and wait for the port to open

	Serial.begin(9600);
  	while(!Serial);

  	// set the pedal pins to input so we can read the analog value

	pinMode(PEDAL_LEFT, INPUT);
	pinMode(PEDAL_RIGHT, INPUT);

	// setting the pin LED2 to HIGH (1) will provide 3.3v power to the sensor at the end of the board

	pinMode(LED2, OUTPUT);
	digitalWrite(LED2, 1);

	// there are some bugs in the sensor library, so we need to begin() two times, and then disableTemp().

	sensor.begin();
	sensor.begin();
	sensor.disableTemp();

}


// this can be called to read the analog value of the pin and convert it to a pedal value 0-500

uint16_t pedalValue(uint8_t pedal) {
	uint16_t v = analogRead(pedal);
	return 500 - min(v, 500);
}


// this gets the angle of the steering wheel based on the magnetic sensor sensing it in degrees

float wheelAngle() {
	return sensor.getAzimuth() / 3.141592 * 180;
}


// main loop

void loop() {

	sensor.updateData();		// trigger the sensor to get the magnetic data

	// send data over serial -------> computer

	// Example:
	
	// WHEEL 45.45 0 250

	// "WHEEL" tells the computer that this is the magnetic steering wheel
	// 45.45 means the wheel is at 45.45 degrees
	// 0 means the left pedal (brakes) is at 0%
	// 250 means the right pedal (gas) is at 50%

	Serial.print("WHEEL ");
	Serial.print(wheelAngle());
	Serial.print(" ");
	Serial.print(pedalValue(PEDAL_LEFT));
	Serial.print(" ");
	Serial.println(pedalValue(PEDAL_RIGHT));

}
