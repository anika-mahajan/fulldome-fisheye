/* Copyright (c) 2024 dr. ext (Vladimir Sigalkin) */

using UnityEngine;

namespace extOSC.Examples
{
	public class LineDrawer : MonoBehaviour
	{
		#region Public Vars

		public string Address = "/StarPosition";

		[Header("OSC Settings")]
		public OSCReceiver Receiver;

		#endregion

		#region Unity Methods

		protected virtual void Start()
		{
			Receiver.Bind(Address, ReceivedMessage);
		}

		#endregion

		#region Private Methods

		private void ReceivedMessage(OSCMessage message)
		{
			Debug.LogFormat("Received: {0}", message);
		}

		#endregion
	}
}