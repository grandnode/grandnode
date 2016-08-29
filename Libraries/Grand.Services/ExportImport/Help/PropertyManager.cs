﻿using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Grand.Services.ExportImport.Help
{
    /// <summary>
    /// Class for working with PropertyByName object list
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class PropertyManager<T>
    {
        /// <summary>
        /// All properties
        /// </summary>
        private readonly Dictionary<string, PropertyByName<T>> _properties;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="properties">All acsess properties</param>
        public PropertyManager(PropertyByName<T>[] properties)
        {
            _properties = new Dictionary<string, PropertyByName<T>>();

            var poz = 1;
            foreach (var propertyByName in properties)
            {
                propertyByName.PropertyOrderPosition = poz;
                poz++;
                _properties.Add(propertyByName.PropertyName, propertyByName);
            }
        }

        /// <summary>
        /// Curent object to acsess
        /// </summary>
        public T CurrentObject { get; set; }

        /// <summary>
        /// Return properti index
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public int GetIndex(string propertyName)
        {
            if (!_properties.ContainsKey(propertyName))
                return -1;

            return _properties[propertyName].PropertyOrderPosition;
        }

        /// <summary>
        /// Access object by property name
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        public object this[string propertyName]
        {
            get
            {
                return _properties.ContainsKey(propertyName) && CurrentObject != null
                    ? _properties[propertyName].GetProperty(CurrentObject)
                    : null;
            }
        }

        /// <summary>
        /// Write object data to XLSX worksheet
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="row">Row index</param>
        public void WriteToXlsx(ExcelWorksheet worksheet, int row)
        {
            if (CurrentObject == null)
                return;

            foreach (var prop in _properties.Values)
            {
                worksheet.Cells[row, prop.PropertyOrderPosition].Value = prop.GetProperty(CurrentObject);
            }
        }

        /// <summary>
        /// Read object data from XLSX worksheet
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="row">Row index</param>
        public void ReadFromXlsx(ExcelWorksheet worksheet, int row)
        {
            if (worksheet == null || worksheet.Cells == null)
                return;

            foreach (var prop in _properties.Values)
            {
                prop.PropertyValue = worksheet.Cells[row, prop.PropertyOrderPosition].Value;
            }
        }

        /// <summary>
        /// Write caption (first row) to XLSX worksheet
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="setStyle">Detection of cell style</param>
        public void WriteCaption(ExcelWorksheet worksheet, Action<ExcelStyle> setStyle)
        {
            foreach (var caption in _properties.Values)
            {
                var cell = worksheet.Cells[1, caption.PropertyOrderPosition];
                cell.Value = caption;
                setStyle(cell.Style);
            }

        }

        /// <summary>
        /// Count of properties
        /// </summary>
        public int Count
        {
            get { return _properties.Count; }
        }

        /// <summary>
        /// Get property by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public PropertyByName<T> GetProperty(string propertyName)
        {
            return _properties.ContainsKey(propertyName) ? _properties[propertyName] : null;
        }


        /// <summary>
        /// Get property array
        /// </summary>
        public PropertyByName<T>[] GetProperties
        {
            get { return _properties.Values.ToArray(); }
        }
    }
}