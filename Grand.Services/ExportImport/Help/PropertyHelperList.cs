using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;

namespace Grand.Services.ExportImport.Help
{
    public class PropertyHelperList<T>
    {
        private static T obj;

        public PropertyHelperList(T obj1)
        {
            ObjectList = new List<PropertyHelperList<T>>();
            obj = obj1;
        }
        public List<PropertyHelperList<T>> ObjectList { get; set; }

        private string PropertyName { get; set; }
        private object PropertyValue { get; set; }

        public PropertyHelperList(string propertyName, Func<T, object> func = null)
        {
            PropertyName = propertyName;
            PropertyValue = func(obj);
        }

        public List<PropertyList> ToList()
        {
            List<PropertyList> list = new List<PropertyList>();
            foreach (var item in ObjectList)
            {
                list.Add(new PropertyList(item.PropertyName, item.PropertyValue?.ToString()));
            }

            return list;
        }

        public class PropertyList
        {
            public PropertyList(string field, string value)
            {
                Field = field;
                Value = value;
            }
            public string Field { get; set; }
            public string Value { get; set; }
        }

        public void WriteToXlsx(ExcelWorksheet worksheet)
        {
            int row = 2;
            foreach (var prop in ToList())
            {
                worksheet.Cells[row, 1].Value = prop.Field;
                worksheet.Cells[row, 2].Value = prop.Value;
                row++;
            }
        }

        /// <summary>
        /// Write caption (first row) to XLSX worksheet
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="setStyle">Detection of cell style</param>
        public void WriteCaption(ExcelWorksheet worksheet, Action<ExcelStyle> setStyle)
        {
            var cellKey = worksheet.Cells[1, 1];
            cellKey.Value = "Key";
            setStyle(cellKey.Style);
            cellKey.Worksheet.Column(1).Width = 30;
            var cellValue = worksheet.Cells[1, 2];
            cellValue.Value = "Value";
            setStyle(cellValue.Style);
            cellValue.Worksheet.Column(2).Width = 70;
        }

    }
}
