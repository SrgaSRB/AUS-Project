using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] request = new byte[12];

            ModbusReadCommandParameters modbusRead = this.CommandParameters as ModbusReadCommandParameters;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)modbusRead.TransactionId)), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)modbusRead.ProtocolId)), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)modbusRead.Length)), 0, request, 4, 2);
            request[6] = modbusRead.UnitId;
            request[7] = modbusRead.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)modbusRead.StartAddress)), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)modbusRead.Quantity)), 0, request, 10, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)//recnik ciji je kljc par tip, adresa a vrednost je value,
        {
            ModbusReadCommandParameters modbusRead = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> dic = new Dictionary<Tuple<PointType, ushort>, ushort>();
            int count = 0;

            ushort adresa = modbusRead.StartAddress;
            ushort value;
            byte maska = 1;

            for (int i = 0; i < response[8]; i++)
            {
                byte tempByte = response[9 + i];
                for (int j = 0; j < 8; j++)
                {
                    value = (ushort)(tempByte & maska);
                    tempByte >>= 1;
                    dic.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, adresa), value);//pakujemo u recnik 
                    count++;
                    adresa++;
                    if (count == modbusRead.Quantity)
                    {
                        break;
                    }
                }
            }
            return dic;
        }
    }
}