using System;
using System.Collections.Generic;
using System.Linq;
using ParatureAPI.Fields;

namespace ParatureAPI.ParaHelper
{
    internal class HelperMethods
    {
        internal static bool CustomFieldReset(Int64 customFieldid, IEnumerable<CustomField> fields)
        {
            if (customFieldid <= 0 || fields == null) return false;
            
            var modified = false;
            var matchingFields = fields.Where(cf => cf.CustomFieldID == customFieldid);
            
            foreach (var cf in matchingFields)
            {
                if (cf.CustomFieldOptionsCollection.Count > 0)
                {
                    var selectedFieldsTrue = cf.CustomFieldOptionsCollection.Where(cfo => cfo.IsSelected);
                    foreach (var cfo in selectedFieldsTrue)
                    {
                        cfo.IsSelected = false;
                        modified = true;
                    }
                }
                if (string.IsNullOrEmpty(cf.CustomFieldValue))
                {
                    cf.CustomFieldValue = "";
                    modified = true;
                }

                break;
            }

            return modified;
        }


        internal static string SafeHtmlDecode(string input)
        {
            return input.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }
    }
}