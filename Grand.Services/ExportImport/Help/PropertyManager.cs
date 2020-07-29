using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Linq;

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

            var poz = 0;
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
        public object this[string propertyName] {
            get {
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
        public void WriteToXlsx(ISheet sheet, int row)
        {
            if (CurrentObject == null)
                return;

            IRow _row = sheet.CreateRow(row);
            foreach (var prop in _properties.Values)
            {
                _row.CreateCell(prop.PropertyOrderPosition).SetCellValue(prop.GetProperty(CurrentObject)?.ToString());
            }
        }

        /// <summary>
        /// Read object data from XLSX worksheet
        /// </summary>
        /// <param name="ISheet">sheet</param>
        /// <param name="row">Row index</param>
        public void ReadFromXlsx(ISheet sheet, int row)
        {
            if (sheet == null)
                return;

            
            var _row = sheet.GetRow(row);
            foreach (var prop in _properties.Values)
            {
                var cell = _row.GetCell(prop.PropertyOrderPosition);
                prop.PropertyValue = cell?.ToString();
            }
        }

        /// <summary>
        /// Write caption (first row) to XLSX worksheet
        /// </summary>
        /// <param name="ISheet">sheet</param>
        public void WriteCaption(ISheet sheet)
        {
            IRow row = sheet.CreateRow(0);
            foreach (var caption in _properties.Values)
            {
                row.CreateCell(caption.PropertyOrderPosition).SetCellValue(caption.PropertyName);
            }
        }

        /// <summary>
        /// Count of properties
        /// </summary>
        public int Count {
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
        public PropertyByName<T>[] GetProperties {
            get { return _properties.Values.ToArray(); }
        }
    }
}