using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] retVal = new byte[12];
            retVal[1] = (byte)(CommandParameters.TransactionId);
            retVal[0] = (byte)(CommandParameters.TransactionId >> 8);
            retVal[3] = (byte)(CommandParameters.ProtocolId);
            retVal[2] = (byte)(CommandParameters.ProtocolId >> 8);
            retVal[5] = (byte)(CommandParameters.Length);
            retVal[4] = (byte)(CommandParameters.Length >> 8);
            retVal[6] = CommandParameters.UnitId;
            retVal[7] = CommandParameters.FunctionCode;
            retVal[9] = (byte)(((ModbusReadCommandParameters)CommandParameters).StartAddress);
            retVal[8] = (byte)(((ModbusReadCommandParameters)CommandParameters).StartAddress >> 8);
            retVal[11] = (byte)(((ModbusReadCommandParameters)CommandParameters).Quantity);
            retVal[10] = (byte)(((ModbusReadCommandParameters)CommandParameters).Quantity >> 8);
            return retVal;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            Dictionary<Tuple<PointType, ushort>, ushort> retVal = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters modbusRead = this.CommandParameters as ModbusReadCommandParameters;

            ushort adresa = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
            ushort byte_count = response[8];//duzina
            ushort val;//vrednost koju treba da procitamo

            for (int i = 0; i < byte_count; i += 2)
            {
                val = BitConverter.ToUInt16(response, 9 + i);//pretvaramo niz bitova u unit
                val = (ushort)IPAddress.NetworkToHostOrder((short)val);//jer skidamo sa mreze
                retVal.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, adresa), val);
                adresa++;
            }
            return retVal;
        }
    }
}