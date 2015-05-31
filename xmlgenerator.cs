using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualBasic;
using ParatureSDK.Fields;
using ParatureSDK.ParaObjects;
using ParatureSDK.ParaObjects.EntityReferences;
using Action = ParatureSDK.ParaObjects.Action;

namespace ParatureSDK
{
    internal class XmlGenerator
    {
        //WIP - not production ready
        static internal XmlDocument GenerateXml(ParaEntity entity)
        {
            var entityType = entity.GetType().ToString();
            var doc = new XmlDocument();
            var rootNode = doc.CreateElement(entityType);
            if (entity.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = entity.Id.ToString();
                rootNode.Attributes.Append(attribute);
            }

            foreach (var sf in entity.StaticFields)
            {
                var fieldVal = sf.Value;

                //generate the nested XML for entity references
                if (fieldVal is EntityReference<ParaEntity>)
                {
                    var entRef = fieldVal as EntityReference<ParaEntity>;
                    var entityRefType = entRef.Entity.GetType().ToString();
                    XmlGenerateComplexEntityNode(doc, rootNode, sf.Name, entityRefType, "id", entRef.Entity.Id.ToString());
                }

                //List fields
                //TODO: Need to use the XmlAttributes, since class types may not match the item node name (ex CsrRole vs Role)
                if (fieldVal is List<ParaEntityBaseProperties>)
                {
                    var sfList = sf.Value as List<ParaEntityBaseProperties>;
                    var node = doc.CreateElement(sf.Name);

                    if (sfList != null && sfList.Count > 0)
                    {
                        //Handle the very specific download folders scenario
                        if (entity is Download && sf.Name == "Folders")
                        {
                            node = GenerateDownloadFoldersNode(entity as Download, doc, rootNode);
                            rootNode.AppendChild(node);
                        }
                        else
                        {
                            foreach (var ent in sfList)
                            {
                                var nodeName = ent.GetType().Name;
                                var nodechild = doc.CreateElement(nodeName);
                                var attribute = doc.CreateAttribute("id");
                                attribute.Value = ent.Id.ToString();
                                nodechild.Attributes.Append(attribute);
                                node.AppendChild(nodechild);
                            }
                        }

                        rootNode.AppendChild(node);
                    }
                }

                //for simple types, a tostring suffices
                if (fieldVal is Int32
                    || fieldVal is Int64
                    || fieldVal is string
                    || fieldVal is bool)
                {
                    XmlGenerateElement(doc, rootNode, sf.Name, sf.Value.ToString());
                }
            }

            ObjectGenerateCustomFieldsXml(doc, rootNode, entity.CustomFields);
            doc.AppendChild(rootNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the account object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(Account obj)
        {
            // TODO viewable accounts?
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("Account");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }


            if (string.IsNullOrEmpty(obj.Account_Name) == false)
            {
                XmlGenerateElement(doc, objNode, "Account_Name", obj.Account_Name);
            }
            if (obj.Sla != null && obj.Sla.Sla != null && obj.Sla.Sla.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Sla", "id", obj.Sla.Sla.Id.ToString());
            }

            if (obj.Viewable_Account != null && obj.Viewable_Account.Count > 0)
            {
                XmlNode node = doc.CreateElement("Shown_Accounts");
                foreach (var vAccount in obj.Viewable_Account)
                {
                    var nodechild = doc.CreateElement("Account");
                    var attribute = doc.CreateAttribute("id");
                    attribute.Value = vAccount.Id.ToString();
                    nodechild.Attributes.Append(attribute);
                    node.AppendChild(nodechild);
                }
                objNode.AppendChild(node);
            }


            if (obj.Default_Customer_Role != null && obj.Default_Customer_Role.Role != null && obj.Default_Customer_Role.Role.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Default_Customer_Role", "CustomerRole", "id", obj.Default_Customer_Role.Role.Id.ToString());
            }

            if (obj.CustomFields.Any())
            {
                ObjectGenerateCustomFieldsXml(doc, objNode, obj.CustomFields);
            }
            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the Contact object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(Customer obj)
        {
            var doc = new XmlDocument();
            XmlNode objNode = doc.CreateElement("Customer");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            if (string.IsNullOrEmpty(obj.First_Name) == false)
            {
                XmlGenerateElement(doc, objNode, "First_Name", obj.First_Name);
            }
            if (string.IsNullOrEmpty(obj.Last_Name) == false)
            {
                XmlGenerateElement(doc, objNode, "Last_Name", obj.Last_Name);
            }
            if (string.IsNullOrEmpty(obj.Email) == false)
            {
                XmlGenerateElement(doc, objNode, "Email", obj.Email);
            }
            if (string.IsNullOrEmpty(obj.User_Name) == false)
            {
                XmlGenerateElement(doc, objNode, "User_Name", obj.User_Name);
            }

            if (string.IsNullOrEmpty(obj.Password) == false)
            {
                XmlGenerateElement(doc, objNode, "Password", obj.Password);
            }
            if (string.IsNullOrEmpty(obj.Password_Confirm) == false)
            {
                XmlGenerateElement(doc, objNode, "Password_Confirm", obj.Password_Confirm);
            }
            else if (string.IsNullOrEmpty(obj.Password) == false)
            {
                XmlGenerateElement(doc, objNode, "Password_Confirm", obj.Password);
            }

            if (obj.Sla != null && obj.Sla.Sla != null && obj.Sla.Sla.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Sla", "id", obj.Sla.Sla.Id.ToString());
            }

            if (obj.Customer_Role != null && obj.Customer_Role.Role != null && obj.Customer_Role.Role.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Customer_Role", "CustomerRole", "id", obj.Customer_Role.Role.Id.ToString());
            }

            if (obj.Account != null && obj.Account.Entity != null && obj.Account.Entity.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Account", "id", obj.Account.Entity.Id.ToString());
            }

            if (obj.Status != null && obj.Status.Status != null && obj.Status.Status.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Status", "id", obj.Status.Status.Id.ToString());
            }

            ObjectGenerateCustomFieldsXml(doc, objNode, obj.CustomFields);

            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the Asset object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(Asset obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("Asset");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            if (string.IsNullOrEmpty(obj.Name) == false)
            {
                XmlGenerateElement(doc, objNode, "Name", obj.Name);
            }

            if (obj.Status != null && obj.Status.Status != null && obj.Status.Status.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Status", "id", obj.Status.Status.Id.ToString());
            }

            if (obj.Customer_Owner != null && obj.Customer_Owner.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Customer_Owner", "Customer", "id", obj.Customer_Owner.Entity.Id.ToString());
            }
            else if (obj.Account_Owner!= null && obj.Account_Owner.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Account_Owner", "Account", "id", obj.Account_Owner.Entity.Id.ToString());
            }

            if (obj.Product != null && obj.Product.Entity != null && obj.Product.Entity.Id > 0)
            {
                XmlGenerateEntityNode(doc, objNode, "Product", "id", obj.Product.Entity.Id.ToString());
            }
            if (string.IsNullOrEmpty(obj.Serial_Number) == false)
            {
                XmlGenerateElement(doc, objNode, "Serial_Number", obj.Serial_Number);
            }
            ObjectGenerateCustomFieldsXml(doc, objNode, obj.CustomFields);
            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the Ticket object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(Ticket obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("Ticket");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            if (obj.Cc_Csr != null)
            {
                XmlGenerateElementFromArray(doc, objNode, "Cc_Csr", new ArrayList() { obj.Cc_Csr }, ",");
            }
            if (obj.Cc_Customer != null)
            {
                XmlGenerateElementFromArray(doc, objNode, "Cc_Customer", new ArrayList() { obj.Cc_Customer }, ",");
            }

            if (obj.Ticket_Product != null && obj.Ticket_Product.Entity != null && obj.Ticket_Product.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Ticket_Product", "Product", "id", obj.Ticket_Product.Entity.Id.ToString());
            }

            if (obj.Ticket_Asset != null && obj.Ticket_Asset.Entity != null && obj.Ticket_Asset.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Ticket_Asset", "Asset", "id", obj.Ticket_Asset.Entity.Id.ToString());
            }

            if (obj.Ticket_Sla != null && obj.Ticket_Sla.Sla != null && obj.Ticket_Sla.Sla.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Ticket_Sla", "Sla", "id", obj.Ticket_Sla.Sla.Id.ToString());
            }

            if (obj.Email_Notification != null)
            {
                XmlGenerateElement(doc, objNode, "Email_Notification", obj.Email_Notification.ToString().ToLower());
            }

            if (obj.Email_Notification_Additional_Contact != null)
            {
                XmlGenerateElement(doc, objNode, "Email_Notification_Additional_Contact", obj.Email_Notification_Additional_Contact.ToString().ToLower());
            }

            if (obj.Ticket_Customer != null && obj.Ticket_Customer.Entity != null && obj.Ticket_Customer.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Ticket_Customer", "Customer", "id", obj.Ticket_Customer.Entity.Id.ToString());
            }

            if (obj.Hide_From_Customer != null)
            {
                XmlGenerateElement(doc, objNode, "Hide_From_Customer", obj.Hide_From_Customer.ToString().ToLower());
            }

            if (obj.Additional_Contact != null && obj.Additional_Contact.Entity != null && obj.Additional_Contact.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Additional_Contact", "Customer", "id", obj.Additional_Contact.Entity.Id.ToString());
            }

            if (obj.Department != null && obj.Department.Department != null && obj.Department.Department.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Department", "Department", "id", obj.Department.Department.Id.ToString());
            }

            if (obj.Ticket_Parent != null && obj.Ticket_Parent.Entity != null && obj.Ticket_Parent.Entity.Id > 0)
            {
                XmlGenerateComplexEntityNode(doc, objNode, "Ticket_Parent", "Ticket", "id", obj.Ticket_Parent.Entity.Id.ToString());
            }

            //Sending back the Child tickets XML.
            if (obj.Ticket_Children != null)
            {
                if (obj.Ticket_Children.Count > 0)
                {
                    var mainnode = doc.CreateElement("Ticket_Children");
                    foreach (var tc in obj.Ticket_Children)
                    {
                        var mainnodechild = doc.CreateElement("Ticket");
                        var attribute = doc.CreateAttribute("id");
                        attribute.Value = tc.Id.ToString();
                        mainnodechild.Attributes.Append(attribute);
                        mainnode.AppendChild(mainnodechild);
                    }
                    objNode.AppendChild(mainnode);
                }
            }
            if (obj.Ticket_Attachments != null && obj.Ticket_Attachments.Count > 0)
            {
                ObjectGenerateAttachmentNodes(doc, objNode, "Ticket_Attachments", "Attachment", obj.Ticket_Attachments);
            }

            ObjectGenerateCustomFieldsXml(doc, objNode, obj.CustomFields);
            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// Generate the XML needed to run an action.
        /// </summary>
        static public XmlDocument GenerateXml(Action obj, ParaEnums.ParatureModule module)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("Action");
            var actionWrapperNode = doc.CreateElement(module.ToString());
            var actionNode = doc.CreateElement("Action");
            var attribute = doc.CreateAttribute("id");
            attribute.Value = obj.Id.ToString();

            objNode.Attributes.Append(attribute);


            if (obj.TimeSpentHours > 0)
            {
                XmlGenerateElement(doc, objNode, "TimeSpentHours", obj.TimeSpentHours.ToString());
            }

            if (obj.TimeSpentMinutes > 0)
            {
                XmlGenerateElement(doc, objNode, "TimeSpentMinutes", obj.TimeSpentMinutes.ToString());
            }

            if (obj.EmailCustList != null)
            {
                if (obj.EmailCustList.Count > 0)
                {
                    XmlGenerateElementFromArray(doc, objNode, "Emailcustlist", obj.EmailCustList, ",");
                }
            }
            if (obj.EmailCsrList != null)
            {
                if (obj.EmailCsrList.Count > 0)
                {
                    XmlGenerateElementFromArray(doc, objNode, "EmailCsrList", obj.EmailCsrList, ",");
                }
            }

            if (module == ParaEnums.ParatureModule.Ticket)
            {
                XmlGenerateElement(doc, objNode, "ShowToCust", obj.ShowToCust.ToString().ToLower());
            }


            if (obj.Comment != null)
            {
                if (string.IsNullOrEmpty(obj.Comment) == false)
                {
                    XmlGenerateElement(doc, objNode, "Comment", obj.Comment);
                }
            }
            if (obj.EmailText != null)
            {
                if (string.IsNullOrEmpty(obj.EmailText) == false)
                {
                    XmlGenerateElement(doc, objNode, "Emailtext", obj.EmailText);
                }
            }
            if (obj.AssignToQueue != null && obj.AssignToQueue > 0)
            {
                XmlGenerateElement(doc, objNode, "AssignToQueue", obj.AssignToQueue.ToString());
            }
            if (obj.AssignToCsr != null && obj.AssignToCsr > 0)
            {
                XmlGenerateElement(doc, objNode, "AssignToCsr", obj.AssignToCsr.ToString());
            }

            actionNode.AppendChild(objNode);
            actionWrapperNode.AppendChild(actionNode);
            doc.AppendChild(actionWrapperNode);
            return doc;
        }

        /// <summary>
        /// Generate the XML needed to create/update a product.
        /// </summary>
        static public XmlDocument GenerateXml(Product obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("Product");

            if (string.IsNullOrEmpty(obj.Currency) == false)
            {
                XmlGenerateElement(doc, objNode, "Currency", obj.Currency.ToString());
            }
            if (string.IsNullOrEmpty(obj.Longdesc) == false)
            {
                XmlGenerateElement(doc, objNode, "Longdesc", obj.Longdesc.ToString());
            }
            if (string.IsNullOrEmpty(obj.Name) == false)
            {
                XmlGenerateElement(doc, objNode, "Name", obj.Name.ToString());
            }
            if (string.IsNullOrEmpty(obj.Shortdesc) == false)
            {
                XmlGenerateElement(doc, objNode, "Shortdesc", obj.Shortdesc.ToString());
            }
            if (string.IsNullOrEmpty(obj.Price) == false)
            {
                XmlGenerateElement(doc, objNode, "Price", obj.Price.ToString());
            }
            if (string.IsNullOrEmpty(obj.Sku) == false)
            {
                XmlGenerateElement(doc, objNode, "Sku", obj.Sku.ToString());
            }
            if (obj.Visible != null)
            {
                XmlGenerateElement(doc, objNode, "Visible", obj.Visible.ToString().ToLower());
            }
            if (obj.InStock != null)
            {
                XmlGenerateElement(doc, objNode, "Instock", obj.InStock.ToString().ToLower());
            }
            if (obj.Folder != null && obj.Folder.ProductFolder != null)
            {
                if (obj.Folder.ProductFolder.Id > 0)
                {
                    XmlGenerateComplexEntityNode(doc, objNode, "Folder", "ProductFolder", "id", obj.Folder.ProductFolder.Id.ToString());
                }
            }

            ObjectGenerateCustomFieldsXml(doc, objNode, obj.CustomFields);

            doc.AppendChild(objNode);
            return doc;
        }

        private static XmlElement GenerateDownloadFoldersNode(Download obj, XmlDocument doc, XmlElement objNode)
        {
            if (!obj.MultipleFolders && obj.Folders.Count > 1)
            {
                throw new ArgumentOutOfRangeException("Folders",
                    "There are too many folders for this Download. MultipleFolders is set to false.");
            }

            //Need to handle multiple folders
            XmlElement node;
            if (obj.MultipleFolders)
            {
                node = doc.CreateElement("Folders");
                foreach (var folder in obj.Folders)
                {
                    var nodechild = doc.CreateElement("DownloadFolder");
                    var attribute = doc.CreateAttribute("id");
                    attribute.Value = folder.Id.ToString();
                    nodechild.Attributes.Append(attribute);
                    node.AppendChild(nodechild);
                }
            }
            else
            {
                node = doc.CreateElement("Folder");
                var folder = obj.Folders.FirstOrDefault();
                var nodechild = doc.CreateElement("DownloadFolder");
                var attribute = doc.CreateAttribute("id");
                attribute.Value = folder.Id.ToString();
                nodechild.Attributes.Append(attribute);
                node.AppendChild(nodechild);
            }

            objNode.AppendChild(node);
            return node;
        }

        /// <summary>
        /// This methods requires the DownloadFolder object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(DownloadFolder obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("DownloadFolder");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            XmlGenerateElement(doc, objNode, "Is_Private", obj.Is_Private.ToString().ToLower());
            XmlGenerateElement(doc, objNode, "Name", obj.Name.ToString());
            XmlGenerateElement(doc, objNode, "Description", obj.Description.ToString());

            XmlGenerateComplexEntityNode(doc, objNode, "Parent_Folder", "DownloadFolder", "id", obj.Parent_Folder.Id.ToString());
            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the ArticleFolder object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(ArticleFolder obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("ArticleFolder");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            XmlGenerateElement(doc, objNode, "Name", obj.Name.ToString());
            XmlGenerateElement(doc, objNode, "Is_Private", obj.Is_Private.ToString().ToLower());

            XmlGenerateComplexEntityNode(doc, objNode, "Parent_Folder", "ArticleFolder", "id", obj.Parent_Folder.Id.ToString());
            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// This methods requires the ProductFolder object to be inserted/updated, and returns the XML to be posted to the APIs Server
        /// </summary>
        static public XmlDocument GenerateXml(ProductFolder obj)
        {
            var doc = new XmlDocument();
            var objNode = doc.CreateElement("ProductFolder");
            if (obj.Id > 0)
            {
                var attribute = doc.CreateAttribute("id");
                attribute.Value = obj.Id.ToString();
                objNode.Attributes.Append(attribute);
            }

            XmlGenerateElement(doc, objNode, "Is_Private", obj.Is_Private.ToString().ToLower());
            XmlGenerateElement(doc, objNode, "Name", obj.Name.ToString());
            XmlGenerateElement(doc, objNode, "Description", obj.Description.ToString());
            XmlGenerateComplexEntityNode(doc, objNode, "Parent_Folder", "ProductFolder", "id", obj.Parent_Folder.Id.ToString());

            doc.AppendChild(objNode);
            return doc;
        }

        /// <summary>
        /// An internal method that generates a node and apprend it to the xmldocument root element passed to it.
        /// </summary>       
        static void XmlGenerateElement(XmlDocument doc, XmlNode objNode, string nodename, string nodevalue)
        {
            XmlNode node = doc.CreateElement(nodename);
            node.InnerText = nodevalue;
            objNode.AppendChild(node);
        }

        /// <summary>
        /// An internal method that generates a CData node and apprend it to the xmldocument root element passed to it.
        /// </summary>       
        static void XmlGenerateCDataElement(XmlDocument doc, XmlNode objNode, string nodename, string nodevalue)
        {
            if (nodevalue.Contains("]]>"))
            {
                XmlGenerateElement(doc, objNode, nodename, nodevalue);
            }
            else
            {
                var CData = doc.CreateCDataSection(nodevalue);
                XmlNode node = doc.CreateElement(nodename);
                node.AppendChild(CData);
                objNode.AppendChild(node);
            }
        }

        /// <summary>
        /// An internal method that generates a node from a string array and apprend it to the xmldocument root element passed to it.
        /// </summary>
        static void XmlGenerateElementFromArray(XmlDocument doc, XmlNode objNode, string nodename, ArrayList nodevalue, string separator)
        {
            var node = doc.CreateElement(nodename);
            var value = "";
            var lastvalue = false;

            for (var i = 0; i < nodevalue.Count; i++)
            {
                lastvalue = false;
                if (i == nodevalue.Count - 1)
                {
                    lastvalue = true;
                }
                if (lastvalue == false)
                {
                    value = value + nodevalue[i].ToString() + separator;
                }
                else
                {
                    value = value + nodevalue[i].ToString();
                }
            }

            node.InnerText = value;
            objNode.AppendChild(node);

        }

        /// <summary>
        /// An internal method that generates a complex entity node with an external name and an internal element name to apprend it to the xmldocument root element passed to it.
        /// </summary>
        static void XmlGenerateComplexEntityNode(XmlDocument doc, XmlNode objNode, string externalNodeName, string internalNodeName, string attributeName, string attributeValue)
        {

            var node = doc.CreateElement(externalNodeName);
            var nodechild = doc.CreateElement(internalNodeName);

            var attribute = doc.CreateAttribute(attributeName);
            attribute.Value = attributeValue;

            nodechild.Attributes.Append(attribute);
            node.AppendChild(nodechild);
            objNode.AppendChild(node);

        }

        /// <summary>
        /// Generates the whole attachments node that contains the attachment collection details.
        /// Useful for objects that may contain multiple attachments (ticket, action, etc).
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="objNode"></param>
        /// <param name="externalAttachmentNodeName">
        /// The external node name for the attachment collection. For example: "Ticket_Attachment" for ticket attachments.
        /// </param>
        /// <param name="attachmentNodeName">
        /// This would be the node name for each attachment in the collection.
        /// </param>
        /// <param name="attachments"></param>
        static void ObjectGenerateAttachmentNodes(XmlDocument doc, XmlNode objNode, string externalAttachmentNodeName, string attachmentNodeName, List<Attachment> attachments)
        {
            var node = doc.CreateElement(externalAttachmentNodeName);
            foreach (var at in attachments)
            {
                ObjectGenerateAttachmentNode(doc, node, attachmentNodeName, at);
            }
            objNode.AppendChild(node);
        }

        /// <summary>
        /// Appends a single attachment node.
        /// </summary>
        static void ObjectGenerateAttachmentNode(XmlDocument doc, XmlNode objNode, string attachmentNodeName, Attachment attachment)
        {
            var node = doc.CreateElement(attachmentNodeName);

            var guid = doc.CreateElement("Guid");
            guid.InnerText = attachment.Guid.ToString();
            var name = doc.CreateElement("Name");
            name.InnerText = attachment.Name.ToString();
            node.AppendChild(guid);
            node.AppendChild(name);
            objNode.AppendChild(node);
        }


        /// <summary>
        /// An internal method that generates an entity node and apprend it to the xmldocument root element passed to it.
        /// </summary>
        static void XmlGenerateEntityNode(XmlDocument doc, XmlNode ObjNode, string nodename, string attributeName, string attributeValue)
        {
            var node = doc.CreateElement(nodename);
            var nodechild = doc.CreateElement(nodename);

            var attribute = doc.CreateAttribute(attributeName);
            attribute.Value = attributeValue;

            nodechild.Attributes.Append(attribute);
            node.AppendChild(nodechild);
            ObjNode.AppendChild(node);
        }

        /// <summary>
        /// Loops through the custom fields and prepares (then appends) the whole XML portion dealing with them.
        /// </summary>
        static void ObjectGenerateCustomFieldsXml(XmlDocument doc, XmlNode objNode, IEnumerable<CustomField> customFields)
        {
            foreach (var cf in customFields)
            {
                XmlNode cfnode = null;
                cfnode = doc.CreateElement("Custom_Field");
                var attid = doc.CreateAttribute("id");
                attid.Value = cf.Id.ToString();
                cfnode.Attributes.Append(attid);

                bool hascustomfields = false;

                int cfocount = cf.Options.Count;

                if (cfocount > 0)
                {
                    bool haschild = false;

                    foreach (var cfo in cf.Options)
                    {
                        XmlNode cfonode = doc.CreateElement("Option");
                        var cfoattid = doc.CreateAttribute("id");
                        cfoattid.Value = cfo.Id.ToString();
                        cfonode.Attributes.Append(cfoattid);

                        if (cfo.Selected == true)
                        {
                            var attSel = doc.CreateAttribute("selected");
                            attSel.Value = "true";
                            cfonode.Attributes.Append(attSel);
                            haschild = true;
                        }

                        cfnode.AppendChild(cfonode);
                    }

                    if (haschild || cf.FlagToDelete)
                    {
                        hascustomfields = true;
                    }

                }
                else
                {
                    if (cf.FlagToDelete)
                    {
                        hascustomfields = true;
                        cf.Value = null;
                    }
                    else if (cf.Value != null)
                    {
                        hascustomfields = true;
                        cfnode.InnerText = cf.Value;
                    }
                }

                if (cf.FlagToDelete)
                {
                    hascustomfields = true;
                }
                if (hascustomfields == true)
                {
                    XmlAttribute atid = doc.CreateAttribute("id");
                    atid.Value = cf.Id.ToString();
                    cfnode.Attributes.Append(atid);
                    objNode.AppendChild(cfnode);
                }
            }
        }
    }
}
