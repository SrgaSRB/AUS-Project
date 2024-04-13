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
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)//prevodimo odgovor u citljiv format,
                                                                                                   //ogovor se salje posle pribavljanja requesta
                                                                                                   //niz bajtova prevodi u recnik ciji je kljc par tip, adresa a vrednost je value,
                                                                                                   //koji je dosao u tom odgovoru
        {
            ModbusReadCommandParameters modbusRead = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> dic = new Dictionary<Tuple<PointType, ushort>, ushort>();
            int count = 0;//njega proveravamo da li je isti kao quantity sto je br bitova koje treba procitati iz requesta

            ushort adresa = modbusRead.StartAddress;//adresa od koje krece citanje

            ushort value;//vrednost koja treba da se procita 
            byte maska = 1;//maska zbog ocitavnja bota

            for (int i = 0; i < response[8]; i++)
            {//posto je digitalan npr 10 bitova stane u 2 bajta
             //response[8] je br bajtova odgovora
                byte tempByte = response[9 + i];//prvi bit u odgovoru
                for (int j = 0; j < 8; j++)
                {//svaki bajt ima 8 bita i sad se pomeramo po tim bitovima kroz bajt
                    value = (ushort)(tempByte & maska);//ako je 1 ostaje 1 ako je 0 onda bude 0, zato se and-uje
                    tempByte >>= 1;//pomeramo se na sledeci bit u bajtu
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