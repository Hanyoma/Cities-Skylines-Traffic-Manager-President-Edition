using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Newtonsoft.Json;

using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using TrafficManager.Traffic;
using TrafficManager.TrafficLight;

namespace NetworkInterface
{
    public class Network
    {
        public static Queue<ushort> selectedNodeIds = new Queue<ushort>(4);

        public static void UpdateSelectedIds(ushort nodeId)
        {
            if (!selectedNodeIds.Contains(nodeId))
            {
                if (selectedNodeIds.Count == 4)
                {
                    selectedNodeIds.Dequeue();
                }
                selectedNodeIds.Enqueue(nodeId);
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                JsonConvert.SerializeObject(selectedNodeIds));
        }

        public static ushort SelectNodeId(int index)
        {
            if (index > 3 || index < 0)
            {
                throw new Exception("Node index must be an integer between 0 and 3!");
            }
            if ((selectedNodeIds.Count - 1) < index)
            {
                throw new Exception("Node index out of bounds; set of lights not fully selected!");
            }
            return selectedNodeIds.ElementAt(index);
        }

        public static NetNode SelectNode(int index)
        {
            ushort nodeId = SelectNodeId(index);
            return TrafficManager.UI.SubTools.ManualTrafficLightsTool.GetNetNode(nodeId);
        }

        public object HandleRequest(string jsonRequest)
        {
            object retObj = null;
            Request request;
            // parse the message according to Request formatting
            try
            {
                request = JsonConvert.DeserializeObject<Request>(jsonRequest);
            }
            catch (Exception e)
            {
                throw new Exception("Error: request not properly formatted: " + e.Message);
            }

            // got well formatted message, now process it
            if (request.Method == MethodType.GET)
            {
                retObj = GetObject(request.Object);
            }
            else if (request.Method == MethodType.SET)
            {

            }
            else if (request.Method == MethodType.EXECUTE)
            {

            }
            else if (request.Method == MethodType.GETDENSITY)
            {
                object nodeIndexObj = GetObject(request.Object);
                int nodeIndex = Convert.ToInt32(nodeIndexObj);
                List<object> parameterObjs = GetParameters(request.Object);
                int segId = Convert.ToInt32(parameterObjs[0]);
                retObj = GetSegmentDensity(nodeIndex, segId);
            }
            else if (request.Method == MethodType.GETSTATE)
            {
                object nodeIndexObj = GetObject(request.Object);
                int nodeIndex = Convert.ToInt32(nodeIndexObj);
                retObj = GetNodeState(nodeIndex);
            }
            else if (request.Method == MethodType.SETSTATE)
            {
                object nodeIndexObj = GetObject(request.Object);
                int nodeIndex = Convert.ToInt32(nodeIndexObj);
                List<object> parameterObjs = GetParameters(request.Object);
                // get segment id
                int segId = Convert.ToInt32(parameterObjs[0]);
                // get vehicle state
                string segState = Convert.ToString(parameterObjs[1]);
                RoadBaseAI.TrafficLightState vehicleState = 
                    (RoadBaseAI.TrafficLightState) Enum.Parse(
                        typeof(RoadBaseAI.TrafficLightState),
                        segState);
                // get pedestrian state
                segState = Convert.ToString(parameterObjs[1]);
                RoadBaseAI.TrafficLightState pedestrianState = 
                    (RoadBaseAI.TrafficLightState) Enum.Parse(
                        typeof(RoadBaseAI.TrafficLightState),
                        segState);
                retObj = SetNodeState(nodeIndex, segId, vehicleState, pedestrianState);
            }
            else
            {
                throw new Exception("Error: unsupported method type!");
            }

            return retObj;
        }

        public object GetSegmentDensity(int nodeIndex, int segIndex)
        {
            byte density = 0;
            ushort nodeId = SelectNodeId(nodeIndex);
            ushort segId = NetManager.instance.m_nodes.m_buffer[nodeId].GetSegment(segIndex);
            density = NetManager.instance.m_segments.m_buffer[segId].m_trafficDensity;
            return density;
        }

        public object SetNodeState(int nodeIndex, int segIndex, RoadBaseAI.TrafficLightState vehicleState, RoadBaseAI.TrafficLightState pedestrianState)
        {
            ushort nodeId = SelectNodeId(nodeIndex);
            ushort segId = NetManager.instance.m_nodes.m_buffer[nodeId].GetSegment(segIndex);

            CustomSegmentLights csls = CustomTrafficLights.GetSegmentLights(nodeId, segId);
            if (csls != null)
            {
                foreach(ExtVehicleType evt in csls.VehicleTypes)
                {
                    CustomSegmentLight csl = csls.GetCustomLight(evt);
                    if (csl != null)
                    {
                        RoadBaseAI.TrafficLightState currentState = csl.GetVisualLightState();
                        if (currentState != vehicleState)
                        {
                            if (vehicleState != csl.GetLightMain())
                                csl.ChangeLightMain();
                            if (vehicleState != csl.GetLightLeft())
                                csl.ChangeLightLeft();
                            if (vehicleState != csl.GetLightRight())
                                csl.ChangeLightRight();
                        }
                    }
                }
            }
            else
                return false;

            return true;
        }

        public object GetNodeState(int nodeIndex)
        {
            Dictionary<string, object> retObj = new Dictionary<string, object>();
            ushort nodeId = SelectNodeId(nodeIndex);
            for (int i=0; i<8; i++)
            {
                Dictionary<string, string> segDict = new Dictionary<string, string>();

                ushort segId = NetManager.instance.m_nodes.m_buffer[nodeId].GetSegment(i);
                CustomSegmentLights csls = CustomTrafficLights.GetSegmentLights(nodeId, segId);

                if (csls != null)
                {
                    foreach (ExtVehicleType evt in csls.VehicleTypes)
                    {
                        CustomSegmentLight csl = csls.GetCustomLight(evt);
                        if (csl != null)
                            segDict.Add("vehicle", csl.GetVisualLightState().ToString());
                    }
                    retObj.Add("segment" + i, segDict);
                }
            }

            return retObj;
        }

        public object GetObject(NetworkObject obj)
        {
            object retObj = null;

            // get required/dependent context now (recursively)
            Type contextType = null;
            object ctx = null;
            if (obj.Dependency != null)
            {
                ctx = GetObject(obj.Dependency);
            }
            if (ctx != null)
            {
                contextType = ctx as Type;
                if (contextType != null)
                {
                    ctx = null;
                }
                else
                {
                    contextType = ctx.GetType();
                }
                if (obj.IsStatic)
                {
                    ctx = null;
                }
            }

            // get object data now
            if (obj.Type == ObjectType.CLASS)
            {
                Type t = GetAssemblyType(obj.Assembly, obj.Name);
                if (t == null)
                {
                    throw new Exception("Couldn't get: " + obj.Name + " from assembly: " + obj.Assembly);
                }
                retObj = t;
            }
            else if (obj.Type == ObjectType.MEMBER || obj.Type == ObjectType.METHOD)
            {
                retObj = GetObjectMember(contextType, ctx, obj);
            }
            else if (obj.Type == ObjectType.PARAMETER) // do we need this type?
            {
            }
            else
            {
                throw new Exception("Usupported object type: "+obj.Type);
            }

            // set the value of the object if it exists
            if (obj.Value != null)
            {
                // need to figure out here how to decide what to do
                Type t = Type.GetType(obj.ValueType);
                if (t == null)
                {
                    t = GetAssemblyType(obj.Assembly, obj.ValueType);
                    retObj = Enum.Parse(t, obj.Value);  // won't always just be an enum...
                }
                else
                {
                    retObj = Convert.ChangeType(obj.Value, t);
                }
            }

            return retObj;
        }

        public List<object> GetParameters(NetworkObject obj)
        {
            // get parameters (if they exist)
            List<object> parameters = new List<object>();
            if (obj.Parameters != null)
            {
                for (int i = 0; i < obj.Parameters.Count; i++)
                {
                    object param = GetObject(obj.Parameters.ElementAt(i));
                    parameters.Add(param);
                }
            }
            return parameters;
        }

        public object GetObjectMember(Type contextType, object ctx, NetworkObject obj)
        {
            object retObj = null;
            // make sure we have context!
            if (contextType != null)
            {
                // get parameters (if they exist)
                List<object> parameters = GetParameters(obj);
                // now actually get the member
                MemberInfo[] mia = contextType.GetMember(obj.Name);
                foreach (var mi in mia)
                {
                    if (mi.MemberType == MemberTypes.Method)
                    {
                        MethodInfo methodInfo = (MethodInfo)mi;
                        if (methodInfo != null)
                        {
                            if (methodInfo.IsGenericMethod)
                            {
                                methodInfo = ((MethodInfo)mi).MakeGenericMethod(contextType);
                            }
                            retObj = methodInfo.Invoke(ctx, parameters.ToArray());
                        }
                        break;
                    }
                    else if (mi.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo pi = (PropertyInfo)mi;
                        if (pi != null)
                        {
                            MethodInfo methodInfo = pi.GetAccessors()[0];
                            if (methodInfo != null)
                            {
                                retObj = methodInfo.Invoke(ctx, null);
                            }
                        }
                        break;
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fi = (FieldInfo)mi;
                        if (fi != null)
                        {
                            retObj = fi.GetValue(ctx);
                        }
                        break;
                    }
                }
            }
            return retObj;
        }

        public Type GetAssemblyType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        public void SetValueFromString(object target, string propertyName, string propertyValue)
        {
            PropertyInfo pi = target.GetType().GetProperty(propertyName);
            Type t = pi.PropertyType;

            if (t.IsGenericType &&
                t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (propertyValue == null)
                {
                    pi.SetValue(target, null, null);
                    return;
                }
                t = new NullableConverter(pi.PropertyType).UnderlyingType;
            }
            pi.SetValue(target, Convert.ChangeType(propertyValue, t), null);
        }
    }
}
