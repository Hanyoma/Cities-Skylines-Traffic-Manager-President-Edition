import socket
import json
import logging
import argparse
import pprint

logging.basicConfig(level=logging.DEBUG)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send(msg):
    response = 0
    sock.settimeout(1)
    msg_string = json.dumps(msg)
    #sock.sendto(data_string, ("localhost", 11000))
    sock.sendto(msg_string, ("192.168.0.111", 11000))
    try:
        response_str, srvr = sock.recvfrom(1024)
        #logging.info("@SEND response_str: %s", pprint.pformat(response_str))
        response = json.loads(response_str)

    except socket.timeout:
        response = ""
        logging.warning('Request timed out')
    return response

def getLightState(IC):
    msg = {
    'Method': 'GETSTATE',
    'Object': {
                'Name': 'NodeId',
                'Type': 'PARAMETER',
                'Value': IC,  #// should be 0 - 3 (for the selected ids)
                'ValueType': 'System.UInt32'
                }
    }
    logging.debug('@GETSTATE msg: %s', pprint.pformat(msg))
    return msg;

def getDensity(IC, segment):
    msg = {
            'Method': 'GETDENSITY',
            'Object':{
                        'Name': 'NodeId',
                        'Type': 'PARAMETER',
                        'Value': IC,  #// should be 0 - 3 (for the selected ids)
                        'ValueType': 'System.UInt32',
                        'Parameters':
                        [
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': segment,
                            'ValueType': 'System.UInt32'
                            }
                        ]
                     }
            }
    return msg;

def getDensities(IC):
    msg = {
            'Method': 'GETDENSITIES',
            'Object':{
                        'Name': 'NodeId',
                        'Type': 'PARAMETER',
                        'Value': IC,  #// should be 0 - 3 (for the selected ids)
                        'ValueType': 'System.UInt32',
                        'Parameters':
                        [
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': 0,
                            'ValueType': 'System.UInt32'
                            },
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': 1,
                            'ValueType': 'System.UInt32'
                            },
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': 2,
                            'ValueType': 'System.UInt32'
                            },
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': 3,
                            'ValueType': 'System.UInt32'
                            },

                        ]
                     }
            }
    return msg;

def setLightState_All(IC, state):
    msg = {
            'Method': 'SETSTATE',
            'Object':
                        {
                        'Name': 'NodeId',
                        'Type': 'PARAMETER',
                        'Value': IC,  #// should be 0 - 3 (for the selected ids)
                        'ValueType': 'System.UInt32',
                        'Parameters':
                                    [
                                        {
                                        'Name': 'SegmentId',
                                        'Type': 'PARAMETER',
                                        'Value': '0',
                                        'ValueType': 'System.UInt32'	    },
                                        {
                                        'Name': 'VehicleState',
                                        'Type': 'PARAMETER',
                                        'Value': state['segment0']['vehicle'],
                                        'ValueType': 'System.String'	    },
                                        {
                                        'Name': 'PedestrianState',
                                        'Type': 'PARAMETER',
                                        'Value':state['segment0']['pedestrian'] ,
                                        'ValueType': 'System.String'
                                        },
                                        {
                                        'Name': 'SegmentId',
                                        'Type': 'PARAMETER',
                                        'Value': '1',
                                        'ValueType': 'System.UInt32'	    },
                                        {
                                        'Name': 'VehicleState',
                                        'Type': 'PARAMETER',
                                        'Value': state['segment1']['vehicle'],
                                        'ValueType': 'System.String'	    },
                                        {
                                        'Name': 'PedestrianState',
                                        'Type': 'PARAMETER',
                                        'Value':state['segment1']['pedestrian'] ,
                                        'ValueType': 'System.String'
                                        },
                                        {
                                        'Name': 'SegmentId',
                                        'Type': 'PARAMETER',
                                        'Value': '2',
                                        'ValueType': 'System.UInt32'	    },
                                        {
                                        'Name': 'VehicleState',
                                        'Type': 'PARAMETER',
                                        'Value': state['segment2']['vehicle'],
                                        'ValueType': 'System.String'	    },
                                        {
                                        'Name': 'PedestrianState',
                                        'Type': 'PARAMETER',
                                        'Value':state['segment2']['pedestrian'] ,
                                        'ValueType': 'System.String'
                                        },
                                        {
                                        'Name': 'SegmentId',
                                        'Type': 'PARAMETER',
                                        'Value': '3',
                                        'ValueType': 'System.UInt32'	    },
                                        {
                                        'Name': 'VehicleState',
                                        'Type': 'PARAMETER',
                                        'Value': state['segment3']['vehicle'],
                                        'ValueType': 'System.String'	    },
                                        {
                                        'Name': 'PedestrianState',
                                        'Type': 'PARAMETER',
                                        'Value':state['segment3']['pedestrian'],
                                        'ValueType': 'System.String'
                                        }
                                    ]
                        }
            }
    return msg
def setLightState(IC, state):
    msg = {
            'Method': 'SETSTATE',
            'Object':
                        {
                        'Name': 'NodeId',
                        'Type': 'PARAMETER',
                        'Value': IC,  #// should be 0 - 3 (for the selected ids)
                        'ValueType': 'System.UInt32',
                        'Parameters':
                                    [
                                        {
                                        'Name': 'SegmentId',
                                        'Type': 'PARAMETER',
                                        'Value': '0',
                                        'ValueType': 'System.UInt32'	    },
                                        {
                                        'Name': 'VehicleState',
                                        'Type': 'PARAMETER',
                                        'Value': state['segment0']['vehicle'],
                                        'ValueType': 'System.String'	    },
                                        {
                                        'Name': 'PedestrianState',
                                        'Type': 'PARAMETER',
                                        'Value':state['segment0']['pedestrian'] ,
                                        'ValueType': 'System.String'
                                        }
                                    ]
                        }
            }
    return msg


IC_id = 0

state = send(getLightState(IC_id))

print "\n"
logging.info("State: \n%s\n", pprint.pformat(state))
for index, segment in enumerate(state):
    segID = segment[-1]
    density = send(getDensity(IC_id, segID))
    logging.info("GETDENSITY: %s, density: %s", segment, density)
print "\n"

densities = send(getDensities(IC_id))
logging.info("GETDENSITIES: %s", densities['segment1'])


state = {'segment0': {'vehicle': 'Red', 'pedestrian':'Red'},
         'segment1': {'vehicle': 'Green', 'pedestrian':'Red'},
         'segment2': {'vehicle': 'Red', 'pedestrian':'Red'},
         'segment3': {'vehicle': 'Red', 'pedestrian':'Red'}}

resp = send(setLightState_All(IC_id, state))
print resp
