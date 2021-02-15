using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace AtacFeed
{
    public class RegolaAlert
    {
        public string Linea { get; set; }
        public string Giorno { get; set; }
        public TimeSpan? Da { get; set; }
        public TimeSpan? A { get; set; }
        public int? VetturaDa { get; set; }
        public int? VetturaA { get; set; }

        public RegolaAlert() { }

        public RegolaAlert(string linea, string giorno, TimeSpan? da, TimeSpan? a, int? vetturaDa, int? vetturaA)
        {
            Linea = linea;
            Giorno = giorno;
            Da = da;
            A = a;
            VetturaDa = vetturaDa;
            VetturaA = vetturaA;
        }

    }

    public class MyBooleanConverter : BooleanConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return string.IsNullOrEmpty(text) || text == "+";
        }
    }

    public sealed class RegolaAlertMap : ClassMap<RegolaAlert>
    {
        public RegolaAlertMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }
}
