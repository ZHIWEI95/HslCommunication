﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.Siemens;
using HslCommunication;
using System.Xml.Linq;
using HslCommunication.Core.Pipe;
using HslCommunicationDemo.PLC.Siemens;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormSiemensS7Plus : HslFormContent
	{
		public FormSiemensS7Plus( )
		{
			InitializeComponent( );
			siemensTcpNet = new SiemensS7Plus( );
		}


		private SiemensS7Plus siemensTcpNet = null;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;


		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( Helper.GetSiemensS7Address( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}

		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Siemens Read PLC Demo";

				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				label_info.Text = "If it is not clear, do not set it";
			}
		}
		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		
		#region Connect And Close
		
		private void button1_Click( object sender, EventArgs e )
		{
			if(!int.TryParse(textBox_port.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			siemensTcpNet.IpAddress = textBox_ip.Text;
			siemensTcpNet.Port = port;
			//siemensTcpNet.LocalBinding = new System.Net.IPEndPoint( System.Net.IPAddress.Parse( "127.0.0.1" ), 12345 );
			try
			{
				//siemensTcpNet.Rack = byte.Parse( textBox_rack.Text );
				//siemensTcpNet.Slot = byte.Parse( textBox_slot.Text );

				//if (!string.IsNullOrEmpty( textBox_localTSAP.Text )) siemensTcpNet.LocalTSAP = int.Parse( textBox_localTSAP.Text );
				siemensTcpNet.LogNet = LogNet;


				OperateResult connect = siemensTcpNet.ConnectServer( );
				if (connect.IsSuccess)
				{
					textBox_pdu.Text = siemensTcpNet.PDULength.ToString( );
					MessageBox.Show( StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					// 设置子控件的读取能力
					userControlReadWriteDevice1.SetReadWriteNet( siemensTcpNet, "M100", true );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( siemensTcpNet, "M100", string.Empty );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => siemensTcpNet.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );

					// 设置代码示例
					codeExampleControl.SetCodeText( siemensTcpNet );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message );
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			siemensTcpNet.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}
		
		#endregion


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox_ip.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox_port.Text );
			element.SetAttributeValue( DemoDeviceList.XmlRack, textBox_rack.Text );
			element.SetAttributeValue( DemoDeviceList.XmlSlot, textBox_slot.Text );
			element.SetAttributeValue( "LocalTSAP",   textBox_localTSAP.Text );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox_ip.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox_port.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox_rack.Text = element.Attribute( DemoDeviceList.XmlRack ).Value;
			textBox_slot.Text = element.Attribute( DemoDeviceList.XmlSlot ).Value;
			textBox_localTSAP.Text   = GetXmlValue( element, "LocalTSAP",   string.Empty, m => m );

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}