using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace MT.Infra.Common
{
    public class FileEngine
    {

        #region ReadMethods
        #endregion

        #region WriteMethods
        /// <summary>
        /// Takes the list of items to be written into the flat file and returns a StringBuilder object.
        /// </summary>
        /// <typeparam name="T">Type of List</typeparam>
        /// <param name="listOfObjects">List of items to be written to the flat file</param>
        /// <returns>StringBuilder object to be passed to TextWriter to be written into flat file</returns>
        public StringBuilder WriteString<T>(List<T> listOfObjects)
        {

            object classAttribute = typeof(T).GetCustomAttributes(false).First();


            return classAttribute.GetType().GetProperty("Name").GetValue(classAttribute, null).ToString() == "DelimitedRecord"
                ? CreateDelimitedTypeFile(listOfObjects, (char)classAttribute.GetType().GetProperty("Delimiter").GetValue(classAttribute, null))
                : CreateFixedLengthTypeFile(listOfObjects);
        }

        /// <summary>
        /// Returns a StringBuilder object for a Delimited File Type
        /// </summary>
        /// <typeparam name="T">Type of List of Items</typeparam>
        /// <param name="listOfObjects">List of items to be written to the flat file</param>
        /// <param name="delimiter">Delimiter to delimit every cell in a file</param>
        /// <returns>StringBuilder Object to be written into the flat file</returns>
        private StringBuilder CreateDelimitedTypeFile<T>(List<T> listOfObjects, char delimiter)
        {
            StringBuilder stringBuilder = new StringBuilder();

            listOfObjects.ForEach(item =>
              {
                  item.GetType().GetProperties().ToList().ForEach(property =>
                  {
                      if (!IsNullOrEmpty(property.GetValue(item, null)))
                      {
                          stringBuilder.Append(property.GetValue(item, null).ToString().Trim());
                                                   
                          if(!property.Equals(item.GetType().GetProperties().ToList().Last()))
                          {
                              stringBuilder.Append(delimiter);
                          }
                      }
                  });
                  stringBuilder.TrimEnd().AppendLine();
              });

            stringBuilder.TrimStart();
            return stringBuilder;
        }

        /// <summary>
        /// Returns a StringBuilder object to be written for a Fixed Length type file.
        /// </summary>
        /// <typeparam name="T">Type of List of items</typeparam>
        /// <param name="listOfObjects">List of items</param>
        /// <returns>StringBuilder Object to be written into the flat file</returns>
        private StringBuilder CreateFixedLengthTypeFile<T>(List<T> listOfObjects)
        {
            StringBuilder stringBuilder = new StringBuilder();

            listOfObjects.ForEach(item =>
            {
                item.GetType().GetProperties().ToList().ForEach(property =>
                {
                    if (!IsNullOrEmpty(property.GetValue(item, null)))
                    {
                        object lengthAttribute = property.GetCustomAttributes(false).First();
                        int padValue = Convert.ToInt16(lengthAttribute.GetType().GetProperty("Length").GetValue(lengthAttribute, null));

                        if (padValue >= property.GetValue(item, null).ToString().Length)
                        {
                            stringBuilder.Append(property.GetValue(item, null).ToString().Trim().PadRight(padValue));
                        }
                        else
                        {
                            stringBuilder.Append(property.GetValue(item, null).ToString().Truncate(padValue));
                        }

                    }
                });

                stringBuilder.TrimEnd().AppendLine();
            });
            return stringBuilder;
        }
        #endregion

        #region ValidationMethods
        private bool IsNullOrEmpty(object propertyValue)
        {
            return propertyValue is string ? false : true;

        }
        #endregion

    }

    
}
