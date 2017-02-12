using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public static class CollectionExtensions
    {
        /****************************************************************************
         * If json is posted to a web form then the HttpRequest.Form collection keys 
         *   looks like this:
         *   
         *      StoreNumber
         *      StoreName
         *      Vehicles[0].Make
         *      Vehicles[1].Make
         *      Equipment[TireStation]
         *      Equipment[Lift]
         * 
         *   When the posted JSON is this:
         *   
         *      {
         *          StoreNumber: 1000,
         *          StoreName:   MidTown
         *          Vehicles:
         *          [
         *            {
         *              Make: "Chevy"
         *            },
         *            {
         *              Make: "Pontiac"
         *            }
         *          ],
         *          Equipment:
         *          {
         *            TireStation: true,
         *            Lift: true,
         *          }
         *      }
         *      
         ****************************************************************************/
        public static XmlDocument ToXml(this NameValueCollection collection)
        {
            XmlDocument xmlResult = new XmlDocument();
            XmlNode     xmlRoot   = xmlResult.AddChild("Data");

            foreach(string key in collection.AllKeys)
            {
                string     value  = collection[key].Trim();
                StringList parts  = new StringList(key, "[", false);
                XmlNode    xmlNew = GetFormNode(parts, 0, xmlRoot);

                xmlNew.InnerText = value;
            }

            return xmlResult;
        }

        /****************************************************************************/
        public static string Pack(this IEnumerable<object> list, string separator, Func<object, string> fn)
        {
            StringBuilder sb = new StringBuilder();

            foreach(object item in list)
            {
                if(sb.Length != 0)
                    sb.Append(separator);

                sb.Append(fn(item));
            }
   
            return(sb.ToString());
        }

        /****************************************************************************/
        private static XmlNode GetFormNode(StringList parts, int index, XmlNode parent, Dictionary<string, string> pluralTransforms = null)
        {
            XmlNode node  = parent;
            string  prefix = "//";

            while(index < parts.Count)
            { 
                string part = parts[index];

                if(part.IndexOf("]") != -1)
                {
                    part = part.Substring(0, part.Length - 1).Trim();

                    if(part == "")
                      break;

                    int arrayIndex = 0;

                    if(int.TryParse(part, out arrayIndex))
                    {
                        part = parent.LocalName;

                        if(pluralTransforms != null && pluralTransforms.ContainsKey(part))
                          part = pluralTransforms[part];
                        else if(part.EndsWith("ies")) // English only hack!! :-(
                          part = part.Substring(0, part.Length - 3) + "y";
                        else if(part.EndsWith("s")) // English only hack!! :-(
                          part = part.Substring(0, part.Length - 1);

                        if(parent.ChildNodes.Count <= arrayIndex)
                            node = parent.AddChild(part);
                        else
                            node = parent.ChildNodes[arrayIndex];
                    }
                    else
                        node = GetNode(parent, part, prefix);
                }
                else
                    node = GetNode(parent, part, prefix);

                parent = node;
                prefix = "";
                ++index;
            }

            return node;
        }

        /****************************************************************************/
        private static XmlNode GetNode(XmlNode parent, string part, string prefix)
        {
            XmlNode node = parent.SelectSingleNode(prefix + part);
            
            if(node == null)
                node = parent.AddChild(part);

            return node;
        }

    }
}
