#region Copyright (c) 2009-2016 Misakai Ltd.
/*************************************************************************
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or(at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.If not, see<http://www.gnu.org/licenses/>.
*************************************************************************/
#endregion Copyright (c) 2009-2016 Misakai Ltd.

using System;
using System.Collections.Generic;
using Emitter.Collections;

namespace Emitter.Network
{
    /// <summary>
    /// Represents the SUBACK message from broker to client.
    /// </summary>
    internal sealed class MqttSubackPacket : MqttPacket
    {
        #region Static Members

        private static readonly ConcurrentPool<MqttSubackPacket> Pool =
            new ConcurrentPool<MqttSubackPacket>("MQTT SubAck Packets", (c) => new MqttSubackPacket());

        /// <summary>
        /// Acquires a new MQTT packet from the pool.
        /// </summary>
        /// <returns></returns>
        public static MqttSubackPacket Acquire(ushort messageId)
        {
            var packet = Pool.Acquire();
            packet.MessageId = messageId;
            return packet;
        }

        #endregion Static Members

        #region Public Properties

        /// <summary>
        /// The return codes to send with the ack.
        /// </summary>
        public readonly List<QoS> Codes = new List<QoS>();

        /// <summary>
        /// Recycles the packet.
        /// </summary>
        public override void Recycle()
        {
            // Call the base
            base.Recycle();

            // Recycle the properties
            this.Codes.Clear();
        }

        #endregion Public Properties

        #region Read/Write Members

        /// <summary>
        /// Reads the packet from the underlying stream.
        /// </summary>
        /// <param name="buffer">The pointer to start reading at.</param>
        /// <param name="offset">The offset to start at.</param>
        /// <param name="length">The remaining length.</param>
        /// <returns>Whether the packet was parsed successfully or not.</returns>
        public override unsafe MqttStatus TryRead(BufferSegment buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the packet into the provided <see cref="BufferSegment"/>.
        /// </summary>
        /// <param name="segment">The buffer to write into.</param>
        /// <returns>The length written.</returns>
        public override unsafe int TryWrite(ArraySegment<byte> segment)
        {
            // The offset
            var buffer = segment.Array;
            int offset = segment.Offset;

            // Calculate the variable header size
            int headerLength = MESSAGE_ID_SIZE;

            // Calculate the payload size and the remaining length
            int payloadLength = this.Codes.Count;

            // Write the type
            buffer[offset++] = ProtocolVersion == MqttProtocolVersion.V3_1_1
                ? (byte)((MQTT_MSG_SUBACK_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_SUBACK_FLAG_BITS)
                : (byte)(MQTT_MSG_SUBACK_TYPE << MSG_TYPE_OFFSET);

            // Write the length
            WriteLength(headerLength + payloadLength, buffer, ref offset);

            // Write the message id
            buffer[offset++] = (byte)((this.MessageId >> 8) & 0x00FF); // MSB
            buffer[offset++] = (byte)(this.MessageId & 0x00FF); // LSB

            // Write the session present flag
            for (int i = 0; i < this.Codes.Count; ++i)
                buffer[offset++] = (byte)this.Codes[i];

            // Calculate the size of the buffer
            return GetFixedHeaderSize(headerLength + payloadLength)
                + headerLength
                + payloadLength;
        }

        #endregion Read/Write Members
    }
}