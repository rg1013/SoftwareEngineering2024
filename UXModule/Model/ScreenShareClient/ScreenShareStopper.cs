﻿
using Networking.Communication;
using Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UXModule.ViewModel;

namespace UXModule.Model
{
    public partial class ScreenshareClient : INotificationHandler
    {

        
        /// Method to stop screensharing. Calling this will stop sending both the image sending
        /// task and confirmation sending task. It will also call stop on the processor and capturer.
        
        public void StopScreensharing()
        {

            Debug.Assert(_id != null, Utils.GetDebugMessage("_id property found null", withTimeStamp: true));
            Debug.Assert(_name != null, Utils.GetDebugMessage("_name property found null", withTimeStamp: true));
            DataPacket deregisterPacket = new(_id, _name, ClientDataHeader.Deregister.ToString(), "", false, false, null);
            var serializedDeregisterPacket = JsonSerializer.Serialize(deregisterPacket);

            StopImageSending();
            StopConfirmationSending();

            // Sending de-rgister request to server
            _communicator.Send(serializedDeregisterPacket, Utils.ModuleIdentifier, null);
            Trace.WriteLine(Utils.GetDebugMessage("Successfully sent DEREGISTER packet to server", withTimeStamp: true));
        }

        
        /// Method to stop sending confirmation packets. Will be called only when the client
        /// stops screensharing.
        
        private void StopConfirmationSending()
        {
            if (_sendConfirmationTask == null)
            {
                return;
            }

            try
            {
                _confirmationCancellationToken = true;
                _sendConfirmationTask.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Unable to cancel confirmation sending task: {e.Message}", withTimeStamp: true));
            }

            _sendConfirmationTask = null;
        }

        
        /// Method to stop image sending. Will be called whenever the screenshare is stopped by
        /// the client or the client is not on the displayed screen of the server.
        
        private void StopImageSending()
        {
            if (_sendImageTask == null)
            {
                return;
            }

            _capturer.StopCapture();
            _processor.StopProcessing();
            Trace.WriteLine(Utils.GetDebugMessage("Successfully stopped capturer and processor"));

            Debug.Assert(_sendImageTask != null,
                Utils.GetDebugMessage("_sendImageTask is not null, cannot stop image sending"));

            try
            {
                _imageCancellationToken = true;
                _sendImageTask.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Unable to cancel image sending task: {e.Message}", withTimeStamp: true));
            }

            _sendImageTask = null;
        }

        
        /// Sends confirmation packet to server once every five seconds. The confirmation packet
        /// does not contain any data. The confirmation packets are always sent once the client
        /// has started screen share. In case the network gets disconnected, these packtes will
        /// stop reaching the server, as a result of which the server will remove the client
        /// as a 'screen sharer'.
        
        private void SendConfirmationPacket()
        {
            _confirmationCancellationToken = false;
            Debug.Assert(_id != null, Utils.GetDebugMessage("_id property found null"));
            Debug.Assert(_name != null, Utils.GetDebugMessage("_name property found null"));
            DataPacket confirmationPacket = new(_id, _name, ClientDataHeader.Confirmation.ToString(), "", false, false, null);
            var serializedConfirmationPacket = JsonSerializer.Serialize(confirmationPacket);

            _sendConfirmationTask = new Task(() =>
            {
                while (!_confirmationCancellationToken)
                {
                    _communicator.Send(serializedConfirmationPacket, Utils.ModuleIdentifier, null);
                    Thread.Sleep(5000);
                }
            });

            Trace.WriteLine(Utils.GetDebugMessage("Starting Confirmation packet sending", withTimeStamp: true));
            _sendConfirmationTask.Start();
        }

        
        /// Used by dashboard module to set the id and name for the client
    
        public void SetUser(string id, string name)
        {
            _id = id;
            _name = name;
            Trace.WriteLine(Utils.GetDebugMessage("Successfully set client name and id"));
        }
    }
}
