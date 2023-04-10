﻿using iText.Kernel.Geom;
using iText.Layout.Properties;
using MoneyAdministrator.DTOs;
using MoneyAdministrator.DTOs.Enums;
using MoneyAdministrator.Module.ImportHsbcSummary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoneyAdministrator.Module.ImportHsbcSummary
{
    public static class GetValuesFromString
    {
        public static CreditCardSummaryDto GetSummaryData(List<string> lines)
        {
            var ccSummary = new CreditCardSummaryDto();

            GetSummaryVariablesData(ref ccSummary, lines);

            ccSummary.AddDetailDto(GetConsolidatedData(lines));
            ccSummary.AddDetailDto(GetConsumptionsData(lines));
            return ccSummary;
        }

        private static void GetSummaryVariablesData(ref CreditCardSummaryDto ccSummary, List<string> lines)
        {
            lines = CleanContent.GetSummaryPropertiesSectionString(lines);

            for (int i = 0; i < lines.Count; i++)
            {
                //Obtengo la fecha de cierre
                if (i == 0)
                {
                    var date = lines[i].Split(" ").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()[0];
                    ccSummary.Date = DateTimeTools.ConvertToDateTime(date);

                    //Obtengo el periodo
                    int month = 0;
                    if (ccSummary.Date.Day >= 20)
                        month = ccSummary.Date.Month + 1;
                    else
                        month = ccSummary.Date.Month;
                    ccSummary.Period = new DateTime(ccSummary.Date.Year, month, 1);
                }

                //Obtengo la fecha de vencimiento
                if (i == 1)
                {
                    var date = lines[i].Split(" ").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()[0];
                    ccSummary.Expiration = DateTimeTools.ConvertToDateTime(date);
                }

                //Obtengo la proxima fecha de cierre
                if (i == 2)
                {
                    var date = lines[i].Split(" ").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()[3];
                    ccSummary.NextDate = DateTimeTools.ConvertToDateTime(date);
                }

                //Obtengo la proxima fecha de vencimiento y el pago minimo
                if (i == 3)
                {
                    var datePayment = lines[i].Split(" ").ToList().Where(x => !string.IsNullOrEmpty(x)).ToList()[1];

                    //Fecha de vencimiento
                    var date = datePayment.Substring(datePayment.Length - 9);
                    ccSummary.NextExpiration = DateTimeTools.ConvertToDateTime(date);

                    //Pago minimo
                    var minimumPayment = datePayment.Substring(0, datePayment.Length - 9);
                    ccSummary.MinimumPayment = decimalTools.ToDecimal(minimumPayment);
                }
            }
        }

        public static List<CreditCardSummaryDetailDto> GetConsolidatedData(List<string> lines)
        {
            var results = new List<CreditCardSummaryDetailDto>();
            var resultType = CreditCardSummaryDetailDtoType.Summary;

            List<string> data = CleanContent.GetConsolidatedSectionString(lines);

            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i].Contains("SUBTOTAL"))
                {
                    resultType = CreditCardSummaryDetailDtoType.TaxesAndMaintenance;
                    continue;
                }

                var ccSummaryDetail = new CreditCardSummaryDetailDto();
                ccSummaryDetail.Type = resultType;

                //Obtengo la fecha
                if (!data[i].StartsWith(" "))
                {
                    var date = data[i].Substring(0, 9);
                    ccSummaryDetail.Date = DateTimeTools.ConvertToDateTime(date);
                }

                //Elimino el espacio de la fecha
                data[i] = data[i].Substring(22, data[i].Length - 22);

                //Obtengo la descripcion
                int index = data[i].IndexOf("  ");
                ccSummaryDetail.Description = data[i].Substring(0, index);

                //Elimino la descripcion
                data[i] = data[i].Substring(index);

                ccSummaryDetail.Installments = "";

                //Ontengo los montos
                var length = data[i].Length > 12 ? 12 : data[i].Length;
                ccSummaryDetail.AmountArs = decimalTools.ToDecimal(data[i].Substring(0, length));
                ccSummaryDetail.AmountUsd = data[i].Length > 12 ? decimalTools.ToDecimal(data[i].Substring(length)) : 0;

                results.Add(ccSummaryDetail);
            }

            return results;
        }

        public static List<CreditCardSummaryDetailDto> GetConsumptionsData(List<string> lines)
        {
            var results = new List<CreditCardSummaryDetailDto>();
            var resultType = CreditCardSummaryDetailDtoType.Details;

            List<string> data = CleanContent.GetDetailsSectionString(lines);

            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i].Contains("FECHA      COMPRAS DEL MES"))
                {
                    resultType = CreditCardSummaryDetailDtoType.Details;
                    continue;
                }
                else if (data[i].Contains("FECHA      CUOTAS DEL MES"))
                {
                    resultType = CreditCardSummaryDetailDtoType.Installments;
                    continue;
                }
                else if (data[i].Contains("FECHA      DEBITOS AUTOMATICOS"))
                {
                    resultType = CreditCardSummaryDetailDtoType.AutomaticDebits;
                    continue;
                }

                var ccSummaryDetail = new CreditCardSummaryDetailDto();

                if (resultType == CreditCardSummaryDetailDtoType.Details || resultType == CreditCardSummaryDetailDtoType.AutomaticDebits)
                    ccSummaryDetail = GetDetailsDto(data[i], resultType);
                else if (resultType == CreditCardSummaryDetailDtoType.Installments)
                    ccSummaryDetail = GetInstallmentsDto(data[i], resultType);


                results.Add(ccSummaryDetail);
            }

            return results;
        }

        private static CreditCardSummaryDetailDto GetDetailsDto(string line, CreditCardSummaryDetailDtoType type)
        {
            var ccSummaryDetail = new CreditCardSummaryDetailDto();
            ccSummaryDetail.Type = type;

            //Obtengo la fecha
            if (!line.StartsWith(" "))
            {
                var date = line.Substring(0, 9);
                ccSummaryDetail.Date = DateTimeTools.ConvertToDateTime(date);
            }

            //Elimino el espacio de la fecha
            line = line.Substring(10);

            //Obtengo la descripcion
            ccSummaryDetail.Description = line.Substring(0, 38).TrimEnd();

            //Elimino el espacio de la descripcion
            line = line.Substring(45);

            //Asigno un valor vacio para no dejarlo null
            ccSummaryDetail.Installments = "";

            //Obtengo los montos
            var length = line.Length > 12 ? 12 : line.Length;
            ccSummaryDetail.AmountArs = decimalTools.ToDecimal(line.Substring(0, length));
            ccSummaryDetail.AmountUsd = line.Length > 12 ? decimalTools.ToDecimal(line.Substring(length)) : 0;

            return ccSummaryDetail;
        }

        private static CreditCardSummaryDetailDto GetInstallmentsDto(string line, CreditCardSummaryDetailDtoType type)
        {
            var ccSummaryDetail = new CreditCardSummaryDetailDto();
            ccSummaryDetail.Type = type;

            //Obtengo la fecha
            if (!line.StartsWith(" "))
            {
                var date = line.Substring(0, 9);
                ccSummaryDetail.Date = DateTimeTools.ConvertToDateTime(date);
            }
            line = line.Substring(10); //Elimino el espacio de la fecha

            //Obtengo la descripcion
            ccSummaryDetail.Description = line.Substring(0, 34).TrimEnd();
            line = line.Substring(34); //Elimino el espacio de la descripcion

            //Obtengo las cuotas
            ccSummaryDetail.Installments = line.Substring(0, 5);
            //Antes de eliminar el espacio, compruebo si tengo codigo de cupon
            bool haveCupon = false;

            if (line[7] != ' ')
                haveCupon = true;

            if (haveCupon)
                line = line.Substring(11); //Elimino el espacio de las cuotas y cupon
            else
                line = line.Substring(5); //Elimino el espacio de las cuotas

            //Obtengo los montos
            var length = line.Length > 12 ? 12 : line.Length;
            ccSummaryDetail.AmountArs = decimalTools.ToDecimal(line.Substring(0, length));
            ccSummaryDetail.AmountUsd = line.Length > 12 ? decimalTools.ToDecimal(line.Substring(length)) : 0;

            return ccSummaryDetail;
        }
    }
}
