using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Export
{
    [XmlType("sales")]
    public class TestDto
    {
        //[XmlArray("sale")]
        public ExportSalesWithAppliedDiscountDto[] sales { get; set; }

        public string ExportSaleDiscount { get; set; }
    }
}
