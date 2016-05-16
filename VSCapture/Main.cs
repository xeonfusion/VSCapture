/*
 * This file is part of VitalSignsCapture v1.005.
 * Copyright (C) 2012-2016 John George K., xeonfusion@users.sourceforge.net

    VitalSignsCapture is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VitalSignsCapture is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with VitalSignsCapture.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Timers;


namespace VSCapture
{
    class Program
    {
		static EventHandler dataEvent;
		
        static void Main(string[] args)
        {
            Console.WriteLine("VitalSignsCapture (C)2012-16 John George K.");
            // Create a new SerialPort object with default settings.
			DSerialPort _serialPort = DSerialPort.getInstance;

            Console.WriteLine("Select the Port to which Datex AS3 Monitor is to be connected, Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine(" {0}", s);
            }


            Console.Write("COM port({0}): ", _serialPort.PortName.ToString());
            string portName = Console.ReadLine();

            if (portName != "")
            {
                // Allow the user to set the appropriate properties.
                _serialPort.PortName = portName;
             }


            try
            {
                _serialPort.Open();
                
				if (_serialPort.OSIsUnix())
                {
                    dataEvent += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            ReadData(sender);
                        });
                }
								
				if(!_serialPort.OSIsUnix())
				{
				_serialPort.DataReceived += new SerialDataReceivedEventHandler(p_DataReceived);
				}

                Console.WriteLine("You may now connect the serial cable to the Datex AS3 Monitor");
                Console.WriteLine("Press any key to continue..");
                
				Console.ReadKey(true);
												
                //if (_serialPort.CtsHolding)
                //{
                    Console.WriteLine();
                    Console.Write("Enter Transmission interval (seconds):");
                    string sInterval = Console.ReadLine();

                    short nInterval = 5;
                    if (sInterval != "") nInterval = Convert.ToInt16(sInterval);

                    Console.WriteLine("Requesting {0} second Transmission from monitor", nInterval);
                    Console.WriteLine();
                    Console.WriteLine("Data will be written to CSV file AS3ExportData.csv in same folder");

					//Request transfer based on the DRI level of the monitor
					_serialPort.RequestTransfer(DataConstants.DRI_PH_DISPL, nInterval, DataConstants.DRI_LEVEL_2005); // Add Request Transmission
					_serialPort.RequestTransfer(DataConstants.DRI_PH_DISPL, nInterval, DataConstants.DRI_LEVEL_2003); // Add Request Transmission
					_serialPort.RequestTransfer(DataConstants.DRI_PH_DISPL, nInterval, DataConstants.DRI_LEVEL_2001); // Add Request Transmission

                    //_serialPort.RequestTransfer(DataConstants.DRI_PH_DISPL, -1); // Add Single Request Transmission

                //}
                //WaitForSeconds(5);

                Console.WriteLine("Press Escape button to Stop");
				
				if(_serialPort.OSIsUnix()) 
					{
						do
	                    {
	                        if (_serialPort.BytesToRead != 0)
	                        {
	                            //dataEvent.Invoke(new object(), new EventArgs());
								dataEvent.Invoke(_serialPort, new EventArgs());
	                        }
	                        
	                        if (Console.KeyAvailable == true)
	                        {
	                            if (Console.ReadKey(true).Key == ConsoleKey.Escape) break;
	                        }
	                    }
	                    while (Console.KeyAvailable == false);
						
					}                
				
				if(!_serialPort.OSIsUnix())
				{
					ConsoleKeyInfo cki;
				
					do
	                {
						cki = Console.ReadKey(true);
					}
	                while (cki.Key != ConsoleKey.Escape);
				}
				

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }
            finally
            {
                _serialPort.StopTransfer();

                _serialPort.Close();
				
            }


        }

		
		static void p_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            ReadData(sender);

        }

        public static void ReadData(object sender)
        {
            try
            {
                //_serialPort.ReadBuffer();
				(sender as DSerialPort).ReadBuffer();
                
            }
            catch (TimeoutException) { }
        }

        public static void WaitForSeconds(int nsec)
        {
            DateTime dt = DateTime.Now;
            DateTime dt2 = dt.AddSeconds(nsec);
            do
            {
                dt = DateTime.Now;
            }
            while (dt2 > dt);

        }


    }


}