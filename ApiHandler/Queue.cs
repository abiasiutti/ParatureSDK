using System;
using System.Xml;
using ParatureSDK.ApiHandler.ApiMethods;
using ParatureSDK.EntityQuery;
using ParatureSDK.ParaObjects;
using ParatureSDK.XmlToObjectParser;

namespace ParatureSDK.ApiHandler
{
    public class Queue : FirstLevelApiGetMethods<ParaObjects.Queue, QueueQuery>
    { }
}