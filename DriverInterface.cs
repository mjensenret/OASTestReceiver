using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using OAS_DriverLibrary;
//Imports System.Windows.Threading
using System.Threading;


namespace OASScaleDriver
{
	public class DriverInterface : BaseClass
	{
		//Demo Enumerations
		private bool InstanceFieldsInitialized = false;

		public DriverInterface()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

			private void InitializeInstanceFields()
			{
				localTimer = new Timer(TimerRoutine, null, Timeout.Infinite, Timeout.Infinite);
			}

        //public enum SelectDriverType
        //{
        //	DriverType0 = 0,
        //	DriverType1 = 1,
        //	DriverType2 = 2
        //}

        //public enum SimTypes
        //{
        //	Dynamic = 0,
        //	Static = 1
        //}

        //public enum DynamicSimTypes
        //{
        //	Ramp = 0,
        //	Random = 1,
        //	Sine = 2
        //}


        string kioskIpAddress;
        string kioskPortAddress;
        string listenerPort;

		//Require Variables
		private string m_DriverName = "OAS Scale Interface Driver";

		//Lists of  Properties For Driver Interface and Tags
		private List<OAS_DriverLibrary.ClassProperty> m_DriverProps = new List<OAS_DriverLibrary.ClassProperty>();

		//Shadowed Events
		public new delegate void AsyncReadCallbackEventHandler(OAS_DriverLibrary.ClassTagValue[] TagData);
		public new event AsyncReadCallbackEventHandler AsyncReadCallback;
		public new delegate void UpdateSystemErrorEventHandler(bool ErrorIsActive, string Category, int MessageID, string Message);
		public new event UpdateSystemErrorEventHandler UpdateSystemError;

		//Demo Code
		private bool m_Connected;
		//Active Tags
		private Hashtable m_Tags = new Hashtable();
		private Hashtable m_StaticTagValues = new Hashtable();
		//Used to simulate different Polling Rates
		private Hashtable m_LastUpdateTime = new Hashtable();
		private Timer localTimer;
		private bool m_InTimerRoutine;

#region Driver Section

		public override string DriverName
		{
			get
			{
				return m_DriverName;
			}
			set
			{
				m_DriverName = value;
			}
		}

		public override List<ClassProperty> DriverConfig
		{
			get
			{
				return m_DriverProps;
			}
			set
			{
				m_DriverProps = value;

			}
		}

		public override List<ClassProperty> GetDefaultDriverConfig()
		{
			try
			{
				List<ClassProperty> DriverProps = new List<ClassProperty>();
				DriverProps.Clear();
				// Define the properties for the Interface.  Typically IP Address, Port Number, database connection settings, etc.
				// Example of an enumerated value
				//DriverProps.Add(new ClassProperty("DriverType", "Driver Type", @"Select Driver Type
    //            DriverType0: Example driver type 0
    //            DriverType1: Example driver type 1
    //            DriverType2: Example driver type 2", typeof(SelectDriverType), SelectDriverType.DriverType0, ClassProperty.ePropInputType.Manual));
				// Example of Integer values.
				// Optionally to each property you can add a Visible property binding to the value of another control.  In this example the DriverType selected will show the appropriate integer value.
				// The Visible binding can use | for an OR condition or & for AND condition to combine criteria for the property to be visible. 
				//DriverProps.Add(new ClassProperty("DriverType0Integer", "Driver Type 0 Integer", "Example value for DriverType0", typeof(Int32), 0, ClassProperty.ePropInputType.Manual, "Visible,DriverType.DriverType0"));
				//DriverProps.Add(new ClassProperty("DriverType1Integer", "Driver Type 1 Integer", "Example value for DriverType1", typeof(Int32), 1, ClassProperty.ePropInputType.Manual, "Visible,DriverType.DriverType1"));
				//DriverProps.Add(new ClassProperty("DriverType1And2Integer", "Driver Type 1 and 2 Integer", "Example value for DriverType1 and DriverType2", typeof(Int32), 2, ClassProperty.ePropInputType.Manual, "Visible,DriverType.DriverType1|DriverType.DriverType2"));
				//DriverProps.Add(new ClassProperty("ExampleDouble", "Example Double", "Example value for all types", typeof(double), 1, ClassProperty.ePropInputType.Manual));
                DriverProps.Add(new ClassProperty("TagListenerPort", "Tag Listening Port", "The tag listening port is used to receive the commands from the kiosk.", typeof(string), "", ClassProperty.ePropInputType.Manual));
                DriverProps.Add(new ClassProperty("KioskIPAddress", "Kiosk IP Address", "IP Address of the scale kiosk.", typeof(string), "", ClassProperty.ePropInputType.Manual));
                DriverProps.Add(new ClassProperty("KioskPort", "Kiosk TCP Port", "TCP Port used to communicate back to the scale kiosk", typeof(string), "", ClassProperty.ePropInputType.Manual));

                return DriverProps;
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Configuration", 1, "GetDefaultDriverConfig Exception: " + ex.Message);
			}
			return null;
		}

		public override void Connect()
		{
			try
			{
                //Add Connection Logic. m_DriverProps is a list of ClassDriverProperty in the same order of Get Driver Config
                //SelectDriverType DriverType = (OASScaleDriver.DriverInterface.SelectDriverType)GetPropValue(m_DriverProps, "DriverType");
                //int DriverType0Integer = Convert.ToInt32(GetPropValue(m_DriverProps, "DriverType0Integer"));
                //int DriverType1Integer = Convert.ToInt32(GetPropValue(m_DriverProps, "DriverType1Integer"));
                //int DriverType2Integer = Convert.ToInt32(GetPropValue(m_DriverProps, "DriverType1And2Integer"));
                //double ExampleDouble = Convert.ToDouble(GetPropValue(m_DriverProps, "ExampleDouble"));
                listenerPort = Convert.ToString(GetPropValue(m_DriverProps, "TagListenerPort"));
                kioskIpAddress = Convert.ToString(GetPropValue(m_DriverProps, "KioskIpAddress"));
                kioskPortAddress = Convert.ToString(GetPropValue(m_DriverProps, "KioskPort"));
				if (!(m_Connected))
				{
					localTimer.Change(100, 100);
				}
				m_Connected = true;
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Connect", 1, "GetDefaultDriverConfig Exception: " + ex.Message);
			}
		}

		public override bool Disconnect()
		{
			try
			{
				if (!(m_Connected))
				{
					return m_Connected;
				}

				//Add Disconnection Logic
				localTimer.Change(Timeout.Infinite, Timeout.Infinite);

				lock (m_Tags.SyncRoot)
				{
					m_Tags.Clear();
				}

				m_Connected = false;
				return m_Connected;
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Disconnect", 1, "GetDefaultDriverConfig Exception: " + ex.Message);
			}
			return false;
		}
#endregion

#region Tag Section
		//This Function defines the tag configuration properties and builds the UI For the Tag Configuration Properties
		//Place items in the order you want them to appear in the UI
		//Adding "-->" to the Property Description will add the next property to the right of the current property.
		//If you have a blank String for the Property Help no Help button will be displayed.

		//Note: Do not use the property names TagName and PollingRate.  These are OAS property names that are already defined for the interface.

		public override List<ClassProperty> GetDefaultTagConfig()
		{
			try
			{
				List<ClassProperty> m_TagProps = new List<ClassProperty>();

				//m_TagProps.Add(new ClassProperty("SimType", "Simulation Type", @"The simulation type of a Parameter can be set to one of the following types.
    //            Dynamic: Read only value that changes dynamically from one of the Dynamic Simuation Types
    //            Static: Value is fixed and can be written to.", typeof(SimTypes), SimTypes.Dynamic, ClassProperty.ePropInputType.Manual));

				//m_TagProps.Add(new ClassProperty("DynamicSimType", "Dynamic Simulation Type", @"The dynamic simulation type of a Parameter can be set to one of the following types.
    //            Ramp: Value changes from 0 to 100.
    //            Random: Value changes randomly from 0 to 100
    //            Sine: Value changes from -1 to 1", typeof(DynamicSimTypes), DynamicSimTypes.Ramp, ClassProperty.ePropInputType.Manual, "Visible,SimType.Dynamic"));

				return m_TagProps;
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Configuration", 1, "GetDefaultTagConfig Exception: " + ex.Message);
			}
			return null;
		}

		public override void AddTags(List<ClassProperty>[] Tags)
		{
			try
			{
				//Add Logic. Props is a list of ClassProperty in the same order of Get Tag Config
				lock (m_Tags.SyncRoot)
				{
					foreach (List<ClassProperty> Props in Tags)
					{
						// Use the TagName as a unique identifier for the Tag Name and Paramater being interfaced with.
						string TagID = Convert.ToString(GetPropValue(Props, "TagName"));
						// Use the polling rate to set the communication rate to your device or software application.
						// If you interface uses async callbacks with a subscription rate you could create multple collections of tags based on PollingRate.
						double PollingRate = Convert.ToDouble(GetPropValue(Props, "PollingRate"));

						if (m_Tags.Contains(TagID))
						{
							m_Tags[TagID] = Props;
						}
						else
						{
							m_Tags.Add(TagID, Props);
						}
						if (m_LastUpdateTime.Contains(TagID))
						{
							m_LastUpdateTime.Remove(TagID);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Communications", 1, "AddTags Exception: " + ex.Message);
			}
		}

		public override void RemoveTags(string[] Tags)
		{
			try
			{
				lock (m_Tags.SyncRoot)
				{
					foreach (string TagID in Tags)
					{
						if (m_Tags.Contains(TagID))
						{
							m_Tags.Remove(TagID);
						}
						if (m_LastUpdateTime.Contains(TagID))
						{
							m_LastUpdateTime.Remove(TagID);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Communications", 2, "RemoveTags Exception: " + ex.Message);
			}
		}

		// This call is performed when a Device Read is executed in OAS.
		public override ClassTagValue[] SyncRead(List<ClassProperty>[] Tags)
		{
			try
			{
				DateTime currentTime = DateTime.Now;
				double localSeconds = currentTime.Second + (currentTime.Millisecond / 1000.0);
				ArrayList localArrayList = new ArrayList();

				lock (m_StaticTagValues.SyncRoot)
				{
					foreach (List<ClassProperty> TagItems in Tags)
					{
						string TagID = Convert.ToString(GetPropValue(TagItems, "TagName"));
						//SimTypes SimType = (OASScaleDriver.DriverInterface.SimTypes)GetPropValue(TagItems, "SimType");
						object Value = null;
						//switch (SimType)
						//{
						//	case SimTypes.Dynamic:
						//		DynamicSimTypes DynamicType = (OASScaleDriver.DriverInterface.DynamicSimTypes)GetPropValue(TagItems, "DynamicSimType");
						//		switch (DynamicType)
						//		{
						//			case DynamicSimTypes.Ramp:
						//				Value = localSeconds * 100 / 60;
						//				break;
						//			case DynamicSimTypes.Random:
						//				Value = Microsoft.VisualBasic.VBMath.Rnd() * 100;
						//				break;
						//			case DynamicSimTypes.Sine:
						//				Value = Math.Sin(Math.PI * (localSeconds * 360 / 60) / 180.0);
						//				break;
						//		}
						//		break;
						//	case SimTypes.Static:
						//		if (m_StaticTagValues.Contains(TagID))
						//		{
						//			Value = m_StaticTagValues[TagID];
						//		}
						//		else
						//		{
						//			Value = 0;
						//		}
						//		break;
						//}
						bool Quality = false;
						if (Value != null)
						{
							Quality = true;
						}
						localArrayList.Add(new ClassTagValue(TagID, Value, currentTime, Quality));
					}
				}

				return (ClassTagValue[])localArrayList.ToArray(typeof(ClassTagValue));
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Communications", 3, "SyncRead Exception: " + ex.Message);
			}

			return null;
		}


		public override void WriteValues(string[] TagIDs, object[] Values, List<ClassProperty>[] TagProperties)
		{
			try
			{
				//Add write Logic to actual driver
				int Index = 0;
				for (Index = 0; Index < TagIDs.GetLength(0); Index++)
				{
					//SimTypes SimType = (OASScaleDriver.DriverInterface.SimTypes)GetPropValue(TagProperties[Index], "SimType");
					//if (SimType == SimTypes.Static)
					//{
					//	lock (m_StaticTagValues.SyncRoot)
					//	{
					//		if (m_StaticTagValues.Contains(TagIDs[Index]))
					//		{
					//			m_StaticTagValues[TagIDs[Index]] = Values[Index];
					//		}
					//		else
					//		{
					//			m_StaticTagValues.Add(TagIDs[Index], Values[Index]);
					//		}
					//	}
					//}
				}
			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Communications", 4, "WriteValues Exception: " + ex.Message);
			}
		}

#endregion

#region Demo Driver Code
		// This is a simple example of getting the properties of a tag and using that to generate a update to the tag value
		private void TimerRoutine(object State)
		{
			try
			{
				if (m_InTimerRoutine)
				{
					return;
				}
				m_InTimerRoutine = true;
				DateTime currentTime = DateTime.Now;
				double localSeconds = currentTime.Second + (currentTime.Millisecond / 1000.0);

				ArrayList localArrayList = new ArrayList();

				lock (m_Tags.SyncRoot)
				{
					lock (m_StaticTagValues.SyncRoot)
					{
						List<ClassProperty> TagItems = null;
						foreach (DictionaryEntry de in m_Tags)
						{
							string TagID = Convert.ToString(de.Key);
							TagItems = (List<ClassProperty>)de.Value;

							// Just simulating using the PollingRate property
							bool OKToPoll = true;
							if (m_LastUpdateTime.Contains(TagID))
							{
								double PollingRate = Convert.ToDouble(GetPropValue(TagItems, "PollingRate"));
								DateTime lastUpdateTime = Convert.ToDateTime(m_LastUpdateTime[TagID]);
								if (lastUpdateTime.AddSeconds(PollingRate) > currentTime)
								{
									OKToPoll = false;
								}
							}

							if (OKToPoll)
							{
								if (m_LastUpdateTime.Contains(TagID))
								{
									m_LastUpdateTime[TagID] = currentTime;
								}
								else
								{
									m_LastUpdateTime.Add(TagID, currentTime);
								}
								//SimTypes SimType = (OASScaleDriver.DriverInterface.SimTypes)GetPropValue(TagItems, "SimType");
								object Value = null;
								//switch (SimType)
								//{
								//	case SimTypes.Dynamic:
								//		DynamicSimTypes DynamicType = (OASScaleDriver.DriverInterface.DynamicSimTypes)GetPropValue(TagItems, "DynamicSimType");
								//		switch (DynamicType)
								//		{
								//			case DynamicSimTypes.Ramp:
								//				Value = localSeconds * 100 / 60;
								//				break;
								//			case DynamicSimTypes.Random:
								//				Value = Microsoft.VisualBasic.VBMath.Rnd() * 100;
								//				break;
								//			case DynamicSimTypes.Sine:
								//				Value = Math.Sin(Math.PI * (localSeconds * 360 / 60) / 180.0);
								//				break;
								//		}
								//		break;
								//	case SimTypes.Static:
								//		if (m_StaticTagValues.Contains(TagID))
								//		{
								//			Value = m_StaticTagValues[TagID];
								//		}
								//		else
								//		{
								//			Value = 0;
								//		}
								//		break;
								//}
								bool Quality = false;
								if (Value != null)
								{
									Quality = true;
								}
								// You can include mutiple values to the same tag with different timestamps in the same callback if you like.
								// In this example it just updates when the timer fires and the check for the PollingRate succeeds.
								localArrayList.Add(new ClassTagValue(TagID, Value, currentTime, Quality));

							}
						}
					}
				}
				// Firing this event will update the tag values
				if (AsyncReadCallback != null)
					AsyncReadCallback((ClassTagValue[])localArrayList.ToArray(typeof(ClassTagValue)));

				// The following can be used in any routine to post an error during Runtime operation of OAS.
				//RaiseEvent UpdateSystemError(True, "Communications", 1, "An example of posting a system error")


			}
			catch (Exception ex)
			{
				if (UpdateSystemError != null)
					UpdateSystemError(true, "Communications", 5, "TimerRoutine Exception: " + ex.Message);
			}
			m_InTimerRoutine = false;
		}


#endregion
	}





}