﻿// https://github.com/LattePandaTeam/LattePanda-Development-Support/tree/master/LattePandaFirmata

// if you make your class, just write in here : [wite by lattepanda team]
/************************************************************
* Copyright(C),2016-2017,LattePanda
* FileName: arduino.cs
* Author:   Kevlin Sun
* Version:  V0.8
* Date:     2016.7
* Description: LattePanda.Firmata is an open-source Firmata
    library provided by LattePanda, which is suitable for
    Windows apps developed in Visual Studio. this class allows
    you to control the Arduino board from Windows apps:
    reading and writing to the digital pins
    reading the analog inputs
    controlling servo
    send and receive data to the I2C Bus
* This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.
* Special thanks to Tim Farley, on whose Firmata.NET library
    this code is based.
*************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
//using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CLattePanda
        {
            
        }
    }
    namespace LattePanda
    {
        //public class CLattePanda
        //{
            public delegate void DidI2CDataReveive(byte address, byte register, byte[] data);
            public delegate void DigitalPinUpdated(byte pin, byte state);
            public delegate void AnalogPinUpdated(int pin, int value);

            public class CArduino
            {
                public const byte LOW = 0;
                public const byte HIGH = 1;
                public const byte INPUT = 0;
                public const byte OUTPUT = 1;
                public const byte ANALOG = 2;
                public const byte PWM = 3;
                public const byte SERVO = 4;
                public const int NONE = -1;
                public const byte I2C_MODE_WRITE = 0x00;
                public const byte I2C_MODE_READ_ONCE = 0x08;
                public const byte I2C_MODE_READ_CONTINUOUSLY = 0x10;
                public const byte I2C_MODE_STOP_READING = 0x18;

                public event DidI2CDataReveive didI2CDataReveive;
                public event DigitalPinUpdated digitalPinUpdated;
                public event AnalogPinUpdated analogPinUpdated;

                public const int MAX_DATA_BYTES = 64;
                public const int DIGITAL_MESSAGE = 0x90; // send data for a digital port
                public const int ANALOG_MESSAGE = 0xE0; // send data for an analog pin (or PWM)
                public const int REPORT_VERSION = 0xF9; // report firmware version
                public const int START_SYSEX = 0xF0; // start a MIDI SysEx message
                public const int END_SYSEX = 0xF7; // end a MIDI SysEx message
                public const int I2C_REPLY = 0x77; // I2C reply messages from an I/O board to a host

                private const int TOTAL_PORTS = 2;
                private const int SERVO_CONFIG = 0x70; // set max angle, minPulse, maxPulse, freq
                private const int REPORT_ANALOG = 0xC0; // enable analog input by pin #
                private const int REPORT_DIGITAL = 0xD0; // enable digital input by port
                private const int SET_PIN_MODE = 0xF4; // set a pin to INPUT/OUTPUT/PWM/etc
                private const int SYSTEM_RESET = 0xFF; // reset from MIDI
                private const int I2C_REQUEST = 0x76; // I2C request messages from a host to an I/O board
                private const int I2C_CONFIG = 0x78; // Configure special I2C settings such as power pins and delay times
                private SerialPort _serialPort;
                private int _delay;

                public volatile int[] _digitalOutputData = new int[MAX_DATA_BYTES];
                public volatile int[] _digitalInputData = new int[MAX_DATA_BYTES];
                public volatile int[] _analogInputData = new int[MAX_DATA_BYTES];

                private Thread _readThread = null;

                /// <summary>
                ///
                /// </summary>
                /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
                /// <param name="baudRate">The baud rate of the communication. Default 57600</param>
                /// <param name="autoStart">Determines whether the serial port should be opened automatically.
                ///                     use the Open() method to open the connection manually.</param>
                /// <param name="_delay">Time delay that may be required to allow some arduino models
                ///                     to reboot after opening a serial connection. The delay will only activate
                ///                     when autoStart is true.</param> 
                /// <param name="autoListen">This parameter activate listen mode. Without this mode you will be
                ///                         unable to use events. This param will only activate when autoStart is true.</param>
                public CArduino(string serialPortName, Int32 baudRate, bool autoStart, int delay, bool autoListen = true)
                {
                    _serialPort = new SerialPort(serialPortName, baudRate);
                    _serialPort.DataBits = 8;
                    _serialPort.Parity = Parity.None;
                    _serialPort.StopBits = StopBits.One;

                    if (autoStart)
                    {
                        this._delay = delay;
                        this.Open(autoListen);
                    }
                }
                public bool IsOpen() { return _serialPort.IsOpen; } // ojw5014
                /// <summary>
                /// Creates an instance of the Arduino object, based on a user-specified serial port.
                /// Assumes default values for baud rate (57600) and reboot delay (8 seconds)
                /// and automatically opens the specified serial connection.
                /// </summary>
                /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
                public CArduino(string serialPortName) : this(serialPortName, 57600, true, 8000) { }

                /// <summary>
                /// Creates an instance of the Arduino object, based on user-specified serial port and baud rate.
                /// Assumes default value for reboot delay (8 seconds).
                /// and automatically opens the specified serial connection.
                /// </summary>
                /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
                /// <param name="baudRate">Baud rate.</param>
                public CArduino(string serialPortName, Int32 baudRate) : this(serialPortName, baudRate, true, 8000) { }

                /// <summary>
                /// Creates an instance of the Arduino object using default arguments.
                /// Assumes the arduino is connected as the HIGHEST serial port on the machine,
                /// default baud rate (57600), and a reboot delay (8 seconds).
                /// and automatically opens the specified serial connection.
                /// </summary>
                public CArduino() : this(CArduino.list().ElementAt(list().Length - 1), 57600, true, 8000) { }
                /// <summary>
                /// Opens the serial port connection, should it be required. By default the port is
                /// opened when the object is first created.
                /// </summary>
                public void Open(bool isListen)
                {
                    _serialPort.DtrEnable = true;
                    _serialPort.Open();

                    Thread.Sleep(_delay);

                    byte[] command = new byte[2];

                    for (int i = 0; i < 6; i++)
                    {
                        command[0] = (byte)(REPORT_ANALOG | i);
                        command[1] = (byte)1;
                        _serialPort.Write(command, 0, 2);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        command[0] = (byte)(REPORT_DIGITAL | i);
                        command[1] = (byte)1;
                        _serialPort.Write(command, 0, 2);
                    }
                    command = null;

                    if (isListen)
                    {
                        this.StartListen();
                    }
                }
                /// <summary>
                /// Closes the serial port.
                /// </summary>
                public void Close()
                {
                    StopListen();
                    _serialPort.Close();
                }

                /// <summary>
                /// Start separate thread to monitor Firmata data for events didI2CDataReveive, digitalPinUpdated and analogPinUpdated
                /// </summary>
                public void StartListen()
                {
                    if (_readThread == null)
                    {
                        _readThread = new Thread(processInput);
                        _readThread.Start();
                    }
                }

                /// <summary>
                /// Stop monitor thread
                /// </summary>
                public void StopListen()
                {
                    if (_readThread != null)
                    {
                        // Hold child thread
                        _readThread.Join(500);
                        // GC thread
                        _readThread = null;
                    }
                }

                internal void callDidI2CDataReveive(byte address, byte register, byte[] data)
                {
                    if (this.didI2CDataReveive != null)
                        this.didI2CDataReveive(address, register, data);
                }
                internal void callDigitalPinUpdated(byte pin, byte state)
                {
                    if (this.digitalPinUpdated != null)
                        this.digitalPinUpdated(pin, state);
                }
                internal void callAnalogPinUpdated(int pin, int value)
                {
                    if (this.analogPinUpdated != null)
                        this.analogPinUpdated(pin, value);
                }

                /// <summary>     
                /// Lists all available serial ports on current system.
                /// </summary>
                /// <returns>An array of strings containing all available serial ports.</returns>
                public static string[] list()
                {
                    return SerialPort.GetPortNames();
                }
                /// <summary>
                /// Sets the mode of the specified pin (INPUT or OUTPUT).
                /// </summary>
                /// <param name="pin">The arduino pin.</param>
                /// <param name="mode">Mode CArduino.INPUT CArduino.OUTPUT CArduino.ANALOG CArduino.PWM or CArduino.SERVO .</param>
                public void pinMode(int pin, byte mode)
                {
                    byte[] message = new byte[3];
                    message[0] = (byte)(SET_PIN_MODE);
                    message[1] = (byte)(pin);
                    message[2] = (byte)(mode);
                    _serialPort.Write(message, 0, 3);
                    message = null;
                }
                /// <summary>
                /// Returns the last known state of the digital pin.
                /// </summary>
                /// <param name="pin">The arduino digital input pin.</param>
                /// <returns>CArduino.HIGH or CArduino.LOW</returns>
                public int digitalRead(int pin)
                {
                    return ((_digitalInputData[pin >> 3] >> (pin & 0x07)) & 0x01);
                }

                /// <summary>
                /// Returns the last known state of the analog pin.
                /// </summary>
                /// <param name="pin">The arduino analog input pin.</param>
                /// <returns>A value representing the analog value between 0 (0V) and 1023 (5V).</returns>
                public int analogRead(int pin)
                {
                    return _analogInputData[pin];
                }
                /// <summary>
                /// Write to a digital pin that has been toggled to output mode with pinMode() method.
                /// </summary>
                /// <param name="pin">The digital pin to write to.</param>
                /// <param name="value">Value either CArduino.LOW or CArduino.HIGH.</param>
                public void digitalWrite(int pin, byte value)
                {
                    int portNumber = (pin >> 3) & 0x0F;
                    byte[] message = new byte[3];

                    if ((int)value == 0)
                        _digitalOutputData[portNumber] &= ~(1 << (pin & 0x07));
                    else
                        _digitalOutputData[portNumber] |= (1 << (pin & 0x07));

                    message[0] = (byte)(DIGITAL_MESSAGE | portNumber);
                    message[1] = (byte)(_digitalOutputData[portNumber] & 0x7F);
                    message[2] = (byte)(_digitalOutputData[portNumber] >> 7);
                    _serialPort.Write(message, 0, 3);
                }

                /// <summary>
                /// Write to an analog pin using Pulse-width modulation (PWM).
                /// </summary>
                /// <param name="pin">Analog output pin.</param>
                /// <param name="value">PWM frequency from 0 (always off) to 255 (always on).</param>
                public void analogWrite(int pin, int value)
                {
                    byte[] message = new byte[3];
                    message[0] = (byte)(ANALOG_MESSAGE | (pin & 0x0F));
                    message[1] = (byte)(value & 0x7F);
                    message[2] = (byte)(value >> 7);
                    _serialPort.Write(message, 0, 3);
                }
                /// <summary>
                /// controlling servo
                /// </summary>
                /// <param name="pin">Servo output pin.</param>
                /// <param name="angle">Servo angle from 0 to 180.</param>
                public void servoWrite(int pin, int angle)
                {
                    byte[] message = new byte[3];
                    message[0] = (byte)(ANALOG_MESSAGE | (pin & 0x0F));
                    message[1] = (byte)(angle & 0x7F);
                    message[2] = (byte)(angle >> 7);
                    _serialPort.Write(message, 0, 3);
                }
                /// <summary>
                /// Init I2C Bus.
                /// </summary>
                /// <param name="angle">delay is necessary for some devices such as WiiNunchuck</param>
                public void wireBegin(Int16 delay)
                {
                    byte[] message = new byte[5];
                    message[0] = (byte)(0XF0);
                    message[1] = (byte)(I2C_CONFIG);
                    message[2] = (byte)(delay & 0x7F);
                    message[3] = (byte)(delay >> 7);
                    message[4] = (byte)(END_SYSEX);//END_SYSEX
                    _serialPort.Write(message, 0, 5);
                }
                /// <summary>
                /// Write to a digital pin that has been toggled to output mode with pinMode() method.
                /// </summary>
                /// <param name="slaveAddress">I2C slave address,7 bit</param>
                /// <param name="slaveRegister">value either I2C slave Register or CArduino.NONE</param>
                /// <param name="data">Write data or length of read data.</param>
                /// <param name="mode">Value either CArduino.I2C_MODE_WRITE or CArduino.I2C_MODE_READ_ONCE or CArduino.I2C_MODE_READ_ONCE or CArduino.I2C_MODE_STOP_READING</param>
                public void wireRequest(byte slaveAddress, Int16 slaveRegister, Int16[] data, byte mode)
                {
                    byte[] message = new byte[MAX_DATA_BYTES];
                    message[0] = (byte)(0xF0);
                    message[1] = (byte)(I2C_REQUEST);
                    message[2] = (byte)(slaveAddress);
                    message[3] = (byte)(mode);
                    int index = 4;
                    if (slaveRegister != CArduino.NONE)
                    {
                        message[index] = (byte)(slaveRegister & 0x7F);
                        index += 1;
                        message[index] = (byte)(slaveRegister >> 7);
                        index += 1;
                    }
                    for (int i = 0; i < (data.Count()); i++)
                    {
                        message[index] = (byte)(data[i] & 0x7F);
                        index += 1;
                        message[index] = (byte)(data[i] >> 7);
                        index += 1;
                    }
                    message[index] = (byte)(END_SYSEX);
                    _serialPort.Write(message, 0, index + 1);
                }
                private int available()
                {
                    return _serialPort.BytesToRead;
                }

                /// <summary>
                /// This function executes only in thread
                /// </summary>
                public void processInput()
                {
                    var autoEvent = new AutoResetEvent(false);
                    var inputProcessor = new InputProcessor(_serialPort, this);
                    // Execute method by a timer every 30ms
                    var stateTimer = new Timer(inputProcessor.InputProcess, autoEvent, 0, 30);

                    // Wait when method return anything
                    autoEvent.WaitOne();
                    // This line will executes only when inputProcessor are done. For example serialPort is closed.
                    stateTimer.Dispose();
                }
            } // End Arduino class

            class InputProcessor
            {
                private SerialPort _serialPort;
                private CArduino _arduino;

                private bool _parsingSysex;
                private int _sysexBytesRead;
                private int _waitForData = 0;
                private int _executeMultiByteCommand = 0;
                private int _multiByteChannel = 0;
                private int[] _storedInputData = new int[CArduino.MAX_DATA_BYTES * 2];

                private int _majorVersion = 0;
                private int _minorVersion = 0;

                public InputProcessor(SerialPort serialPort, CArduino arduino)
                {
                    _serialPort = serialPort;
                    _arduino = arduino;
                }

                public void InputProcess(Object stateInfo)
                {
                    if (Monitor.TryEnter(_serialPort))
                    {
                        try
                        {
                            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

                            if (!_serialPort.IsOpen)
                            {
                                // Signal the waiting thread we are done
                                autoEvent.Set();
                                return;
                            }

                            if (_serialPort.BytesToRead == 0)
                            {
                                return;
                            }

                            int inputData = _serialPort.ReadByte();
                            int command;

                            if (_parsingSysex)
                            {
                                if (inputData == CArduino.END_SYSEX)
                                {
                                    _parsingSysex = false;
                                    if (_sysexBytesRead > 5 && _storedInputData[0] == CArduino.I2C_REPLY)
                                    {
                                        byte[] i2cReceivedData = new byte[(_sysexBytesRead - 1) / 2];
                                        for (int i = 0; i < i2cReceivedData.Count(); i++)
                                        {
                                            i2cReceivedData[i] = (byte)(_storedInputData[(i * 2) + 1] | _storedInputData[(i * 2) + 2] << 7);
                                        }
                                        _arduino.callDidI2CDataReveive(i2cReceivedData[0], i2cReceivedData[1], i2cReceivedData.Skip(2).ToArray());

                                    }
                                    _sysexBytesRead = 0;
                                }
                                else
                                {
                                    _storedInputData[_sysexBytesRead] = inputData;
                                    _sysexBytesRead++;
                                }
                            }
                            else if (_waitForData > 0 && inputData < 128)
                            {
                                _waitForData--;
                                _storedInputData[_waitForData] = inputData;

                                if (_executeMultiByteCommand != 0 && _waitForData == 0)
                                {
                                    //we got everything
                                    switch (_executeMultiByteCommand)
                                    {
                                        case CArduino.DIGITAL_MESSAGE:
                                            int currentDigitalInput = (_storedInputData[0] << 7) + _storedInputData[1];
                                            for (int i = 0; i < 8; i++)
                                            {
                                                if (((1 << i) & (currentDigitalInput & 0xff)) != ((1 << i) & (_arduino._digitalInputData[_multiByteChannel] & 0xff)))
                                                {
                                                    if ((((1 << i) & (currentDigitalInput & 0xff))) != 0)
                                                    {
                                                        _arduino.callDigitalPinUpdated((byte)(i + _multiByteChannel * 8), CArduino.HIGH);
                                                    }
                                                    else
                                                    {
                                                        _arduino.callDigitalPinUpdated((byte)(i + _multiByteChannel * 8), CArduino.LOW);
                                                    }
                                                }
                                            }
                                            _arduino._digitalInputData[_multiByteChannel] = (_storedInputData[0] << 7) + _storedInputData[1];

                                            break;
                                        case CArduino.ANALOG_MESSAGE:
                                            _arduino._analogInputData[_multiByteChannel] = (_storedInputData[0] << 7) + _storedInputData[1];
                                            _arduino.callAnalogPinUpdated(_multiByteChannel, (_storedInputData[0] << 7) + _storedInputData[1]);
                                            break;
                                        case CArduino.REPORT_VERSION:
                                            this._majorVersion = _storedInputData[1];
                                            this._minorVersion = _storedInputData[0];
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (inputData < 0xF0)
                                {
                                    command = inputData & 0xF0;
                                    _multiByteChannel = inputData & 0x0F;
                                    switch (command)
                                    {
                                        case CArduino.DIGITAL_MESSAGE:
                                        case CArduino.ANALOG_MESSAGE:
                                        case CArduino.REPORT_VERSION:
                                            _waitForData = 2;
                                            _executeMultiByteCommand = command;
                                            break;
                                    }
                                }
                                else if (inputData == 0xF0)
                                {
                                    _parsingSysex = true;
                                    // commands in the 0xF* range don't use channel data
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Ojw.CMessage.Write_Error(ex.ToString());
                        }
                        finally
                        {
                            Monitor.Exit(_serialPort);
                        }
                    }
                }
            }

        //}
    } // End namespace       
}
