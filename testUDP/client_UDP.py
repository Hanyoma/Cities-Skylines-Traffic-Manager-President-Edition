import socket
import json
import logging
import argparse
import pprint

logging.basicConfig(level=logging.INFO)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send(data):
    sock.settimeout(1)
    logging.debug("@SEND msg: %s", pprint.pformat(data))
    data_string = json.dumps(data)

    sock.sendto(data_string, ("localhost", 11000))

    try:
        response_str, srvr = sock.recvfrom(1024)
        logging.info("@SEND response_string: %s", pprint.pformat(response_str))

        response = json.loads(response_str)

    except socket.timeout:
        response = ""
        logging.warning('Request timed out')
    return response

def getState():
    data = {
    'Method': 'GETSTATE',
    'Object': {
                'Name': 'NodeId',
                'Type': 'PARAMETER',
                'Value': 0,  #// should be 0 - 3 (for the selected ids)
                'ValueType': 'System.UInt32'
                }
    }
    logging.debug('@GETSTATE data: %s', pprint.pformat(data))
    return data;

def getDensity(segment):
    data = {
            'Method': 'GETDENSITY',
            'Object':{
                        'Name': 'NodeId',
                        'Type': 'PARAMETER',
                        'Value': 0,  #// should be 0 - 3 (for the selected ids)
                        'ValueType': 'System.UInt32',
                        'Parameters':
                        [
                            {
                            'Name': 'SegmentId',
                            'Type': 'PARAMETER',
                            'Value': segment[-1],
                            'ValueType': 'System.UInt32'
                            }
                        ]
                     }
            }
    return data;

state = send(getState())
print "\n"
logging.info("State: \n%s\n", pprint.pformat(state))
for index, item in enumerate(state):
    density = send(getDensity(item))
    logging.info("GETDENSITY: %s, density: %s", item, density)
print "\n"
